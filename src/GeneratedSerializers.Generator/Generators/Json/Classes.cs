using System;
using System.Collections.Generic;
using System.Linq;
using GeneratedSerializers.Extensions;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class StaticJsonSerializerGenerator : IStaticSerializerResolver, ISerializerGenerator
	{
		private readonly string _namespace;
		private readonly IPropertyFinder _propertyFinder;
		private readonly SourceFileMetadata _generatedCodeMeta;
		private readonly Func<GeneratorContext> _propertyGenerator;
		private readonly bool _disableToUpperCtor;
		private readonly RoslynMetadataHelper _metadataHelper;

		private const string _overChar = "overChar";

		private const string _writer = "writer";
		private const string _object = "objectWriter";

		public StaticJsonSerializerGenerator(
			RoslynMetadataHelper metadataHelper,
			string @namespace,
			bool disableToUpperCtor, 
			IPropertyFinder propertyFinder,
			IEnumerable<IValueSerializationGenerator> valueGenerators,
			SourceFileMetadata generatedCodeMeta
		)
		{
			_metadataHelper = metadataHelper;
			_namespace = @namespace;
			_propertyFinder = propertyFinder;
			_generatedCodeMeta = generatedCodeMeta;
			_disableToUpperCtor = disableToUpperCtor;
			_propertyGenerator = () => new GeneratorContext(
				_metadataHelper,
				new ReadContext(SerializerConstants.ReaderParameterName, _overChar),
				new WriteContext(_writer, _object),
				valueGenerators.ToArray()
			);
		}

		public string Generate(ITypeSymbol type)
		{
			if(type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var sb = new IndentedStringBuilder();

			sb.AppendLine(_generatedCodeMeta.FileHeader);

			var defaultImportedNamespaces = new[]
			{
				"System",
				"System.Collections.Generic",
				"System.IO",
				"GeneratedSerializers",
				"Uno.Extensions",
			};

			foreach (var ns in type
				.GetTypeAndAllGenericArguments()
				.Select(t => t.ContainingNamespace?.ToDisplayString())
				.Trim()
				.Union(defaultImportedNamespaces)
				.Distinct()
				.OrderBy(ns => ns))
			{
				sb.AppendLineInvariant("using {0};", ns);
			}
			sb.AppendLine();

			using (sb.BlockInvariant("namespace {0}", _namespace))
			{
				var names = GetClasses(type);

				// instance
				sb.AppendLine(_generatedCodeMeta.ClassAttributes);
				using (sb.BlockInvariant("public sealed class {0} : StaticJsonSerializerBase<{1}>", names.SerializerName, names.TypeName))
				{
					// Ctor
					WriteConstructor(sb, names);
					sb.AppendLine();

					// Singleton
					WriteSingleton(sb, names);
					sb.AppendLine();

					WriteDeserialization(sb, names, _propertyFinder.GetWritingProperties(type).ToArray());
					sb.AppendLine();

					// Create the serialization method
					WriteSerialization(sb, names, _propertyFinder.GetReadingProperties(type).ToArray());
					sb.AppendLine();
				}
			}

			return sb.ToString();
		}

		private void WriteConstructor(IndentedStringBuilder sb, SerializerClassesName names)
		{
			if (!_disableToUpperCtor)
			{
				sb.AppendLineInvariant("// If the next line fails to compile, you're probably running an older revision of Umbrella which does not support");
				sb.AppendLineInvariant("// configurable use of the toUpper parameter. If you cannot update to the a release that supports it, set the");
				sb.AppendLineInvariant("// DisableToUpperConstructor configuration parameter to true, in the SerializationConfig.xml file.");
				sb.AppendLineInvariant("// Note that disabling this feature has a performance in impact, which adds pressure to the GC.");
				sb.AppendLineInvariant("public {0}() : base(false){{ }}", names.SerializerName);
			}
		}

		private static void WriteSingleton(IndentedStringBuilder sb, SerializerClassesName names)
		{
			sb.AppendLineInvariant("private static IStaticSerializer<{0}> _instance = null;", names.TypeName);
			sb.AppendLineInvariant("public static IStaticSerializer<{0}> Instance => _instance ?? (_instance = new {1}());", names.TypeName, names.SerializerName);
			sb.AppendLine();
		}

		private void WriteDeserialization(IndentedStringBuilder sb, SerializerClassesName names, DeserializationPropertyInfo[] propertyInfos)
		{
			// Generate the switch/case mapping manually, to be able to use OrdinalIgnoreCase comparison
			using (sb.BlockInvariant("private static readonly Dictionary<string, int> _propertyMap = new Dictionary<string, int>({0}, StringComparer.OrdinalIgnoreCase)", propertyInfos.Length))
			{
				// For each properties for this type
				foreach (var propertyInfo in propertyInfos.Select((v, i) => new {Value = v, Index = i}))
				{
					sb.AppendLineInvariant("{{ \"{0}\", {1} }},", propertyInfo.Value.PropertyName.ToUpperInvariant(), propertyInfo.Index);
				}
			}
			sb.AppendLine(";");
			sb.AppendLine();

			// Read property method (switch case over property names)
			using (sb.BlockInvariant("protected override void ReadProperty({0} {1}, string {2}, JsonReader {3}, out char? {4})",
				names.TypeName,
				SerializerConstants.EntityParameterName,
				SerializerConstants.PropertyNameParameterName,
				SerializerConstants.ReaderParameterName,
				_overChar))
			{
				using (sb.BlockInvariant("switch (_propertyMap.UnoGetValueOrDefault({0}, -1))", SerializerConstants.PropertyNameParameterName))
				{
					// For each properties for this type
					foreach (var pair in propertyInfos.Select((v, i) => new {Value = v, Index = i}))
					{
						var propertyInfo = pair.Value;

						// Write the "case"
						sb.AppendLineInvariant("case {0}:", pair.Index);
						using (sb.Indent())
						{
							sb.AppendLineInvariant("Read{2}Value({1}, {3}, out {4});",
								String.Empty,
								SerializerConstants.EntityParameterName,
								propertyInfo.Property.Name,
								SerializerConstants.ReaderParameterName,
								_overChar);
							sb.AppendLineInvariant("break;");
						}
					}

					sb.AppendLineInvariant("default:");
					using (sb.Indent())
					{
						sb.AppendLineInvariant("{0}.SkipItem(out {1});", SerializerConstants.ReaderParameterName, _overChar);
						sb.AppendLineInvariant("break;");
					}
				}
			}
			sb.AppendLine();

			// Create the deserizalition methods
			foreach (var property in propertyInfos)
			{
				using (sb.BlockInvariant("private void Read{2}Value({0} {1}, JsonReader {3}, out char? {4})",
					names.TypeName,
					SerializerConstants.EntityParameterName,
					property.Property.Name,
					SerializerConstants.ReaderParameterName,
					_overChar))
				{
					sb.AppendLine(_propertyGenerator().GetRead(SerializerConstants.EntityParameterName, property.Property));
					sb.AppendLine();
				}
				sb.AppendLine();
			}
		}

		private void WriteSerialization(IndentedStringBuilder sb, SerializerClassesName names, DeserializationPropertyInfo[] propertyInfos)
		{
			sb.AppendLine($@"
				protected override void SerializeCore(JsonWriter {_writer}, {names.TypeName} entity)
				{{
					if (entity == null)
					{{
						{_writer}.WriteNullValue();
					}}
					else
					{{
						using(var {_object} = {_writer}.OpenObject())
						{{
							{propertyInfos.Select(property => _propertyGenerator().GetWrite(property.PropertyName, "entity", property.Property)).JoinBy(Environment.NewLine)}
						}}
					}}
				}}
				");
		}

		/// <summary>
		/// Determines if a serializer may be resolved for a given type
		/// </summary>
		public bool IsResolvable(ITypeSymbol type)
		{
			// Even if we generate collection deserializer, the type may implement IEnumerable
			// !type.IsCollection() /* Includes IDictionary */

			return !type.IsAbstract && type.TypeKind != TypeKind.Interface;
		}

		/// <summary>
		/// Get the code to use to resolve the instance of the serializer of a given type
		/// </summary>
		public string GetResolve(ITypeSymbol type)
		{
			return $"{GetClasses(type).SerializerName}.Instance";
		}

		private SerializerClassesName GetClasses(ITypeSymbol type)
		{
			var normalizedName = type.GetSerializedGenericFullName();

			return new SerializerClassesName
			{
				TypeName = type.GetDeclarationGenericName(),
				TypeFullName = type.GetDeclarationGenericFullName(),
				SerializerName = $"{normalizedName}_StaticJsonSerializer",
			};
		}

		public class SerializerClassesName
		{
			public string TypeName { get; internal set; }

			public string TypeFullName { get; internal set; }


			public string SerializerName { get; internal set; }
		}
	}
}
