using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Uno.Extensions;

namespace GeneratedSerializers
{
	public class CompositeValueGenerator : IValueSerializationGenerator
	{
		private readonly IValueSerializationGenerator[] _generators;

		public CompositeValueGenerator(params IValueSerializationGenerator[] generators)
		{
			_generators = generators;
		}

		public string GetRead(string target, IPropertySymbol targetProperty, IValueSerializationGeneratorContext context)
		{
			return _generators
				.Select(g => g.GetRead(target, targetProperty, context))
				.FirstOrDefault(code => code.HasValue());
		}

		public string GetWrite(string sourceName, string source, IPropertySymbol sourceProperty, IValueSerializationGeneratorContext context)
		{
			return _generators
				.Select(g => g.GetWrite(sourceName, source, sourceProperty, context))
				.FirstOrDefault(code => code.HasValue());
		}

		public string GetRead(string target, ITypeSymbol targetType, IValueSerializationGeneratorContext context)
		{
			return _generators
				.Select(g => g.GetRead(target, targetType, context))
				.FirstOrDefault(code => code.HasValue());
		}

		public string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, IValueSerializationGeneratorContext context)
		{
			return _generators
				.Select(g => g.GetWrite(sourceName, sourceCode, sourceType, context))
				.FirstOrDefault(code => code.HasValue());
		}
	}
}
