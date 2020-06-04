namespace GeneratedSerializers
{
	/// <summary>
	/// Base class generated StaticSerailizer. DO NOT USE IT.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class StaticJsonSerializerBase<T> : IStaticSerializer<T>
		where T : new()
	{
		private readonly bool _toUpper;

		protected StaticJsonSerializerBase(bool toUpper = true)
		{
			_toUpper = toUpper;
		}

		void IStaticSerializer.Serialize(JsonWriter writer, object value) => SerializeCore(writer, (T)value);

		void IStaticSerializer<T>.Serialize(JsonWriter writer, T value) => SerializeCore(writer, (T)value);

		object IStaticSerializer.Deserialize(JsonReader reader, char firstChar, out char? overChar) => DeserializeCore(reader, firstChar, out overChar);

		T IStaticSerializer<T>.Deserialize(JsonReader reader, char firstChar, out char? overChar) => DeserializeCore(reader, firstChar, out overChar);

		protected abstract void SerializeCore(JsonWriter writer, T value);

		/// <summary>
		/// Deserialization base, handle json object markup and call <seealso cref="ReadProperty"/> for each value to deserialize.
		/// </summary>
		/// <param name="reader">Source to use</param>
		/// <param name="firstChar">The first NON WHITE SPACE char of the object</param>
		/// <param name="overChar">Char read which should not be read, or null if not</param>
		/// <returns>Deserialized object</returns>
		protected T DeserializeCore(JsonReader reader, char firstChar, out char? overChar)
		{
			if (reader.OpenObject(firstChar, out overChar))
			{
				var entity = new T();
				var propertyName = default(string);

				while (reader.MoveToNextProperty(ref overChar, ref propertyName, _toUpper))
				{
					ReadProperty(entity, propertyName, reader, out overChar);
				}

				return entity;
			}
			else
			{
				return default(T);
			}
		}

		protected abstract void ReadProperty(T entity, string propertyName, JsonReader reader, out char? overChar);
	}
}
