using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public static class ValueSerializationGeneratorExtensions
	{
		public static string Read<T>(this IValueSerializationGeneratorContext context, string targetPropertyName)
		{
			return context.GetRead(targetPropertyName, context.Roselyn.GetTypeByFullName(typeof(T).FullName));
		}

		public static string Write<T>(this IValueSerializationGeneratorContext context, string sourceName, string source)
		{
			return context.GetWrite(sourceName, source, context.Roselyn.GetTypeByFullName(typeof(T).FullName));
		}

		public static IValueSerializationGeneratorContext UsingOverAsFirstChar(this IValueSerializationGeneratorContext context, bool overHasValue = false)
		{
			return new Context(context, read: context.Read.UsingOverAsFirstChar(overHasValue));
		}

		public static IValueSerializationGeneratorContext IgnoringCurrentFirstChar(this IValueSerializationGeneratorContext context, bool overHasValue = false)
		{
			return new Context(context, read: context.Read.IgnoringCurrentFirstChar(overHasValue));
		}

		public static string GetRead (this IValueSerializationGeneratorContext context, string target, IPropertySymbol targetProperty) => context.ValueGenerator.GetRead(target, targetProperty, context);
		public static string GetWrite(this IValueSerializationGeneratorContext context, string sourceName, string source, IPropertySymbol sourceProperty) => context.ValueGenerator.GetWrite(sourceName, source, sourceProperty, context);
		public static string GetRead (this IValueSerializationGeneratorContext context, string target, ITypeSymbol targetType) => context.ValueGenerator.GetRead(target, targetType, context);
		public static string GetWrite(this IValueSerializationGeneratorContext context, string sourceName, string sourceCode, ITypeSymbol sourceType) => context.ValueGenerator.GetWrite(sourceName, sourceCode, sourceType, context);

		private class Context : IValueSerializationGeneratorContext
		{
			private readonly IValueSerializationGeneratorContext _inner;

			public Context(IValueSerializationGeneratorContext inner, ReadContext read = null, WriteContext write = null)
			{
				_inner = inner;
				Read = read ?? inner.Read;
				Write = write ?? inner.Write;
			}

			public RoslynMetadataHelper Roselyn => _inner.Roselyn;

			public IValueSerializationGenerator ValueGenerator => _inner.ValueGenerator;

			public ReadContext Read { get; }

			public WriteContext Write { get; }
		}
	}
}
