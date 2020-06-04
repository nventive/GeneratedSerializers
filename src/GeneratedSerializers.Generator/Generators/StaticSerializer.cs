using System;
using System.Collections.Generic;
using System.Linq;
using Uno.Extensions;
using Microsoft.CodeAnalysis;
using Uno.Equality;

namespace GeneratedSerializers
{
	public class SerializerGenerator
	{
		private const string _serializer = "_serializer";
		private const string _reader = "reader";
		private const string _firstChar = "firstChar";
		private const string _overChar = "overChar";
		private const string _writer = "writer";

		private readonly string _classFullName;
		private readonly IStaticSerializerResolver _staticSerializerResolver;
		private readonly ICollectionImplementationResolver _supportedCollections;
		private readonly IValueSerializationGenerator _valuesGenerator;
		private readonly bool _isRootImmutableDisabled;
		private readonly SourceFileMetadata _generatedCodeMeta;
		private readonly RoslynMetadataHelper _roslyn;
		private string _template;

		public SerializerGenerator(
			string classFullName,
			IStaticSerializerResolver staticSerializerResolver,
			ICollectionImplementationResolver supportedCollections,
			IValueSerializationGenerator valuesGenerator,
			bool isRootImmutableDisabled,
			RoslynMetadataHelper roslyn,
			SourceFileMetadata generatedCodeMeta)
		{
			_classFullName = classFullName;
			_staticSerializerResolver = staticSerializerResolver;
			_supportedCollections = supportedCollections;
			_valuesGenerator = valuesGenerator;
			_isRootImmutableDisabled = isRootImmutableDisabled;
			_roslyn = roslyn;
			_generatedCodeMeta = generatedCodeMeta;
		}

		public string SerializerClassName => _classFullName.Split('.').Last();

		private string Template => _template ??
			(_template = GetType().Assembly.GetManifestResourceStream("GeneratedSerializers.Templates.StaticSerializer.cs").ReadToEnd());

		public string Generate(ITypeSymbol[] types)
		{
			var @class = _classFullName.Split('.').Last();
			var @namespace = _classFullName.TrimEnd("." + @class);
			var implementations = types
				.Select(type => (_supportedCollections as IImplementationResolver).FindImplementation(type) ?? new Implemtation(type))
				.GroupBy(i => i.Implementation)
				.ToDictionary(group => group.Key, group => group.Select(g => g.Contract).ToArray());

			var collectionTypes = _supportedCollections
				.GetUnboundedSupportedTypes()
				.Where(t => !_isRootImmutableDisabled || !t.Implementation.GetDeclarationGenericFullName().Contains("Immutable"))
				.Concat(new Implemtation(_roslyn.GetArray(_roslyn.GetGenericType())))
				.GroupBy(i => i.Implementation)
				.ToDictionary(group => group.Key, group => group.Select(g => g.Contract).ToArray());

			var serializers = GetSerializers(implementations).ToArray();

			var serializer = Template
				.Replace("%FILE_HEADER%", _generatedCodeMeta.FileHeader)
				.Replace("%NAMESPACE%", @namespace)
				.Replace("%CLASS%", @class)
				.Replace("%CLASS_ATTRIBUTES%", _generatedCodeMeta.ClassAttributes)
				.Replace("%SERIALIZER_FACTORIES_MAP%", GetSerializerFactoriesMap(serializers))
				.Replace("%SERIALIZER_FACTORIES%", GetSerializerFactories(serializers))
				.Replace("%GENERIC_SERIALIZERS%", GetCollectionGenericSerializers(collectionTypes));

			return serializer;
		}

		private string GetSerializerFactoriesMap(SerializerInfo[] serializers)
		{
			return $@"new Dictionary<string, SerializerFactory>({serializers.Length})
				{{
					{
						serializers
							.Select(info => $@"{{""{info.Type.GetFullMetadataName()}"", new SerializerFactory({info.ResolveSerializer()})}}")
							.JoinBy(",\r\n")
					}
				}};";
		}

		private string GetSerializerFactories(SerializerInfo[] serializers)
		{
			return serializers
				.Distinct(FuncEqualityComparer<SerializerInfo>.Create(info => info.Name))
				.Select(info => info.BuildSerializer())
				.Trim()
				.JoinBy(Environment.NewLine);
		}

		private string GetCollectionGenericSerializers(IDictionary<ITypeSymbol, ITypeSymbol[]> collectionTypes)
		{
			return collectionTypes
				.Select(kvp => GetCollectionGenericSerializer(kvp.Key, kvp.Value))
				.JoinBy(Environment.NewLine);
		}

		private string GetCollectionGenericSerializer(ITypeSymbol unboundedImplementation, params ITypeSymbol[] unboundedContracts)
		{
			var className = GetGenericSerializerName(unboundedImplementation);

			var generic = _roslyn.GetGenericType("T");
			var implementation = _roslyn.ConstructFromUnbounded(unboundedImplementation, generic);
			var contracts = unboundedContracts.Select(contract => _roslyn.ConstructFromUnbounded(contract, generic));

			var implementationName = implementation.GetDeclarationGenericName();
			var contractsNames = contracts.Select(contract => contract.GetDeclarationGenericFullName()).ToArray();
			var enumerableName = implementation.IsCollectionOfKeyValuePairOfString()
				? "IEnumerable<System.Collections.Generic.KeyValuePair<string, T>>"
				: "IEnumerable<T>";

			var context = new GeneratorContext(
				_roslyn,
				new ReadContext(_reader, _firstChar, _overChar),
				new WriteContext(_writer),
				new GenericSerializerGenerator(_serializer));

			return $@"
				{_generatedCodeMeta.ClassAttributes}
				private sealed class {className}<T> : IStaticSerializer, {contractsNames.Select(t => $"IStaticSerializer<{t}>").JoinBy(", ")}
				{{
					private readonly IStaticSerializer<T> {_serializer};

					public {className}(IStaticSerializer<T> serializer)
					{{
						{_serializer} = serializer;
					}}

					object IStaticSerializer.Deserialize(JsonReader reader, char firstChar, out char? overChar) => DeserializeCore(reader, firstChar, out overChar);
					void IStaticSerializer.Serialize(JsonWriter writer, object value) => SerializeCore(writer, ({enumerableName}) value);
					{
						contractsNames
							.Select(t => $@"
								{t} IStaticSerializer<{t}>.Deserialize(JsonReader reader, char firstChar, out char? overChar) => ({t}) DeserializeCore(reader, firstChar, out overChar);
								void IStaticSerializer<{t}>.Serialize(JsonWriter writer, {t} value) => SerializeCore(writer, value);")
							.JoinBy(Environment.NewLine)
					}

					private {implementationName} DeserializeCore(JsonReader {_reader}, char {_firstChar}, out char? {_overChar})
					{{
						var result = default({implementationName});
						{_valuesGenerator.GetRead("result", implementation, context)}
						return result;
					}}

					private void SerializeCore(JsonWriter {_writer}, {enumerableName} value)
					{{
						{_valuesGenerator.GetWrite(null, "value", implementation, context)}
					}}
				}}";
		}

		private static string GetGenericSerializerName(ITypeSymbol implementation) => $"Generic_{(implementation is IArrayTypeSymbol ? "Array" : implementation.Name)}_Serializer";

		private IEnumerable<SerializerInfo> GetSerializers(IDictionary<ITypeSymbol, ITypeSymbol[]> types)
		{
			foreach (var typeValuePair in types)
			{
				var implementation = typeValuePair.Key;
				var implementationName = implementation.GetDeclarationGenericFullName();
				var contracts = typeValuePair.Value;

				string serializerName, resolveSerializer;

				ITypeSymbol type;
				if (implementation.IsDictionary(out type)
					|| implementation.IsCollection(out type))
				{
					var resolveItemSerializer = _staticSerializerResolver.GetResolve(type);

					serializerName = $"SerializerOf_{implementation.GetSerializedGenericFullName()}";
					resolveSerializer = $@"new {GetGenericSerializerName(implementation)}<{type.GetDeclarationGenericFullName()}>({resolveItemSerializer})";
				}
				else if (_staticSerializerResolver.IsResolvable(implementation))
				{
					serializerName = $"SerializerOf_{implementation.GetSerializedGenericFullName()}";
					resolveSerializer = _staticSerializerResolver.GetResolve(implementation);
				}
				else
				{
					throw new InvalidOperationException($"Cannot get resolution code for {type.GetDeclarationGenericFullName()}.");
				}

				foreach (var contract in contracts)
				{
					yield return new SerializerInfo(serializerName, contract, resolveSerializer);
				}
			}
		}

		// A value serializer which redirecte all values to the inner serializer
		private class GenericSerializerGenerator : IValueSerializationGenerator
		{
			private readonly string _serializer;

			public GenericSerializerGenerator(string serializer)
			{
				_serializer = serializer;
			}

			public string GetRead(string target, IPropertySymbol targetProperty, IValueSerializationGeneratorContext context) => GetRead(
				$"{target}.{targetProperty.Name}",
				targetProperty.Type,
				context);

			public string GetWrite(string sourceName, string source, IPropertySymbol sourceProperty, IValueSerializationGeneratorContext context) => GetWrite(
				sourceName,
				$"{source}.{sourceProperty.Name}",
				sourceProperty.Type,
				context);

			public string GetRead(string target, ITypeSymbol targetType, IValueSerializationGeneratorContext context)
			{
				return $"{target} = {_serializer}.Deserialize({context.Read.Reader}, {context.Read.FirstChar}, out {context.Read.OverChar});";
			}

			public string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, IValueSerializationGeneratorContext context)
			{
				return $@"{_serializer}.Serialize({context.Write.Writer}, {sourceCode});";
			}
		}

		private class SerializerInfo
		{
			private readonly string _getSerializer;

			public SerializerInfo(string name, ITypeSymbol type, string getSerializer)
			{
				_getSerializer = getSerializer;
				Name = name;
				Type = type;
			}

			/// <summary>
			/// Name of the serializer
			/// </summary>
			public string Name { get; }

			/// <summary>
			/// Type supported by the target serializer
			/// </summary>
			public ITypeSymbol Type { get; }

			public string ResolveSerializer() => $"Get_{Name}";

			public string BuildSerializer() => $"private static IStaticSerializer Get_{Name}() => {_getSerializer};\r\n";
		}
	}
}
