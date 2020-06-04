#if DEBUG
//#define CONTEXT_COMPILATION_DEBUG // Uncomment this if you need to get the status of the compilation BEFORE running the generator.
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Xml.Serialization;
using Uno.Extensions;
using Uno.Logging;
using Microsoft.Build.Execution;
using Microsoft.CodeAnalysis;
using Uno.SourceGeneration;

namespace GeneratedSerializers
{
#if CONTEXT_COMPILATION_DEBUG
	[GenerateAfter("Uno.ImmutableGenerator")]
	[GenerateBefore("GeneratedSerializers.StaticSerializersGeneration.SerializerGenerationTask")]
	public class BeforeSerializerGenerationTask : SourceGenerator
	{
		public override void Execute(SourceGeneratorContext context)
		{
			var compilation = context.Compilation;
			var diags = compilation.GetDiagnostics();
			Debugger.Launch();
		}
	}
#endif

	[GenerateAfter("Uno.ImmutableGenerator")]
	public class SerializerGenerationTask : SourceGenerator
	{
		private const string StaticSerializationMsBuildProperty = "StaticSerializationConfigFile";

		private static readonly string[] _nonGeneratedNameSpaces =
		{
			"System"
		};

		private readonly List<Tuple<string, string>> _generatedFiles = new List<Tuple<string, string>>();

		private SerializerGenerationConfiguration _config;
		private ITypeSymbol[] _allNeededTypes;
		private ITypeSymbol[] _rootTypes;
		private Dictionary<ITypeSymbol, TypedConstant> _fallbackValueOverrides = new Dictionary<ITypeSymbol, TypedConstant>();
		private ITypeSymbol[] _customSerializationAttributeTypes;
		private ICollectionImplementationResolver _collections;
		private StaticJsonSerializerGenerator _typeGenerator;
		private StaticJsonBuilderSerializerGenerator _builderGenerator;
		private ValueTypeSerializersGenerator _valueTypeGenerator;
		private StaticJsonCustomDeserializerGenerator _customSerializerGenerator;
		private SerializerGenerator _serializerGenerator;
		private ModuleGenerator _moduleGenerator;
		private RoslynMetadataHelper _roslynHelper;
		private SerializationType _serializationTypeInstance;
		private string _serializerClassFullName;
		private Enums _enumsGenerator;

		private string _configFile;
		private string[] _analyzerSuppressions;

		private Project _project;
		private ProjectInstance _projectInstance;
		private Compilation _compilation;

		private Dictionary<INamedTypeSymbol, INamedTypeSymbol> _knownCustomDeserializers = new Dictionary<INamedTypeSymbol, INamedTypeSymbol>();

		public override void Execute(SourceGeneratorContext context)
		{
			_project = context.Project;
			_projectInstance = context.GetProjectInstance();
			_compilation = context.Compilation;

			// Debugger.Launch();

			GenerateSerializers();

			foreach (var files in _generatedFiles)
			{
				context.AddCompilationUnit(files.Item1, files.Item2);
			}
		}

		private bool GenerateSerializers()
		{
			ReadBuildSettings();

			// Configure
			if (ReadConfig())
			{
				// Open dll and find type for configured types and nested types
				ResolveTypes();

				// Instance generator for all process
				ConfigureGenerators();

				// Generate custom deserializers for all properties marked with attribute
				GenerateCustomDeserializers();

				GenerateEnums();

				// Generate serializer for value types (like int, int?)
				GenerateValueTypeSerializer();
				
				// Generate serializers for all needed types
				GenerateTypeDeserializers();

				// Generate implementation of ISerializer (and ISerializerProvider) for configured types
				GenerateSerializer();

				// Generate module (resgitartion of ISerializer)
				GenerateModule();

				return true;
			}

			return false;
		}

		private void ReadBuildSettings()
		{
			var configFileName = _projectInstance.GetProperty(StaticSerializationMsBuildProperty)?.EvaluatedValue;

			_configFile = _projectInstance
				.GetItems("None")
				.FirstOrDefault(i => i.EvaluatedInclude.EndsWith(configFileName))
				?.EvaluatedInclude;

			if (_configFile == null)
			{
				if (_projectInstance
					.GetItems("Content")
					.Any(i => i.EvaluatedInclude.EndsWith(configFileName)))
				{
					throw new Exception($"The [{configFileName}] build action in {Path.GetFileName(_projectInstance.FullPath)} must be set to [None].");
				}
			}

			_analyzerSuppressions = _projectInstance
				.GetItems("StaticSerializerAnalyzerSuppressions")
				.Select(i => i.EvaluatedInclude)
				.ToArray();
		}

		private bool ReadConfig()
		{
			// Ensure config file exist
			if (!File.Exists(_configFile))
			{
				return false;
			}

			// Deserialize configuration file
			this.Log().Debug($"Generate static deserializer using configuration file {_configFile}");
			try
			{
				using (var fs = File.OpenRead(_configFile))
				{
					// TODO: Validate config
					_config = (SerializerGenerationConfiguration)new XmlSerializer(typeof(SerializerGenerationConfiguration)).Deserialize(fs);
					_serializerClassFullName = $"{_config.SerializersNameSpace}.{_config.SerializationType}StaticSerializer";
					_config.ModuleClassName = _config.ModuleClassName ?? "Module";
				}
			}
			catch (Exception e)
			{
				throw new Exception("Invalid static deserializer configuration file ({0})".InvariantCultureFormat(_configFile), e);
			}

			return true;
		}

		private void ResolveTypes()
		{
			var additionalTypes = _config.Entities.Safe().Concat(WellKnownTypes.Types).ToArray();

			// Augment the current compilation by adding types to compilation
			_roslynHelper = new RoslynMetadataHelper(_compilation,
				_project,
				additionalTypes
			);

			// Change current compilation for the new one
			_compilation = _roslynHelper.Compilation;

			_serializationTypeInstance = _config.CreateInstance(_roslynHelper);

			_collections = new CollectionImplementationResolver(_roslynHelper);

			// Prepare known custom deserializers
			_knownCustomDeserializers.Add(
				(INamedTypeSymbol)_roslynHelper.GetTypeByFullName("GeneratedSerializers.JwtData`1"),
				(INamedTypeSymbol)_roslynHelper.GetTypeByFullName("GeneratedSerializers.JwtDataDeserializer`1"));

			SymbolExtensions.KnownCustomDeserializers = _knownCustomDeserializers;

			// Extract types declared by using the [assembly: JsonSerializableType(typeof(<some Type>))] attribute
			var jsonSerializableTypesFromAttributes =
				//Types in current assembly
				GetSerializableTypesFromAttribute(_compilation.Assembly)
				//Types in referenced assemblies
				.Concat(_compilation.Assembly
					.Modules
					.First()
					.ReferencedAssemblySymbols
					.SelectMany(GetSerializableTypesFromAttribute)
				);

			// Extract types declared in the configuration file (SerializationConfig.xml) -- that's a legacy feature
			var typesFromConfig = _config
				.Entities
				.Safe()
				.Select(typeName =>
				{
					var type = _roslynHelper.FindTypeByFullName(typeName);

					if (type == null)
					{
						throw new TypeAccessException("Cannot resolve type {0}.".InvariantCultureFormat(typeName));
					}

					return type;
				});

			// Join the 2 lists into one
			var configuredTypes = typesFromConfig
				.Concat(jsonSerializableTypesFromAttributes)
				.Distinct()
				.ToList();

			// Unwrap Collections & Dictionaries...
			var simplifiedConfiguredTypes = configuredTypes
				.Select(t => t.DictionnaryOf() ?? t.EnumerableOf() ?? t.NullableOf() ?? t)
				.Distinct()
				.ToArray();

			// Find all types to generate a StaticDeserializer for (include nested types)
			var allNeededTypes = simplifiedConfiguredTypes
				.Concat(_serializationTypeInstance
					.StaticDeserializerPropertyFinder
					.GetNestedTypes(simplifiedConfiguredTypes)
					.Where(type => _nonGeneratedNameSpaces.None(ns => type.ContainingNamespace?.ToDisplayString().Equals(ns, StringComparison.OrdinalIgnoreCase) ?? false)))
				.ToList();

			foreach (var t in allNeededTypes.ToArray())
			{
				var attr = t.FindAttribute("Uno.ImmutableBuilderAttribute");
				if (attr == null)
				{
					// no builder for this one
					continue;
				}

				if (!(attr.ConstructorArguments[0].Value is ITypeSymbol builderType))
				{
					continue; // That would bea weird situation.
				}

				// Add the builder to type to deserialize (neededtype + root types)
				configuredTypes.Add(builderType);
				allNeededTypes.Add(builderType);
			}

			_allNeededTypes = allNeededTypes.ToArray();

			// Check if configuration restrict the generation to configured types
			if (!GetForceGenerateCollectionSerializable(_compilation.Assembly))
			{
				var nullable = (INamedTypeSymbol)_roslynHelper.GetTypeByFullName("System.Nullable`1");

				var collectionTypes = _collections
					.GetUnboundedSupportedTypes()
					.Where(t => !_config.IsImmutablesAtRootDisabled || !t.Implementation.GetDeclarationGenericFullName().Contains("Immutable"))
					.Concat(new Implemtation(_roslynHelper.GetArray(_roslynHelper.GetGenericType())));

				var typesToGenerate = new List<ITypeSymbol>();

				// Add Nullable<T> version for enums
				typesToGenerate.AddRange(_allNeededTypes
					.SelectMany(type => type.TypeKind == TypeKind.Enum
						? new[] { type, nullable.Construct(type) }
						: new[] { type }));

				// Add all "primitives" types (value types + string)
				typesToGenerate.AddRange(ValueTypeSerializersGenerator.Types.Select(_roslynHelper.GetTypeByFullName));

				// Add all permutations collection/type in collection<type>
				typesToGenerate.AddRange(typesToGenerate
					.SelectMany(type => collectionTypes
						.Select(colImp => _roslynHelper.ConstructFromUnbounded(colImp.Contract, type)))
					.ToArray());

				// That's all
				_rootTypes = typesToGenerate.ToArray();
			}
			else
			{
				_rootTypes = configuredTypes.ToArray();
			}

			//Find all CustomDeserializer Types present (include nested types)
			_customSerializationAttributeTypes = _serializationTypeInstance
				.CustomDeserializerPropertyFinder
				.GetNestedCustomDeserializerTypes(simplifiedConfiguredTypes)
				.Concat(simplifiedConfiguredTypes.Select(type => type.FindCustomDeserializerType()).Trim())
				.Distinct()
				.ToArray();
		}

		private IEnumerable<ITypeSymbol> GetSerializableTypesFromAttribute(IAssemblySymbol assembly)
		{
			return assembly.GetAttributes()
				.Where(a => a.AttributeClass.Name == "JsonSerializableTypeAttribute")
				.Select(a =>
				{
					if (a.ConstructorArguments[0].Value is ITypeSymbol type && a.ConstructorArguments.Length == 2)
					{
						_fallbackValueOverrides[type] = a.ConstructorArguments[1];
					}

					return a.ConstructorArguments[0].Value as ITypeSymbol;
				})
				.Distinct()
				.Trim();
		}

		private static bool GetForceGenerateCollectionSerializable(IAssemblySymbol assembly)
		{
			var attributes = assembly.GetAttributes()
				.Where(a => a.AttributeClass.Name == "JsonSerializationConfigurationAttribute")
				.Select(a => 
				{
					bool.TryParse(a.NamedArguments.FirstOrDefault(kvp => kvp.Key == "GenerateOnlyRegisteredTypes").Value.Value?.ToString(), out var value);
					return value;
				})
				.ToArray();
			
			//Don't use single. We want to have a clear error message.
			var attributesCount = attributes.Length;
			if (attributesCount == 0)
			{
				throw new Exception("Static Serializer must have JsonSerializationConfigurationAttribute, please add it to SerializableTypes.cs");
			}
			else if (attributesCount > 1)
			{
				throw new Exception("JsonSerializationConfigurationAttribute must be set only one time");
			}

			return attributes.First();
		}

		private void ConfigureGenerators()
		{
			_typeGenerator = (StaticJsonSerializerGenerator)_serializationTypeInstance.GetGenerator(_config, _roslynHelper, GetMetadata<StaticJsonSerializerGenerator>());
			_builderGenerator = (StaticJsonBuilderSerializerGenerator)_serializationTypeInstance.GetBuilderGenerator(_config, _roslynHelper, GetMetadata<StaticJsonSerializerGenerator>()); ;
			_customSerializerGenerator = (StaticJsonCustomDeserializerGenerator)_serializationTypeInstance.GetCustomSerializerGenerator(_config, _serializerClassFullName, GetMetadata<StaticJsonCustomDeserializerGenerator>());
			_enumsGenerator = new Enums(_config.SerializersNameSpace, _config.UseTryParseOrDefault, _config.IsMissingFallbackOnEnumsAllowed, GetMetadata<Enums>(), _serializationTypeInstance.PropertyFinder, _fallbackValueOverrides);
			_valueTypeGenerator = new ValueTypeSerializersGenerator(_config.SerializersNameSpace, _roslynHelper, _config.UseTryParseOrDefault, GetMetadata<ValueTypeSerializersGenerator>());

			_serializerGenerator = new SerializerGenerator(
				_serializerClassFullName,
				new CompositeStaticSerializerResolver(_valueTypeGenerator, _enumsGenerator, _customSerializerGenerator, _builderGenerator, _typeGenerator),
				_collections,
				new JsonCollectionGenerator(_collections),
				_config.IsImmutablesAtRootDisabled,
				_roslynHelper,
				GetMetadata<SerializerGenerator>());
			_moduleGenerator = new ModuleGenerator(_config.SerializersNameSpace, GetMetadata<ModuleGenerator>());
		}

		public SourceFileMetadata GetMetadata<T>()
		{
			var generatorVersion = typeof(T).Assembly.GetVersionNumber();

			var classAttributes = new IndentedStringBuilder();
			classAttributes.AppendLine($"[System.CodeDom.Compiler.GeneratedCode(\"StaticSerializerGenerator\", \"{generatorVersion}\")]");
			AnalyzerSuppressionsGenerator.Generate(classAttributes, _analyzerSuppressions);
			classAttributes.AppendLine();

			return new SourceFileMetadata
			{
				FileHeader = $@"
					// <autogenerated />
					// WARNING : THIS FILE HAS BEEN GENERATED BY A TOOL DO NOT UPDATE MANUALLY
					//
					//		Tool : {typeof(T).AssemblyQualifiedName}
					//		User : {WindowsIdentity.GetCurrent()?.Name ?? "UNKNOWN"}@{Environment.MachineName}
					//

#pragma warning disable
				",
				ClassAttributes = classAttributes.ToString()
			};
		}

		private void Write(string fileName, string content)
		{
			_generatedFiles.Add(Tuple.Create(fileName, content));
		}

		private void GenerateCustomDeserializers()
		{
			_customSerializationAttributeTypes
#if !DEBUG
				.AsParallel()
#endif
				.ForEach(type =>
				{
					if (type.IsAbstract)
					{
						throw new NotSupportedException("Cannot generate custom deserializers for abstract type [{0}]".InvariantCultureFormat(type));
					}

					if ((type as INamedTypeSymbol)?.IsUnboundGenericType ?? true)
					{
						throw new NotSupportedException("Cannot generate custom deserializers for generic type definition [{0}]".InvariantCultureFormat(type));
					}

					var serializedName = type.GetSerializedGenericFullName() + ".g.i.cs";

					try
					{
						// Generate files
						Write(type.GetSerializedGenericFullName(), _customSerializerGenerator.Generate(type));
					}
					catch (Exception e)
					{
						throw new Exception($"Failed to generate {serializedName}. {e}");
					}
				}
				);
		}

		private void GenerateEnums()
		{
			var x = _allNeededTypes
				.Where(type => type.TypeKind == TypeKind.Enum)
#if !DEBUG
				.AsParallel()
#endif
				.ForEach(type =>
				{
					// Generate files
					Write(type.GetSerializedGenericFullName(), _enumsGenerator.Generate(type));
				})
				.ToList();
		}

		private void GenerateTypeDeserializers()
		{
			_allNeededTypes
				// filter system namespace because they are managed by GenerateValueTypeSerializer
				.Where(type => _nonGeneratedNameSpaces.None(ns => type.ContainingNamespace?.ToDisplayString().Equals(ns, StringComparison.OrdinalIgnoreCase) ?? false))
				// Exclude the types with custom deserialization, since they will handled by their own deserializers
				.Where(type => type.TypeKind != TypeKind.Enum && type.FindCustomDeserializerType() == null)
#if !DEBUG
				.AsParallel()
#endif
				.ForEach(type =>
				{
					if (type.IsAbstract)
					{
						throw new NotSupportedException("Cannot generate serializers for abstract type [{0}]".InvariantCultureFormat(type));
					}

					// Generate files
					var generator = _builderGenerator.IsResolvable(type) ? (ISerializerGenerator) _builderGenerator : _typeGenerator;
					Write(type.GetSerializedGenericFullName() + "Deserializer", generator.Generate(type));
				});
		}

		private void GenerateValueTypeSerializer()
		{
			Write("System_ValueTypes_Serializers", _valueTypeGenerator.Generate());
		}

		private void GenerateSerializer()
		{
			Write(_serializerGenerator.SerializerClassName, _serializerGenerator.Generate(_rootTypes));
		}

		private void GenerateModule()
		{
			Write(_config.ModuleClassName, _moduleGenerator.Generate(_serializerGenerator.SerializerClassName, _config.ModuleClassName));
		}
	}
}
