using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class StaticJsonBuilderSerializerGenerator : IStaticSerializerResolver, ISerializerGenerator
	{
		private readonly RoslynMetadataHelper _metadataHelper;
		private readonly string _namespace;
		private readonly bool _disableToUpperCtor;
		private readonly IPropertyFinder _propertyFinder;
		private readonly IEnumerable<IValueSerializationGenerator> _valueGenerators;
		private readonly SourceFileMetadata _generatedCodeMeta;
		private Func<GeneratorContext> _propertyGenerator;

		private const string _overChar = "overChar";
		private const string _writer = "writer";
		private const string _object = "objectWriter";

		public StaticJsonBuilderSerializerGenerator(
			RoslynMetadataHelper metadataHelper,
			string @namespace,
			bool disableToUpperCtor,
			IPropertyFinder propertyFinder,
			IEnumerable<IValueSerializationGenerator> valueGenerators,
			SourceFileMetadata generatedCodeMeta)
		{
			_metadataHelper = metadataHelper;
			_namespace = @namespace;
			_disableToUpperCtor = disableToUpperCtor;
			_propertyFinder = propertyFinder;
			_valueGenerators = valueGenerators;
			_generatedCodeMeta = generatedCodeMeta;
			_propertyGenerator = () => new GeneratorContext(
				_metadataHelper,
				new ReadContext(SerializerConstants.ReaderParameterName, _overChar),
				new WriteContext(_writer, _object),
				valueGenerators.ToArray()
			);
		}

		public string Generate(ITypeSymbol type)
		{
			if (type == null)
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
				using (sb.BlockInvariant($"public sealed class {names.SerializerName} : IStaticSerializer<{names.TypeName}>"))
				{
					// Ctor
					WriteConstructor(sb, names);
					sb.AppendLine();

					// Singleton
					WriteSingleton(sb, names);
					sb.AppendLine();

					WriteDeserialization(sb, names);
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
			sb.AppendLine($"private {names.SerializerName}(){{ }}");
		}

		private static void WriteSingleton(IndentedStringBuilder sb, SerializerClassesName names)
		{
			sb.AppendLineInvariant($"private static IStaticSerializer<{names.TypeName}> _instance = null;");
			sb.AppendLineInvariant($"public static IStaticSerializer<{names.TypeName}> Instance => _instance ?? (_instance = new {names.SerializerName}());");
			sb.AppendLineInvariant($"private static IStaticSerializer<{names.BuilderTypeFullName}> _builder => {names.BuilderSerializerName}.Instance;");
			sb.AppendLine();
		}

		private void WriteDeserialization(IndentedStringBuilder sb, SerializerClassesName names)
		{
			sb.AppendLine(
$@"object IStaticSerializer.Deserialize(JsonReader {SerializerConstants.ReaderParameterName}, char firstChar, out char? {_overChar})
{{
	return Deserialize({SerializerConstants.ReaderParameterName}, firstChar, out {_overChar});
}}

public {names.TypeName} Deserialize(JsonReader {SerializerConstants.ReaderParameterName}, char firstChar, out char? {_overChar})
{{
	Uno.IImmutableBuilder<{names.TypeName}> builderInstance = _builder.Deserialize({SerializerConstants.ReaderParameterName}, firstChar, out {_overChar});
	return builderInstance?.ToImmutable();
}}");
			sb.AppendLine();
		}

		private void WriteSerialization(IndentedStringBuilder sb, SerializerClassesName names, DeserializationPropertyInfo[] propertyInfos)
		{
			sb.AppendLine(
$@"void IStaticSerializer.Serialize(JsonWriter {_writer}, object value)
{{
	Serialize({_writer}, value as {names.TypeName});
}}

public void Serialize(JsonWriter {_writer}, {names.TypeName} entity)
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
}}");
			sb.AppendLine();
		}


		public bool IsResolvable(ITypeSymbol type)
		{
			var attr = type.FindAttribute("Uno.ImmutableBuilderAttribute");
			return attr != null;
		}

		public string GetResolve(ITypeSymbol type)
		{
			return $"{GetClasses(type).SerializerName}.Instance";
		}

		private SerializerClassesName GetClasses(ITypeSymbol type)
		{
			var builderType = type
				.FindAttribute("Uno.ImmutableBuilderAttribute")
				.ConstructorArguments[0]
				.Value as ITypeSymbol;

			return new SerializerClassesName
			{
				TypeName = type.GetDeclarationGenericName(),
				TypeFullName = type.GetDeclarationGenericFullName(),
				SerializerName = $"{type.GetSerializedGenericFullName()}_StaticJsonSerializer",
				BuilderTypeFullName = builderType.GetDeclarationGenericFullName(),
				BuilderSerializerName = $"{builderType.GetSerializedGenericFullName()}_StaticJsonSerializer",
			};
		}

		public class SerializerClassesName
		{
			public string TypeName { get; internal set; }

			public string TypeFullName { get; internal set; }

			public string SerializerName { get; internal set; }

			public string BuilderTypeFullName { get; internal set; }

			public string BuilderSerializerName { get; internal set; }
		}

	}
}
