using System;
using System.Linq;

namespace GeneratedSerializers
{
	public class SerializerGenerationConfiguration
	{
		private string _serializersNameSpace;

		/// <summary>
		/// [Required] Type of generator to use to generate serializers.
		/// </summary>
		public SerializationTypeName SerializationType { get; set; }

		/// <summary>
		/// [Required] The default namespace use to search <seealso cref="Entities"/> to generate
		/// </summary>
		public string EntitiesNameSpace { get; set; }

		/// <summary>
		/// [Required] Name of types to generate (Use full qualified names or Types in <seealso cref="EntitiesNameSpace"/> namespace).
		/// </summary>
		public string[] Entities { get; set; }
        
        /// <summary>
        /// Disable the use of ReflectionOnly assembly loading (WinRT compatibility when loading WinMD related types)
        /// </summary>
		public bool DisableReflectionOnlyLookup { get; set; }

		/// <summary>
		/// Disables the use of the ToUpper parameter in StaticJsonDeserializerBase. The default value is false, set to true for compatibility with older umbrella versions.
		/// </summary>
		public bool DisableToUpperConstructor { get; set; }

		/// <summary>
		/// If set to true, the deserializer will try parsing the value.
		/// If the parsing fails, it will set it to the default of T.
		/// </summary>
		public bool UseTryParseOrDefault { get; set; }

		/// <summary>
		/// Allow to not define the FallbackAttributes on enums
		/// </summary>
		public bool IsMissingFallbackOnEnumsAllowed { get; set; }

		/// <summary>
		/// Determines if the Immutable collection are generated at root or not
		/// <remarks>This is by default true until we have the immutables collections for all platforms</remarks>
		/// </summary>
		public bool IsImmutablesAtRootDisabled { get; set; } = true;

		/// <summary>
		/// Namespace of deserializers. Default is <see cref="EntitiesNameSpace"/>.Serializers
		/// </summary>
		public string SerializersNameSpace
		{
			get { return _serializersNameSpace ?? EntitiesNameSpace + ".Serializers"; }
			set { _serializersNameSpace = value; }
		}

		/// <summary>
		/// Name of types with custom serialization code (Use full qualified names or Types in <seealso cref="EntitiesNameSpace"/> namespace). This 
		/// will generate partial methods for implementing custom serialization code. 
		/// </summary>
		public string[] CustomSerializationTypeNames { get; set; }

		/// <summary>
		/// Defines the generated module class name
		/// </summary>
		public string ModuleClassName { get; set; }
	}
}
