using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Uno;
using Microsoft.CodeAnalysis.CSharp;
using GeneratedSerializers.Extensions;
using GeneratedSerializers.Equality;
using GeneratedSerializers.Helpers;

namespace GeneratedSerializers
{
	/// <summary>
	/// A generator which generate static serializer for enums
	/// </summary>
	public class Enums : ISerializerGenerator, IStaticSerializerResolver
	{
		private readonly string _nameSpace;
		private readonly bool _useTryParseOrDefault;
		private readonly SourceFileMetadata _meta;
		private readonly IPropertyFinder _codeAnalyzer;
		private readonly bool _isMissingFallbackAllowed;
		private readonly IReadOnlyDictionary<ITypeSymbol, TypedConstant> _fallbackValueOverrides;

		public Enums(string @namespace, bool useTryParseOrDefault, bool isMissingFallbackAllowed, SourceFileMetadata meta, IPropertyFinder codeAnalyzer, Dictionary<ITypeSymbol, TypedConstant> fallbackValueOverrides)
		{
			_nameSpace = @namespace;
			_useTryParseOrDefault = useTryParseOrDefault;
			_isMissingFallbackAllowed = isMissingFallbackAllowed;
			_meta = meta;
			_codeAnalyzer = codeAnalyzer;
			_fallbackValueOverrides = fallbackValueOverrides;
		}

		public string Generate(ITypeSymbol type)
		{
			if (!IsResolvable(type))
			{
				throw new InvalidOperationException($"{type.GetDeclarationGenericFullName()} is not an enum.");
			}

			return $@"
			{_meta.FileHeader}

			using System;
			using System.Collections.Generic;
			using System.Linq;
			using System.Text;
			using GeneratedSerializers;

			namespace {_nameSpace}
			{{
				{GetConverter(type)}

				{GetSerializer(type, nullable: false)}

				{GetSerializer(type, nullable: true)}
			}}";
		}

		public bool IsResolvable(ITypeSymbol type) => type.TypeKind == TypeKind.Enum || (type.IsNullable(out type) && type.TypeKind == TypeKind.Enum);

		public string GetResolve(ITypeSymbol type)
		{
			if (type.TypeKind == TypeKind.Enum)
			{
				return $"{GetSerializerName(type)}.Instance";
			}
			else if (type.IsNullable(out type) && type.TypeKind == TypeKind.Enum)
			{
				return $"{GetNullableSerializerName(type)}.Instance";
			}
			else
			{
				return null;
			}
		}

		public static string GetConverterName(ITypeSymbol type) => $"{(type.NullableOf() ?? type).GetSerializedGenericFullName()}_StaticConverter";

		private static string GetSerializerName(ITypeSymbol type) => $"{type.GetSerializedGenericFullName()}_StaticSerializer";
		private static string GetNullableSerializerName(ITypeSymbol type) => $"Nullable_{type.GetSerializedGenericFullName()}_StaticSerializer";

		private string GetConverter(ITypeSymbol type)
		{
			var className = GetConverterName(type);
			var TEnum = type.GetDeclarationGenericFullName();

			string emptyCheck;
			var fallback = GetConversionFallback(type, out emptyCheck);

			return $@"
				{_meta.ClassAttributes}
				internal static class {className}
				{{
					private static readonly IDictionary<{TEnum}, string> _values = {GetValuesMap(type)};

					private static readonly IDictionary<string, {TEnum}> _reversedValues = {GetReversedValuesMap(type)};

					public static string ToString({TEnum}? value)
					{{
						return value.HasValue
							? _values[value.Value]
							: null;
					}}

					public static string ToString({TEnum} value)
					{{
						return _values[value];
					}}

					public static {TEnum} FromString(string value)
					{{
						{emptyCheck}

						{TEnum} result;
						if (value != null && _reversedValues.TryGetValue(value, out result))
						{{
							return result;
						}}
						else
						{{
							{fallback}
						}}
					}}
				}}";
		}

		private string GetSerializer(ITypeSymbol type, bool nullable)
		{
			var converter = GetConverterName(type);

			string className, TEnum, read, write;
			if (nullable)
			{
				className = GetNullableSerializerName(type);
				TEnum = $"{type.GetDeclarationGenericFullName()}?";
				read = $@"string.IsNullOrEmpty(value) 
					? default({TEnum}) 
					: {converter}.FromString(value);";
				write = $@"if (value.HasValue)
					{{
						writer.WriteStringValue({converter}.ToString(value));
					}}
					else
					{{
						writer.WriteNullValue();
					}}";
			}
			else
			{
				className = GetSerializerName(type);
				TEnum = type.GetDeclarationGenericFullName();
				read = $@"{converter}.FromString(value);";
				write = $@"writer.WriteStringValue({converter}.ToString(value));";
			}
			
			return $@"
				{_meta.ClassAttributes}
				internal sealed class {className} : IStaticSerializer<{TEnum}>
				{{
					private static IStaticSerializer<{TEnum}> _instance;
					public static IStaticSerializer<{TEnum}> Instance => _instance ?? (_instance = new {className}());

					private {className}() {{ }}

					void IStaticSerializer.Serialize(JsonWriter writer, object value) => Serialize(writer, ({TEnum}) value);
					public void Serialize(JsonWriter writer, {TEnum} value)
					{{
						{write}
					}}

					object IStaticSerializer.Deserialize(JsonReader reader, char firstChar, out char? overChar) => Deserialize(reader, firstChar, out overChar);
					public {TEnum} Deserialize(JsonReader reader, char firstChar, out char? overChar)
					{{
						var value = reader.ReadString(firstChar, out overChar);
						
						return {read}
					}}
				}}";
		}

		// From Value to string (serialization)
		private string GetValuesMap(ITypeSymbol type)
		{
			var typeName = type.GetDeclarationGenericFullName();

			string[] values;
			if (type.FindAttribute("blabla") != null)
			{
				values = type
					.GetMembers()
					.OfType<IFieldSymbol>()
					// If the enum have multiple fields with the same value (eg. BlaBlaEnum.Default = BlaBlaEnum.HellWorld), the generated dictionary will be valid
					// and will throw a TypeInitializationException or ArgumentException. So keep only the first occurence.
					.Distinct(FuncEqualityComparer<IFieldSymbol>.Create(field => field.ConstantValue))
					.Select(field => $@"{{{typeName}.{field.Name}, ""{field.ConstantValue.ToString()}""}},")
					.ToArray();
			}
			else
			{
				values = type
					.GetMembers()
					.OfType<IFieldSymbol>()
					// If the enum have multiple fields with the same value (eg. BlaBlaEnum.Default = BlaBlaEnum.HellWorld), the generated dictionary will be valid
					// and will throw a TypeInitializationException or ArgumentException. So keep only the first occurence.
					.Distinct(FuncEqualityComparer<IFieldSymbol>.Create(field => field.ConstantValue))
					.Select(field => $@"{{{typeName}.{field.Name}, ""{_codeAnalyzer.GetName(field)}""}},")
					.ToArray();
			}

			return $@"new Dictionary<{typeName}, string>({values.Length})
				{{
					{values.JoinBy(Environment.NewLine)}
				}}";
		}

		// From string to enum value (Deserialization)
		private string GetReversedValuesMap(ITypeSymbol type)
		{
			var typeName = type.GetDeclarationGenericFullName();

			var literalValues = type
				.GetMembers()
				.OfType<IFieldSymbol>()
				.Select(field => $@"{{""{_codeAnalyzer.GetName(field)}"", {typeName}.{field.Name}}},");
			var underlyingTypeValues = type
				.GetMembers()
				.OfType<IFieldSymbol>()
				.Distinct(FuncEqualityComparer<IFieldSymbol>.Create(field => field.ConstantValue))
				.Select(field => $@"{{""{field.ConstantValue.ToString()}"", {typeName}.{field.Name}}},");

			var values = literalValues.Concat(underlyingTypeValues).ToArray();

			return $@"new Dictionary<string, {typeName}>({values.Length}, System.StringComparer.OrdinalIgnoreCase)
				{{
					{values.JoinBy(Environment.NewLine)}
				}}";
		}

		private string GetConversionFallback(ITypeSymbol type, out string emptyCheck)
		{
			var TEnum = type.GetDeclarationGenericFullName();
			var fallback = type.GetFieldsWithAttributeShortName(typeof(FallbackValueAttribute).Name).FirstOrDefault();

			emptyCheck = null;

			if (_fallbackValueOverrides != null && _fallbackValueOverrides.TryGetValue(type, out var fallbackOverride))
			{
				return $"return {fallbackOverride.ToCSharpString()};";
			}
			else if (fallback != null)
			{
				return $@"//Fallback value
							return {TEnum}.{fallback.Name};";
			}
			else if (!_isMissingFallbackAllowed)
			{
				return $"\r\n#error You must define the {typeof (FallbackValueAttribute).Name} attribute for enum '{TEnum}'.";
			}
			else if (_useTryParseOrDefault)
			{
				return $"return default({TEnum});";
			}
			else
			{
				emptyCheck = $@"
					// Note: this is not the right behavior (should throw), but this specific case was altered for backward compatibility with EnumUtilities.ParseOrDefault
					if (string.IsNullOrWhiteSpace(value))
					{{
						return default({TEnum});
					}}";

				return $@"throw new FormatException(""'{{0}}' is not valid value for {TEnum}."".InvariantCultureFormat(value));";
			}
		}
	}
}
