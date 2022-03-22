using System;
using System.Collections.Generic;
using System.Linq;
using GeneratedSerializers.Extensions;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class StaticJsonCustomDeserializerGenerator : IStaticSerializerResolver, ISerializerGenerator
	{
		private readonly string _namespace;
		private readonly SourceFileMetadata _generatedCodeMeta;
		private readonly string _serializerProviderFullName;

		public StaticJsonCustomDeserializerGenerator(string @namespace, SourceFileMetadata generatedCodeMeta, string serializerProviderFullName)
		{
			_namespace = @namespace;
			_generatedCodeMeta = generatedCodeMeta;
			_serializerProviderFullName = serializerProviderFullName;
		}

		public static string GetClassName(ITypeSymbol type) => $"{type.GetSerializedGenericFullName()}_CustomSerializer";

		public string Generate(ITypeSymbol sourceType)
		{
			var type = sourceType as INamedTypeSymbol;

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

			var entityType = GetTypeParameter(type);

			foreach (var ns in type.GetTypeAndAllGenericArguments()
				.Concat(entityType.GetTypeAndAllGenericArguments())
				.Select(t => GetTypeNamespace(t))
				.Union(defaultImportedNamespaces)
				.Distinct()
				.OrderBy(ns => ns)
				)
			{
				sb.AppendLineInvariant("using {0};", ns);
			}
			sb.AppendLine();

			using (sb.BlockInvariant("namespace {0}", _namespace))
			{
				var className = GetClassName(type);
				var serializerTypeName = type.GetDeclarationGenericName();
				var entityTypeName = entityType.GetDeclarationGenericName();

				sb.Append($@"
					{_generatedCodeMeta.ClassAttributes}
					internal sealed class {className} : IStaticSerializer<{entityTypeName}>
					{{
						private static IStaticSerializer<{entityTypeName}> _instance;
						internal static IStaticSerializer<{entityTypeName}> Instance => _instance ?? (_instance = new {className}());

						private IStaticSerializerProvider _staticSerializers;
						private ICustomTypeSerializer<{entityTypeName}> _serializer;

						private {className}()
						{{
							_serializer = new {serializerTypeName}();
							_staticSerializers = {_serializerProviderFullName}.Instance;

							if (_staticSerializers == null)
							{{
								throw new InvalidOperationException(""Static serializers are not initialized. You must use {_serializerProviderFullName} for custom deserialization."");
							}}
						}}

						void IStaticSerializer.Serialize(JsonWriter writer, object value) =>  _serializer.Write(writer, ({entityTypeName})value, _staticSerializers);

						void IStaticSerializer<{entityTypeName}>.Serialize(JsonWriter writer, {entityTypeName} value) => _serializer.Write(writer, value, _staticSerializers);

						object IStaticSerializer.Deserialize(JsonReader reader, char firstChar, out char? overChar) => _serializer.Read(reader, firstChar, out overChar, _staticSerializers);

						{entityTypeName} IStaticSerializer<{entityTypeName}>.Deserialize(JsonReader reader, char firstChar, out char? overChar) => _serializer.Read(reader, firstChar, out overChar, _staticSerializers);
					}}
					");
			}

			return sb.ToString();
		}

		private static string GetTypeNamespace(ITypeSymbol t)
		{
			var namedType = t as INamedTypeSymbol;
			if(namedType != null)
			{
				return namedType.ContainingNamespace.ToDisplayString();
			}

			var arrayType = t as IArrayTypeSymbol;
			if (arrayType != null)
			{
				return GetTypeNamespace(arrayType.ElementType);
			}

			throw new NotSupportedException($"The type {t} is not supported as a convertible type.");
		}

		private ITypeSymbol GetTypeParameter( INamedTypeSymbol type)
		{
			var typeParameter = type
				.GetAllInterfaces()
				.Where( t => IsMatch(t))
				.Select( t => t.TypeArguments.First() as ITypeSymbol)
				.FirstOrDefault();

			if(typeParameter == null)
			{
				throw new InvalidOperationException($"Unable to find an interface ICustomTypeSerializer<T> on {type}");
			}

			return typeParameter;
		}

		private static bool IsMatch(INamedTypeSymbol type)
		{
			return type.IsGenericType 
				&& type.Name.StartsWith("ICustomTypeSerializer");
		}

		public bool IsResolvable(ITypeSymbol type) => HasCustomDeserializationOnType(type);

		private bool HasCustomDeserializationOnType(ITypeSymbol type) => type.FindCustomDeserializerType() != null;

		public string GetResolve(ITypeSymbol type) => $"{GetClassName(type.FindCustomDeserializerType())}.Instance";
	}
}
