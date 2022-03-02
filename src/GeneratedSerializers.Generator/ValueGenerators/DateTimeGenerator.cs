using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class DateTimeGenerator : IValueSerializationGenerator
	{
		private const string _invariantCulture = ", System.Globalization.CultureInfo.InvariantCulture";
		private static readonly string _timeSpan = typeof(TimeSpan).ToString();
		private static readonly string[] _supportedTypes = 
			{
				typeof(DateTime).ToString(),
				typeof(DateTimeOffset).ToString(),
				_timeSpan
			};

		private readonly bool _useTryParseOrDefault;

		public DateTimeGenerator(bool useTryParseOrDefault)
		{
			_useTryParseOrDefault = useTryParseOrDefault;
		}

		public string GetRead(string target, IPropertySymbol targetProperty, IValueSerializationGeneratorContext context)
		{
			var formatAttribute = targetProperty.FindAttributeByShortName("DateTimeFormatAttribute");

			string format = null, styles = null;
			if (formatAttribute != null)
			{
				format = (formatAttribute.NamedArguments.FirstOrDefault(a => a.Key == "Format").Value.Value
					?? formatAttribute.ConstructorArguments.FirstOrDefault(x => x.Type.Name == "String").Value) as string;

				styles = (formatAttribute.NamedArguments.FirstOrDefault(a => a.Key == "Styles").Value.Value
				    ?? formatAttribute.ConstructorArguments.FirstOrDefault(x => x.Type.Name == "DateTimeStyles").Value) as string;
			}

			return GetRead($"{target}.{targetProperty.Name}", targetProperty.Type, format, styles, context);
		}

		public string GetRead(string target, ITypeSymbol targetType, IValueSerializationGeneratorContext context) => GetRead(target, targetType, null, null, context);

		private string GetRead(string target, ITypeSymbol targetType, string formatValue, string stylesValue, IValueSerializationGeneratorContext context)
		{
			if (_supportedTypes.Contains(targetType.GetDeclarationGenericFullName()))
			{
				var value = VariableHelper.GetName<string>();
				return $@"
					string {value};
					{context.Read<string>(value)}
					{GetParseMethod(value, target, targetType, formatValue, stylesValue, context)}";
			}
			else if (targetType.IsNullable(out targetType)
				&& _supportedTypes.Contains(targetType.GetDeclarationGenericFullName()))
			{
				var value = VariableHelper.GetName<string>();
				return $@"
					string {value};
					{context.Read<string>(value)}
					if (!string.IsNullOrWhiteSpace({value}))
					{{
						{GetParseMethod(value, target, targetType, formatValue, stylesValue, context)}
					}}";
			}
			else
			{
				return null;
			}
		}

		private string GetParseMethod(string value, string target, ITypeSymbol targetType, string formatValue, string stylesValue, IValueSerializationGeneratorContext context)
		{
			var type = targetType.GetDeclarationGenericFullName();

			string exact, format, styles;
			if (formatValue.IsNullOrWhiteSpace())
			{
				exact = string.Empty;
				format = string.Empty;
			}
			else
			{
				exact = "Exact";
				format = $", \"{formatValue}\"";
			}

			if (type == _timeSpan)
			{
				// No DateTimeStyle for TimeSpan parsing
				styles = string.Empty;
			}
			else if (stylesValue == null)
			{
				//https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/DateTime.cs
				//as default value in .net Parse() method
				styles = ", System.Globalization.DateTimeStyles.None";
			}
			else
			{
				styles = $", (System.Globalization.DateTimeStyles){stylesValue}";
			}

			if (_useTryParseOrDefault)
			{
				var result = VariableHelper.GetName(targetType);
				return $@"
					{type} {result};
					{type}.TryParse{exact}({value} {format} {_invariantCulture} {styles}, out {result});
					{target} = {result};";
			}
			else
			{
				return $@"{target} = {type}.Parse{exact}({value} {format} {_invariantCulture} {styles});";
			}
		}


		public string GetWrite(string sourceName, string source, IPropertySymbol sourceProperty, IValueSerializationGeneratorContext context)
		{
			var formatAttribute = sourceProperty.FindAttributeByShortName("DateTimeFormatAttribute");

			string format = null;
			if (formatAttribute != null)
			{
				format = (formatAttribute.NamedArguments.FirstOrDefault(a => a.Key == "Format").Value.Value
					?? formatAttribute.ConstructorArguments.FirstOrDefault(x => x.Type.Name == "String").Value) as string;
			}

			return GetWrite(sourceName, $"{source}.{sourceProperty.Name}", sourceProperty.Type, format, context);
		}

		public string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, IValueSerializationGeneratorContext context) => GetWrite(sourceName, sourceCode, sourceType, null, context);

		private string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, string format, IValueSerializationGeneratorContext context)
		{
			var formatParameter = format.IsNullOrWhiteSpace()
				? string.Empty
				: $"\"{format}\", ";
			var cultureParameter = sourceType.GetDeclarationGenericFullName().Contains("TimeSpan") && formatParameter.IsNullOrEmpty()
				? string.Empty
				: "System.Globalization.CultureInfo.InvariantCulture";

			if (_supportedTypes.Contains(sourceType.GetDeclarationGenericFullName()))
			{
				return context.Write<string>(sourceName, $"{sourceCode}.ToString({formatParameter} {cultureParameter})");
            }
			else if (sourceType.IsNullable(out sourceType) && _supportedTypes.Contains(sourceType.GetDeclarationGenericFullName()))
			{
				var value = VariableHelper.GetName(sourceType);
				return $@"
					{sourceType.GetDeclarationGenericFullName()}? {value} = {sourceCode};
					if ({value}.HasValue)
					{{
						{context.Write<string>(sourceName, $"{value}.Value.ToString({formatParameter} {cultureParameter})")}
					}}";
			}

			return null;
		}
	}
}
