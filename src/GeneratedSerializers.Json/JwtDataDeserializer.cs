using System;
using System.Text;

namespace GeneratedSerializers
{
	/// <summary>
	/// Custom deserializer to parse a RFC 7519 JSON Web Token
	/// </summary>
	/// <remarks>
	/// Header will be parsed as an dictionary.
	/// </remarks>
	public sealed class JwtDataDeserializer<TPayload> : ICustomTypeSerializer<JwtData<TPayload>>
		where TPayload : class
	{
		/// <inheritdoc />
		public JwtData<TPayload> Read(
			JsonReader reader,
			char firstChar,
			out char? overChar,
			IStaticSerializerProvider staticSerializerProvider)
		{
			var token = reader.TryReadString(firstChar, out overChar);
			var serializer = staticSerializerProvider.GetObjectSerializer();

			return new JwtData<TPayload>(token, serializer);
		}

		/// <inheritdoc />
		public void Write(JsonWriter writer, JwtData<TPayload> value, IStaticSerializerProvider staticSerializerProvider)
		{
			if (value?.Token == null)
			{
				writer.WriteNullValue();
			}
			else
			{
				writer.WriteStringValue(value.Token);
			}
		}
	}
}
