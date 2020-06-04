using System;
using System.IO;
using System.Text;

namespace GeneratedSerializers
{
	/// <summary>
	/// An abstraction over an object serializer.
	/// </summary>
	public interface IObjectSerializer
	{
		/// <summary>
		/// Determines whatever the given type is supported by this serializer or not.
		/// </summary>
		/// <param name="valueType"></param>
		/// <returns>True if this type can be serialized / deserialized, false else.</returns>
		bool IsSerializable(Type valueType);

		/// <summary>
		/// Creates a serialized representation of an object.
		/// </summary>
		/// <param name="value">The object to serialize.</param>
		/// <param name="valueType">The type to use to serialize the object. <paramref name="value"/> must be convertible to this type.</param>
		/// <returns>The serialized representation of <paramref name="value"/>.</returns>
		string ToString(object value, Type valueType);

		/// <summary>
		/// Creates a serialized representation of an object.
		/// </summary>
		/// <param name="value">The object to serialize.</param>
		/// <param name="valueType">The type to use to serialize the object. <paramref name="value"/> must be convertible to this type.</param>
		/// <returns>The serialized representation of <paramref name="value"/>.</returns>
		Stream ToStream(object value, Type valueType);

		/// <summary>
		/// Write a serialized representation of an object to a given string.
		/// </summary>
		/// <param name="value">The object to serialize.</param>
		/// <param name="valueType">The type to use to serialize the object. <paramref name="value"/> must be convertible to this type.</param>
		/// <param name="builder">The string builder to which the value should be written.</param>
		void WriteToString(object value, Type valueType, StringBuilder builder);

		/// <summary>
		/// Write a serialized representation of an object to a given <see cref="Stream"/>.
		/// </summary>
		/// <param name="value">The object to serialize.</param>
		/// <param name="valueType">The type to use to serialize the object. <paramref name="value"/> must be convertible to this type.</param>
		/// <param name="stream">The stream to which the value should be written.</param>
		/// <param name="canDisposeStream">A bool which indicates if the <paramref name="stream"/> can be disposed after having written the object or not.</param>
		void WriteToStream(object value, Type valueType, Stream stream, bool canDisposeStream = true);

		/// <summary>
		/// Creates an instance of <paramref name="targetType"/> from a serialized representation.
		/// </summary>
		/// <param name="source">A serialized representation of a <paramref name="targetType"/>.</param>
		/// <param name="targetType">The type to use to deserialize the <paramref name="source"/>.</param>
		/// <returns>The instance of <paramref name="targetType"/> deserialized from the <see cref="source"/>.</returns>
		object FromString(string source, Type targetType);

		/// <summary>
		/// Creates an instance of <paramref name="targetType"/> from a serialized representation.
		/// </summary>
		/// <param name="source">A serialized representation of a <paramref name="targetType"/>.</param>
		/// <param name="targetType">The type to use to deserialize the <paramref name="source"/>.</param>
		/// <returns>The instance of <paramref name="targetType"/> deserialized from the <see cref="source"/>.</returns>
		object FromStream(Stream source, Type targetType);
	}
}
