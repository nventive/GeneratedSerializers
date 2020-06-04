using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GeneratedSerializers
{
	/// <summary>
	/// Provide an abstraction over a serializer.
	/// </summary>
	/// <remarks>
	/// This is a legacy interface, the use of <see cref="IObjectSerializer"/> should be done instead.
	/// </remarks>
	public interface ISerializer : IObjectSerializer
	{
		/// <summary>
		/// Deserialize an instance from a <see cref="Stream"/>.
		/// </summary>
		T Deserialize<T>(Stream stream);

		/// <summary>
		/// Get a <see cref="Stream"/> containing a serialized representation of the instance.
		/// </summary>
		Stream Serialize<T>(T instance);
	}
}
