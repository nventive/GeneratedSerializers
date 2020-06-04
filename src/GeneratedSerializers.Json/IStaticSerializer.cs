using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratedSerializers
{
	/// <summary>
	/// A serializer dedicated to a well known type (Usually a serializer which was generated for a given type).
	/// </summary>
	public interface IStaticSerializer
	{
		/// <summary>
		/// Write an entity to a JsonWriter
		/// </summary>
		/// <param name="writer">The writer on which the entity serialized entity should be written</param>
		/// <param name="value">Entity to serialize</param>
		void Serialize(JsonWriter writer, object value);

		/// <summary>
		/// Read an entity from a JsonReader
		/// </summary>
		/// <param name="reader">The reader on which the entity should be read</param>
		/// <param name="firstChar">The first non whitespace char of the entity</param>
		/// <param name="overChar">A char mistaken read after the end of the entity or null if none.</param>
		/// <returns>Entity read</returns>
		object Deserialize(JsonReader reader, char firstChar, out char? overChar);
	}

	/// <summary>
	/// A serializer dedicated to <typeparamref name="T"/> (Usually a serializer which was generated for a <typeparamref name="T"/>).
	/// </summary>
	public interface IStaticSerializer<T> : IStaticSerializer
	{
		/// <summary>
		/// Write an entity to a JsonWriter
		/// </summary>
		/// <param name="writer">The writer on which the entity serialized entity should be written</param>
		/// <param name="value">Entity to serialize</param>
		void Serialize(JsonWriter writer, T value);

		/// <summary>
		/// Read an entity from a JsonReader
		/// </summary>
		/// <param name="reader">The reader on which the entity should be read</param>
		/// <param name="firstChar">The first non whitespace char of the entity</param>
		/// <param name="overChar">A char mistaken read after the end of the entity or null if none.</param>
		/// <returns>Entity read</returns>
		new T Deserialize(JsonReader reader, char firstChar, out char? overChar);
	}
}
