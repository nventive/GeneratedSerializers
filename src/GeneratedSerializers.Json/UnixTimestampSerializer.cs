using System;
using System.Linq;

namespace GeneratedSerializers
{
	/// <summary>
	/// This deserializer can be used to parse a Json string or numeric value into a DateTimeOffset, based on the Unix Timestamp definition.
	/// (offset in seconds since Jan 1st, 1970). It supports sub-second accuracy.
	/// </summary>
	public class UnixTimestampSerializer : ICustomTypeSerializer<DateTimeOffset>
	{
		DateTimeOffset ICustomTypeSerializer<DateTimeOffset>.Read(JsonReader reader, char firstChar, out char? overChar, IStaticSerializerProvider staticSerializerProvider)
		{
			var offset = reader.ReadLong(firstChar, out overChar);

			return DateTimeExtensions.FromUnixTimeSeconds(offset, TimeSpan.Zero);
        }

		public void Write(JsonWriter writer, DateTimeOffset value, IStaticSerializerProvider staticSerializerProvider)
		{
			writer.Write(value.ToUnixTimeSeconds());
		}
	}
}
