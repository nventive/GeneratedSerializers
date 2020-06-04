using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	/// <summary>
	/// Generator for types that are directly readable from the JsonReader
	/// </summary>
	public class JsonReaderGenerator : IValueSerializationGenerator
	{
		private readonly IDictionary<string, string> _lookups;

		private static IDictionary<string, string> GenerateReadLookup()
		{
			return new Dictionary<string, string>
			{
				{typeof(string).ToString(), "ReadString"},

				{typeof(long).ToString(), "ReadLong"},
				{typeof(int).ToString(), "ReadInteger"},
				{typeof(short).ToString(), "ReadShort"},
				{typeof(ulong).ToString(), "ReadUnsignedLong"},
				{typeof(uint).ToString(), "ReadUnsignedInteger"},
				{typeof(ushort).ToString(), "ReadUnsignedShort"},
				{typeof(byte).ToString(), "ReadByte"},
				{typeof(double).ToString(), "ReadDouble"},
				{typeof(float).ToString(), "ReadFloat"},
				{typeof(decimal).ToString(), "ReadDecimal"},
				{typeof(bool).ToString(), "ReadBoolean"},

				{$"System.Nullable<{typeof(long).ToString()}>", "ReadNullableLong"},
				{$"System.Nullable<{typeof(int).ToString()}>", "ReadNullableInteger"},
				{$"System.Nullable<{typeof(short).ToString()}>", "ReadNullableShort"},
				{$"System.Nullable<{typeof(ulong).ToString()}>", "ReadNullableUnsignedLong"},
				{$"System.Nullable<{typeof(uint).ToString()}>", "ReadNullableUnsignedInteger"},
				{$"System.Nullable<{typeof(ushort).ToString()}>", "ReadNullableUnsignedShort"},
				{$"System.Nullable<{typeof(byte).ToString()}>", "ReadNullableByte"},
				{$"System.Nullable<{typeof(double).ToString()}>", "ReadNullableDouble"},
				{$"System.Nullable<{typeof(float).ToString()}>", "ReadNullableFloat"},
				{$"System.Nullable<{typeof(decimal).ToString()}>", "ReadNullableDecimal"},
				{$"System.Nullable<{typeof(bool).ToString()}>", "ReadNullableBoolean"},
			};
		}

		private static IDictionary<string, string> GenerateTryReadLookup()
		{
			return new Dictionary<string, string>
			{
				{typeof(string).ToString(), "TryReadString"},

				{typeof(long).ToString(), "TryReadLong"},
				{typeof(int).ToString(), "TryReadInteger"},
				{typeof(short).ToString(), "TryReadShort"},
				{typeof(ulong).ToString(), "TryReadUnsignedLong"},
				{typeof(uint).ToString(), "TryReadUnsignedInteger"},
				{typeof(ushort).ToString(), "TryReadUnsignedShort"},
				{typeof(byte).ToString(), "TryReadByte"},
				{typeof(double).ToString(), "TryReadDouble"},
				{typeof(float).ToString(), "TryReadFloat"},
				{typeof(decimal).ToString(), "TryReadDecimal"},
				{typeof(bool).ToString(), "TryReadBoolean"},

				{$"System.Nullable<{typeof(long).ToString()}>", "TryReadNullableLong"},
				{$"System.Nullable<{typeof(int).ToString()}>", "TryReadNullableInteger"},
				{$"System.Nullable<{typeof(short).ToString()}>", "TryReadNullableShort"},
				{$"System.Nullable<{typeof(ulong).ToString()}>", "TryReadNullableUnsignedLong"},
				{$"System.Nullable<{typeof(uint).ToString()}>", "TryReadNullableUnsignedInteger"},
				{$"System.Nullable<{typeof(ushort).ToString()}>", "TryReadNullableUnsignedShort"},
				{$"System.Nullable<{typeof(byte).ToString()}>", "TryReadNullableByte"},
				{$"System.Nullable<{typeof(double).ToString()}>", "TryReadNullableDouble"},
				{$"System.Nullable<{typeof(float).ToString()}>", "TryReadNullableFloat"},
				{$"System.Nullable<{typeof(decimal).ToString()}>", "TryReadNullableDecimal"},
				{$"System.Nullable<{typeof(bool).ToString()}>", "TryReadNullableBoolean"},
			};
		}

		public JsonReaderGenerator(bool useTryParseOrDefault)
		{
			_lookups = useTryParseOrDefault 
				? GenerateTryReadLookup()
				: GenerateReadLookup();
		}

		public string GetRead(string target, IPropertySymbol targetProperty, IValueSerializationGeneratorContext context) => GetRead(
			$"{target}.{targetProperty.Name}",
			targetProperty.Type,
			context);

		public string GetRead(string target, ITypeSymbol targetType, IValueSerializationGeneratorContext context)
		{
			string method;
			if (_lookups.TryGetValue(targetType.GetDeclarationGenericFullName(), out method))
			{
				return $"{target} = {context.Read.Reader}.{method}({context.Read.FirstChar}, out {context.Read.OverChar});";
			}
			else
			{
				return null;
			}
		}

		public string GetWrite(string sourceName, string source, IPropertySymbol sourceProperty, IValueSerializationGeneratorContext context) => null;

		public string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, IValueSerializationGeneratorContext context) => null;
	}
}
