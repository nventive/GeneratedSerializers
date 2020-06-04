using Microsoft.CodeAnalysis;
using Uno.Extensions;

namespace GeneratedSerializers
{
	public class RecursiveStaticSerializerGenerator : IValueSerializationGenerator
	{
		private readonly IStaticSerializerResolver _serializer;

		public RecursiveStaticSerializerGenerator(IStaticSerializerResolver serializer)
		{
			_serializer = serializer;
		}

		public string GetRead(string target, IPropertySymbol targetProperty, IValueSerializationGeneratorContext context)
		{
			if (targetProperty.Type is INamedTypeSymbol instanceType
				&& _serializer.IsResolvable(instanceType))
			{
				return $"{target}.{targetProperty.Name} = {_serializer.GetResolve(instanceType)}.Deserialize({context.Read.Reader}, {context.Read.FirstChar}, out {context.Read.OverChar});";
			}

			return null;
		}

		public string GetWrite(string sourceName, string source, IPropertySymbol sourceProperty, IValueSerializationGeneratorContext context) => GetWrite(
			sourceName,
			$"{source}.{sourceProperty.Name}",
			sourceProperty.Type,
			context);

		public string GetRead(string target, ITypeSymbol targetType, IValueSerializationGeneratorContext context)
		{
			return _serializer.IsResolvable(targetType)
				? $"{target} = {_serializer.GetResolve(targetType)}.Deserialize({context.Read.Reader}, {context.Read.FirstChar}, out {context.Read.OverChar});"
				: null;
		}

		public string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, IValueSerializationGeneratorContext context)
		{
			if (_serializer.IsResolvable(sourceType))
			{
				var value = VariableHelper.GetName(sourceType);

				string result;
				if (sourceName.IsNullOrWhiteSpace())
				{
					// We are writing an item of a collection or something like that. 
					// We cannot ignore null values, instead we must write the "null" keyword.

					result =
$@"var {value} = {sourceCode};
if ({value} == null)
{{
	{context.Write.Writer}.WriteNullValue();
}}
else
{{
	{_serializer.GetResolve(sourceType)}.Serialize({context.Write.Writer}, {value});
}}";
				}
				else
				{
					result =
$@"var {value} = {sourceCode};
if ({value} != null)
{{
	{context.Write.Object}.WritePropertyName(""{sourceName}"");
	{_serializer.GetResolve(sourceType)}.Serialize({context.Write.Writer}, {value});
}}";
				}

				return result;
			}

			return null;
		}
	}
}
