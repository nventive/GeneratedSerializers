using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class EnumGenerator : IValueSerializationGenerator
	{
		private readonly Enums _enums;

		public EnumGenerator(Enums enums)
		{
			_enums = enums;
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
			if (_enums.IsResolvable(targetType))
			{
				var value = VariableHelper.GetName(targetType);
				if (targetType.IsNullable())
				{
					return $@"
						string {value};
						{context.Read<string>(value)}
						if (!string.IsNullOrEmpty({value}))
						{{
							{target} = {Enums.GetConverterName(targetType)}.FromString({value});
						}}";
				}
				else
				{
					return $@"
						string {value};
						{context.Read<string>(value)}
						{target} = {Enums.GetConverterName(targetType)}.FromString({value});";
				}
			}

			return null;
		}

		public string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, IValueSerializationGeneratorContext context)
		{
			if (_enums.IsResolvable(sourceType))
			{
				return context.Write<string>(sourceName, $"{Enums.GetConverterName(sourceType)}.ToString({sourceCode})");
			}

			return null;
		}
	}
}
