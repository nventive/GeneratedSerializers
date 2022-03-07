using System;
using System.Collections.Generic;
using GeneratedSerializers.Extensions;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class JsonWriterGenerator : IValueSerializationGenerator
	{
		private static readonly string _stringType = typeof(string).ToString();

		private static readonly IDictionary<string, string> _supportedTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{typeof (long).ToString(), "long"},
			{typeof (int).ToString(), "long"},
			{typeof (short).ToString(), "int"},  // **** Mapped
			{typeof (ulong).ToString(), "ulong"},
			{typeof (uint).ToString(), "uint"},
			{typeof (ushort).ToString(), "ushort"},
			{typeof (byte).ToString(), "int"},  // **** Mapped
			{typeof (double).ToString(), "double"},
			{typeof (float).ToString(), "float"},
			{typeof (decimal).ToString(), "decimal"},
			{typeof (bool).ToString(), "bool"},
		};

		public string GetRead(string target, IPropertySymbol targetProperty, IValueSerializationGeneratorContext context) => null;

		public string GetRead(string target, ITypeSymbol targetType, IValueSerializationGeneratorContext context) => null;

		public string GetWrite(string sourceName, string source, IPropertySymbol sourceProperty, IValueSerializationGeneratorContext context) => GetWrite(
			sourceName,
			$"{source}.{sourceProperty.Name}",
			sourceProperty.Type,
			context);

		public string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, IValueSerializationGeneratorContext context)
		{
			string mappedType;
			if (sourceName.HasValueTrimmed())
			{
				// We are writing a property value, if the source is null, we can just ignore the whole property.

				if (_stringType.Equals(sourceType.GetDeclarationGenericFullName(), StringComparison.OrdinalIgnoreCase))
				{
					var value = VariableHelper.GetName(sourceType);
					return $@"
						var {value} = {sourceCode};
						if ({value} != null)
						{{
							{context.Write.Object}.WritePropertyName(""{sourceName}"");
							{context.Write.Writer}.WriteStringValue({value});
						}}";
				}
				else if (_supportedTypes.TryGetValue(sourceType.GetDeclarationGenericFullName(), out mappedType))
				{
					return $@"
						{context.Write.Object}.WritePropertyName(""{sourceName}"");
						{context.Write.Writer}.Write(({mappedType}){sourceCode});";
				}
				else if (sourceType.IsNullable(out sourceType) && _supportedTypes.TryGetValue(sourceType.GetDeclarationGenericFullName(), out mappedType))
				{
					var value = VariableHelper.GetName(sourceType);
					return $@"
						var {value} = {sourceCode};
						if ({value}.HasValue)
						{{
							{context.Write.Object}.WritePropertyName(""{sourceName}"");
							{context.Write.Writer}.Write(({mappedType}){value}.Value);
						}}";
				}
			}
			else
			{
				// We are writing an item of a collection or something like that. 
				// We cannot ignore null values, instead we must write the "null" keyword.

				if (_stringType.Equals(sourceType.GetDeclarationGenericFullName(), StringComparison.OrdinalIgnoreCase))
				{
					// WriteStringValue handles the "null" itself.
					return $@"{context.Write.Writer}.WriteStringValue({sourceCode});";
				}
				else if (_supportedTypes.TryGetValue(sourceType.GetDeclarationGenericFullName(), out mappedType))
				{
					return $@"{context.Write.Writer}.Write(({mappedType}){sourceCode});";
				}
				else if (sourceType.IsNullable(out sourceType) && _supportedTypes.TryGetValue(sourceType.GetDeclarationGenericFullName(), out mappedType))
				{
					var value = VariableHelper.GetName(sourceType);
					return $@"
						var {value} = {sourceCode};
						if ({value}.HasValue)
						{{
							{context.Write.Writer}.Write(({mappedType}){value}.Value);
						}}
						else
						{{
							{context.Write.Writer}.WriteNullValue();
						}}";
				}
			}

			return null;
		}
	}
}
