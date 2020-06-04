using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace GeneratedSerializers
{
	/// <summary>
	/// A TextWriter dedicated to write Json content
	/// </summary>
	public abstract class JsonWriter : TextWriter
    {
		/// <inheritdoc/>
		public abstract override void Write(char value);

		/// <inheritdoc/>
		public override void Write(bool value)
		{
			// Override to put it in lower case
			Write(value ? "true" : "false");
		}

		/// <summary>
		/// Get the culture used to format values (double, etc.) which is always <see cref="CultureInfo.InvariantCulture"/> for Json.
		/// </summary>
		public sealed override IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

		/// <summary>
		/// Gets the encoding used to write data (if needed). This is always <see cref="Encoding.UTF8"/> for Json.
		/// </summary>
		public sealed override Encoding Encoding => Encoding.UTF8;

		/// <summary>
		/// Writes a Json quoted and escaped string
		/// </summary>
		public void WriteStringValue(string value)
		{
			if (value == null)
			{
				WriteNullValue();
			}
			else
			{
				Write('"');
				foreach (var c in value)
				{
					switch (c)
					{
						case '"':
							Write(@"\""");
							break;
						case '\\':
							Write(@"\\");
							break;
						case '/':
							Write(@"\/");
							break;
						case '\b':
							Write(@"\b");
							break;
						case '\f':
							Write(@"\f");
							break;
						case '\n':
							Write(@"\n");
							break;
						case '\r':
							Write(@"\r");
							break;
						case '\t':
							Write(@"\t");
							break;
						default:
							Write(c);
							break;
					}
				}
				Write('"');
			}
		}

		/// <summary>
		/// Writes the Json keyword for empty values (i.e. "null")
		/// </summary>
		public void WriteNullValue()
		{
			Write("null");
		}
    }
}
