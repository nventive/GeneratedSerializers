using System;
using System.Collections.Generic;

namespace GeneratedSerializers
{
	public enum SerializationTypeName
	{
		Json
	}

	public static class SerializationTypeNameExtensions
	{
		public static SerializationType CreateInstance(this SerializerGenerationConfiguration configuration, RoslynMetadataHelper helper)
		{
			switch (configuration.SerializationType)
			{
				case SerializationTypeName.Json:
					return new JsonSerializationType
					{
						Name = "Json",
						PropertyFinder = new DefaultPropertyFinder(),
						StaticDeserializerPropertyFinder = new JsonStaticDeserializerPropertyFinder(),
						CustomDeserializerPropertyFinder = new JsonCustomDeserializerPropertyFinder(),
						PropertyGenerators = new List<IValueSerializationGenerator>
						{
							// Note: Generators are used in the order of the registration.
							// Be sure to put "special cases" before "generic cases"
							//
							// For instance, put the MicrosoftDateTimeGenerator before the JsonReaderGenerator 
							// which also handles DateTime, but does not validate the attribute.

							new CustomSerializerGenerator(), // This must be the first in order to be able to override all types

							new UriGenerator(), 
							new GuidGenerator(), 
							new EnumGenerator(new Enums(string.Empty, true, true, null, new DefaultPropertyFinder(), null)),
							new MicrosotDateTimeGenerator(configuration.UseTryParseOrDefault),
							new DateTimeGenerator(configuration.UseTryParseOrDefault),

							new JsonCollectionGenerator(new CollectionImplementationResolver(helper)),

							new JsonReaderGenerator(configuration.UseTryParseOrDefault),
							new JsonWriterGenerator(),
						}
					};

				default:
					throw new ArgumentOutOfRangeException("typeName", "Unknwon serialization type.");
			}
		}
	}

	public abstract class SerializationType
	{
		/// <summary>
		/// The name of the serialization type.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Defines how we find all properties that are part of a static deserializer for an object. 
		/// </summary>
		public IPropertyFinder PropertyFinder { get; set; }

		/// <summary>
		/// Define how we determine which property of a type should be included as a static serializer.
		/// </summary>
		public IPropertyFinder StaticDeserializerPropertyFinder { get; set; }

		/// <summary>
		/// Define how we determine which property of a type should have a custom property serializer, instead of an automatically generated serializer.
		/// </summary>
		public IPropertyFinder CustomDeserializerPropertyFinder { get; set; }

		/// <summary>
		/// Define how a property will be generated.
		/// </summary>
		public List<IValueSerializationGenerator> PropertyGenerators { get; set; }

		public abstract IStaticSerializerResolver GetGenerator(SerializerGenerationConfiguration config, RoslynMetadataHelper metadataHelper, SourceFileMetadata metadata);
		public abstract IStaticSerializerResolver GetBuilderGenerator(SerializerGenerationConfiguration config, RoslynMetadataHelper metadataHelper, SourceFileMetadata metadata);

		public abstract ISerializerGenerator GetCustomSerializerGenerator(SerializerGenerationConfiguration config, string serializerProviderFullName, SourceFileMetadata metadata);
	}

	public class JsonSerializationType : SerializationType
	{
		public override IStaticSerializerResolver GetGenerator(
			SerializerGenerationConfiguration config,
			RoslynMetadataHelper metadataHelper,
			SourceFileMetadata generatedCodeMeta)
		{
			var generator = new StaticJsonSerializerGenerator(
				metadataHelper,
				config.SerializersNameSpace, 
				config.DisableToUpperConstructor,
				PropertyFinder,
				PropertyGenerators,
				generatedCodeMeta
			);
			
			PropertyGenerators.Add(new RecursiveStaticSerializerGenerator(generator));

			return generator;
		}

		public override IStaticSerializerResolver GetBuilderGenerator(
			SerializerGenerationConfiguration config,
			RoslynMetadataHelper metadataHelper,
			SourceFileMetadata generatedCodeMeta)
		{
			var generator = new StaticJsonBuilderSerializerGenerator(
				metadataHelper,
				config.SerializersNameSpace,
				config.DisableToUpperConstructor,
				PropertyFinder,
				PropertyGenerators,
				generatedCodeMeta
			);

			PropertyGenerators.Add(new RecursiveStaticSerializerGenerator(generator));

			return generator;
		}

		public override ISerializerGenerator GetCustomSerializerGenerator(SerializerGenerationConfiguration config, string serializerProviderFullName, SourceFileMetadata generatedCodeMeta)
		{
			var generator = new StaticJsonCustomDeserializerGenerator(
				config.SerializersNameSpace,
				generatedCodeMeta,
				serializerProviderFullName
			);

			PropertyGenerators.Add(new RecursiveStaticSerializerGenerator(generator));

			return generator;
		}
	}	
}
