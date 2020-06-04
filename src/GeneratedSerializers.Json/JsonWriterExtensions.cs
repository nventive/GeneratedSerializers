using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratedSerializers
{
	/// <summary>
	/// Extensions for <see cref="JsonWriter"/>
	/// </summary>
    public static class JsonWriterExtensions
    {
		/// <summary>
		/// Write a Json property name, ending with the ':' (eg. "MyProperty": )
		/// </summary>
		/// <param name="name">Name of the property to write</param>
		public static void WritePropertyName(this JsonWriter writer, string name)
		{
			writer.Write('"');
			writer.Write(name);
			writer.Write("\":");
		}

		/// <summary>
		/// Gets an helper to write properties of an object (autmomatically add the '{' and '}', and help to write the property names unsing <see cref="JsonObjectWriter.WritePropertyName"/>).
		/// </summary>
		/// <param name="writer"></param>
		/// <returns></returns>
		public static JsonObjectWriter OpenObject(this JsonWriter writer)
	    {
		    return new JsonObjectWriter(writer);
	    }

	    public class JsonObjectWriter : IDisposable
	    {
		    private readonly JsonWriter _writer;
		    private bool _needsComma;

		    public JsonObjectWriter(JsonWriter writer)
		    {
			    _writer = writer;

				_writer.Write('{');
		    }

			/// <summary>
			/// Write the Json properties separator (',') if the property is not the first for this ObjectWriter,
			/// and then write a Json property name, ending with the ':' (eg. "MyProperty":  OR , "MyProperty":)
			/// </summary>
			/// <param name="name">Name of the property to write</param>
			public void WritePropertyName(string name)
			{
			    if (_needsComma)
			    {
					_writer.Write(", \"");
			    }
			    else
			    {
					_writer.Write('"');

					_needsComma = true;
			    }
				_writer.Write(name);
				_writer.Write("\":");
			}

			public void Dispose()
		    {
				_writer.Write('}');
			}
	    }
    }
}
