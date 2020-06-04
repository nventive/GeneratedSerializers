using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratedSerializers
{
	public interface IStaticSerializerProvider
	{
		/// <summary>
		/// Get a serializer for a specific type in the current serialization/deserialization context.
		/// </summary>
		IStaticSerializer Get<T>();

		/// <summary>
		/// Get the root serializer for a new serialization context.
		/// </summary>
		/// <remarks>
		/// This is mostly used for custom serializer/deserializer when you need to
		/// encapsulate serialized string as data.
		/// Ex: JsonWebToken is used this feature because it's json encoded in another json context.
		/// </remarks>
		IObjectSerializer GetObjectSerializer();
	}
}
