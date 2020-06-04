using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class CustomSerializerGenerator : IValueSerializationGenerator
	{
		public string GetRead(string target, IPropertySymbol targetProperty, IValueSerializationGeneratorContext context)
		{
			var serializerType = FindCustomSerializerType(targetProperty);
			if (serializerType != null)
			{
				return $@"{target}.{targetProperty.Name} = {StaticJsonCustomDeserializerGenerator.GetClassName(serializerType)}.Instance.Deserialize(
					{context.Read.Reader},
					{context.Read.FirstChar},
					out {context.Read.OverChar});";
			}

			return null;
		}

		public string GetWrite(string sourceName, string source, IPropertySymbol sourceProperty, IValueSerializationGeneratorContext context)
		{
			var serializerType = FindCustomSerializerType(sourceProperty);
			if (serializerType != null)
			{
				var value = VariableHelper.GetName(sourceProperty.Type);
				return $@"
					var {value} = {source}.{sourceProperty.Name};
					if ({value} != null)
					{{
						{context.Write.Object}.WritePropertyName(""{sourceName}"");
						{StaticJsonCustomDeserializerGenerator.GetClassName(serializerType)}.Instance.Serialize({context.Write.Writer}, {value});
					}}";
			}

			return null;
		}

		public string GetRead(string target, ITypeSymbol targetType, IValueSerializationGeneratorContext context) => null;

		public string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, IValueSerializationGeneratorContext context) => null;

		private INamedTypeSymbol FindCustomSerializerType(IPropertySymbol property)
		{
			// Try getting the custom deserializer from the property's attributes first, and then from the 
			// type of the property (since it, too, can have a custom deserialization strategy)
			return property.FindCustomDeserializerType() ?? property.Type.FindCustomDeserializerType();
		}
	}
}
