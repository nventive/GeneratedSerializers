using System;
using System.Linq;
using System.Reflection;

namespace GeneratedSerializers
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class SerializationCustomAttribute : Attribute
	{
		/// <summary>
		/// This attribute is used to specify the type of the class responsible for the deserialization of the property it is attached to.
		/// <remarks>The type passed in parameter must implment <see cref="ICustomTypeSerializer<T>"/>, otherwise the generation of the serializers will fail.</remarks>
		/// </summary>
		/// <param name="type">Type of the class responsible for deserialization.  This must implement <see cref="ICustomTypeDeserializer<T>"/></param>
		public SerializationCustomAttribute(Type type)
		{
			Type = type;
		}

		/// <summary>
		/// Type of the class responsible for deserialize the property. 
		/// <remarks>The type passed in parameter must implment <see cref="ICustomTypeSerializer<T>"/>, otherwise the generation of the serializers will fail.</remarks>
		/// </summary>
		public Type Type { get; set; }
	}
}
