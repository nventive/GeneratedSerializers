using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class GeneratorContext : IValueSerializationGeneratorContext
	{
		public GeneratorContext(
			RoslynMetadataHelper roselyn,
			ReadContext read,
			WriteContext write,
			params IValueSerializationGenerator[] generators)
			: this(roselyn, read, write, new CompositeValueGenerator(generators))
		{
			
		}

		public GeneratorContext(
			RoslynMetadataHelper roselyn,
			ReadContext read,
			WriteContext write,
			IValueSerializationGenerator generator)
		{
			Roselyn = roselyn;
			Read = read;
			Write = write;
			ValueGenerator = generator;
		}

		public RoslynMetadataHelper Roselyn { get; }
		public IValueSerializationGenerator ValueGenerator { get; }
		public ReadContext Read { get; }
		public WriteContext Write { get; }
	}
}
