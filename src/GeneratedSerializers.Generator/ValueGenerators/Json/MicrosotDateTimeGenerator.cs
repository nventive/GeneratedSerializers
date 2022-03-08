using System;
using System.Collections.Generic;
using System.Linq;
using GeneratedSerializers.Extensions;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class MicrosotDateTimeGenerator : IValueSerializationGenerator
	{
		private readonly bool _useTryParseOrDefault;

		private static readonly string[] _supportedTypes =
			{
				typeof(DateTime).ToString(),
				typeof(DateTimeOffset).ToString()
			};

		private static readonly string _nonSupportedTypeError = $"You defined the MicrosoftDateTimeFormatAttribute on a non supported type (must be one of {_supportedTypes.SelectMany(t => new[] {t, t + "?"}).JoinBy(", ")}";

		public MicrosotDateTimeGenerator(bool useTryParseOrDefault)
		{
			_useTryParseOrDefault = useTryParseOrDefault;
		}

		public string GetRead(string target, IPropertySymbol targetProperty, IValueSerializationGeneratorContext context)
		{
			if (targetProperty.FindAttributeByShortName("MicrosoftDateTimeFormatAttribute") != null)
			{
				var value = VariableHelper.GetName("dateTime");
				var targetName = $"{target}.{targetProperty.Name}";
				var type = targetProperty.Type;
				if (_supportedTypes.Contains(type.GetDeclarationGenericFullName()))
				{
					return $@"
						string {value};
						{context.Read<string>(value)}
						{GetParseMethod(value, targetName, type)}";
				}
				else if (type.IsNullable(out type) && _supportedTypes.Contains(type.GetDeclarationGenericFullName()))
				{
					return $@"
						string {value};
						{context.Read<string>(value)}
						if (!string.IsNullOrWhiteSpace({value}))
						{{
							{GetParseMethod(value, targetName, type)}
						}}";
				}
				else
				{
					return $"\r\n #error {_nonSupportedTypeError}";
				}
			}
			else
			{
				return null;
			}
		}

		private string GetParseMethod(string value, string target, ITypeSymbol type)
		{
			if (_useTryParseOrDefault)
			{
				var result = VariableHelper.GetName(type);
				return $@"
					{type.GetDeclarationGenericFullName()} {result};
					if (GeneratedSerializers.MicrosoftDateTimeHelper.TryParse{type.Name}({value}, out {result}))
					{{
						{target} = {result};
					}}";
			}
			else
			{
				return $"{target} = GeneratedSerializers.MicrosoftDateTimeHelper.Parse{type.Name}({value});";
			}
		}

		public string GetWrite(string sourceName, string source, IPropertySymbol sourceProperty, IValueSerializationGeneratorContext context)
		{
			if (sourceProperty.FindAttributeByShortName("MicrosoftDateTimeFormatAttribute") != null)
			{
				var type = sourceProperty.Type;
				if (_supportedTypes.Contains(type.GetDeclarationGenericFullName()))
				{
					return context.Write<string>(sourceName, $"GeneratedSerializers.MicrosoftDateTimeHelper.ToString({source}.{sourceProperty.Name})");
				}
				else if(type.IsNullable(out type) && _supportedTypes.Contains(type.GetDeclarationGenericFullName()))
				{
					var value = VariableHelper.GetName(sourceProperty.Type);
					return $@"
						var {value} = {source}.{sourceProperty.Name};
						if ({value}.HasValue)
						{{
							{context.Write<string>(sourceName, $"GeneratedSerializers.MicrosoftDateTimeHelper.ToString({value}.Value)")}
						}}";
				}
				else
				{
					return $"\r\n #error {_nonSupportedTypeError}";
				}
			}
			else
			{
				return null;
			}
		}

		public string GetRead(string target, ITypeSymbol targetType, IValueSerializationGeneratorContext context) => null;

		public string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, IValueSerializationGeneratorContext context) => null;
	}
}
