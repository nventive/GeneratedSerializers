using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GeneratedSerializers
{
	public static class JsonReaderExtension
	{
		#region long
		public static long ReadLong(this JsonReader reader, char firstChar, out char? overChar)
		{
			return Int64.Parse(ReadValue(reader, firstChar, out overChar), CultureInfo.InvariantCulture);
		}

		public static long? ReadNullableLong(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(long?) : Int64.Parse(value, CultureInfo.InvariantCulture);
		}

		public static long TryReadLong(this JsonReader reader, char firstChar, out char? overChar)
		{
			return TryParseLong(ReadValue(reader, firstChar, out overChar));
		}

		public static long? TryReadNullableLong(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(long?) : TryParseNullableLong(value);
		}

		private static long TryParseLong(string value)
		{
			long result;
			Int64.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			return result;
		}

		private static long? TryParseNullableLong(string value)
		{
			long result;
			return (Int64.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) ? result : default(long?);
		}
		#endregion

		#region int
		public static int ReadInteger(this JsonReader reader, char firstChar, out char? overChar)
		{
			return Int32.Parse(ReadValue(reader, firstChar, out overChar), CultureInfo.InvariantCulture);
		}

		public static int? ReadNullableInteger(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(int?) : Int32.Parse(value, CultureInfo.InvariantCulture);
		}

		public static int TryReadInteger(this JsonReader reader, char firstChar, out char? overChar)
		{
			return TryParseInteger(ReadValue(reader, firstChar, out overChar));
		}

		public static int? TryReadNullableInteger(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(int?) : TryParseNullableInteger(value);
		}

		private static int TryParseInteger(string value)
		{
			int result;
			Int32.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			return result;
		}

		private static int? TryParseNullableInteger(string value)
		{
			int result;
			return (Int32.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) ? result : default(int?);
		}
		#endregion

		#region short
		public static short ReadShort(this JsonReader reader, char firstChar, out char? overChar)
		{
			return Int16.Parse(ReadValue(reader, firstChar, out overChar), CultureInfo.InvariantCulture);
		}

		public static short? ReadNullableShort(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(short?) : Int16.Parse(value, CultureInfo.InvariantCulture);
		}

		public static short TryReadShort(this JsonReader reader, char firstChar, out char? overChar)
		{
			return TryParseShort(ReadValue(reader, firstChar, out overChar));
		}

		public static short? TryReadNullableShort(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(short?) : TryParseNullableShort(value);
		}

		private static short TryParseShort(string value)
		{
			short result;
			Int16.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			return result;
		}

		private static short? TryParseNullableShort(string value)
		{
			short result;
			return (Int16.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) ? result : default(short?);
		}
		#endregion

		#region Ulong
		public static ulong ReadUnsignedLong(this JsonReader reader, char firstChar, out char? overChar)
		{
			return UInt64.Parse(ReadValue(reader, firstChar, out overChar), CultureInfo.InvariantCulture);
		}

		public static ulong? ReadNullableUnsignedLong(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(ulong?) : UInt64.Parse(value, CultureInfo.InvariantCulture);
		}

		public static ulong TryReadUnsignedLong(this JsonReader reader, char firstChar, out char? overChar)
		{
			return TryParseUnsignedLong(ReadValue(reader, firstChar, out overChar));
		}

		public static ulong? TryReadNullableUnsignedLong(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(ulong?) : TryParseNullableUnsignedLong(value);
		}

		private static ulong TryParseUnsignedLong(string value)
		{
			ulong result;
			UInt64.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			return result;
		}

		private static ulong? TryParseNullableUnsignedLong(string value)
		{
			ulong result;
			return (UInt64.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) ? result : default(ulong?);
		}
		#endregion

		#region Uint
		public static uint ReadUnsignedInteger(this JsonReader reader, char firstChar, out char? overChar)
		{
			return UInt32.Parse(ReadValue(reader, firstChar, out overChar), CultureInfo.InvariantCulture);
		}

		public static uint? ReadNullableUnsignedInteger(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(uint?) : UInt32.Parse(value, CultureInfo.InvariantCulture);
		}

		public static uint TryReadUnsignedInteger(this JsonReader reader, char firstChar, out char? overChar)
		{
			return TryParseUnsignedInteger(ReadValue(reader, firstChar, out overChar));
		}

		public static uint? TryReadNullableUnsignedInteger(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(uint?) : TryParseNullableUnsignedInteger(value);
		}

		private static uint TryParseUnsignedInteger(string value)
		{
			uint result;
			UInt32.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			return result;
		}

		private static uint? TryParseNullableUnsignedInteger(string value)
		{
			uint result;
			return (UInt32.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) ? result : default(uint?);
		}
		#endregion

		#region Ushort
		public static ushort ReadUnsignedShort(this JsonReader reader, char firstChar, out char? overChar)
		{
			return UInt16.Parse(ReadValue(reader, firstChar, out overChar), CultureInfo.InvariantCulture);
		}

		public static ushort? ReadNullableUnsignedShort(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(ushort?) : UInt16.Parse(value, CultureInfo.InvariantCulture);
		}

		public static ushort TryReadUnsignedShort(this JsonReader reader, char firstChar, out char? overChar)
		{
			return TryParseUnsignedShort(ReadValue(reader, firstChar, out overChar));
		}

		public static ushort? TryReadNullableUnsignedShort(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(ushort?) : TryParseNullableUnsignedShort(value);
		}

		private static ushort TryParseUnsignedShort(string value)
		{
			ushort result;
			UInt16.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			return result;
		}

		private static ushort? TryParseNullableUnsignedShort(string value)
		{
			ushort result;
			return (UInt16.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) ? result : default(ushort?);
		}
		#endregion

		#region byte
		public static byte ReadByte(this JsonReader reader, char firstChar, out char? overChar)
		{
			return Byte.Parse(ReadValue(reader, firstChar, out overChar), CultureInfo.InvariantCulture);
		}

		public static byte? ReadNullableByte(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(byte?) : Byte.Parse(value, CultureInfo.InvariantCulture);
		}

		public static byte TryReadByte(this JsonReader reader, char firstChar, out char? overChar)
		{
			return TryParseByte(ReadValue(reader, firstChar, out overChar));
		}

		public static byte? TryReadNullableByte(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(byte?) : TryParseNullableByte(value);
		}

		private static byte TryParseByte(string value)
		{
			byte result;
			Byte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			return result;
		}

		private static byte? TryParseNullableByte(string value)
		{
			byte result;
			return (Byte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) ? result : default(byte?);
		}
		#endregion

		#region double
		public static double ReadDouble(this JsonReader reader, char firstChar, out char? overChar)
		{
			return Double.Parse(ReadValue(reader, firstChar, out overChar), CultureInfo.InvariantCulture);
		}

		public static double? ReadNullableDouble(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(double?) : Double.Parse(value, CultureInfo.InvariantCulture);
		}

		public static double TryReadDouble(this JsonReader reader, char firstChar, out char? overChar)
		{
			return TryParseDouble(ReadValue(reader, firstChar, out overChar));
		}

		public static double? TryReadNullableDouble(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(double?) : TryParseNullableDouble(value);
		}

		private static double TryParseDouble(string value)
		{
			double result;
			Double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			return result;
		}

		private static double? TryParseNullableDouble(string value)
		{
			double result;
			return (Double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) ? result : default(double?);
		}
		#endregion

		#region float
		public static float ReadFloat(this JsonReader reader, char firstChar, out char? overChar)
		{
			return Single.Parse(ReadValue(reader, firstChar, out overChar), CultureInfo.InvariantCulture);
		}

		public static float? ReadNullableFloat(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(float?) : Single.Parse(value, CultureInfo.InvariantCulture);
		}

		public static float TryReadFloat(this JsonReader reader, char firstChar, out char? overChar)
		{
			return TryParseFloat(ReadValue(reader, firstChar, out overChar));
		}

		public static float? TryReadNullableFloat(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(float?) : TryParseNullableFloat(value);
		}

		private static float TryParseFloat(string value)
		{
			float result;
			Single.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			return result;
		}

		private static float? TryParseNullableFloat(string value)
		{
			float result;
			return (Single.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) ? result : default(float?);
		}
		#endregion

		#region decimal
		public static decimal ReadDecimal(this JsonReader reader, char firstChar, out char? overChar)
		{
			return Decimal.Parse(ReadValue(reader, firstChar, out overChar), CultureInfo.InvariantCulture);
		}

		public static decimal? ReadNullableDecimal(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(decimal?) : Decimal.Parse(value, CultureInfo.InvariantCulture);
		}

		public static decimal TryReadDecimal(this JsonReader reader, char firstChar, out char? overChar)
		{
			return TryParseDecimal(ReadValue(reader, firstChar, out overChar));
		}

		public static decimal? TryReadNullableDecimal(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(decimal?) : TryParseNullableDecimal(value);
		}

		private static decimal TryParseDecimal(string value)
		{
			decimal result;
			Decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			return result;
		}

		private static decimal? TryParseNullableDecimal(string value)
		{
			decimal result;
			return (Decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) ? result : default(decimal?);
		}
		#endregion

		#region bool
		public static bool ReadBoolean(this JsonReader reader, char firstChar, out char? overChar)
		{
			return Boolean.Parse(ReadValue(reader, firstChar, out overChar));
		}

		public static bool? ReadNullableBoolean(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(bool?) : Boolean.Parse(value);
		}

		public static bool TryReadBoolean(this JsonReader reader, char firstChar, out char? overChar)
		{
			return TryParseBoolean(ReadValue(reader, firstChar, out overChar));
		}

		public static bool? TryReadNullableBoolean(this JsonReader reader, char firstChar, out char? overChar)
		{
			var value = ReadValue(reader, firstChar, out overChar);
			return IsNull(value) ? default(bool?) : TryParseNullableBoolean(value);
		}

		private static bool TryParseBoolean(string value)
		{
			bool result;
			Boolean.TryParse(value, out result);
			return result;
		}

		private static bool? TryParseNullableBoolean(string value)
		{
			bool result;
			return (Boolean.TryParse(value, out result)) ? result : default(bool?);
		}
		#endregion

		#region string
		public static string ReadString(this JsonReader reader, char firstChar, out char? overChar)
		{
			return ReadValue(reader, firstChar, out overChar);
		}

		public static string TryReadString(this JsonReader reader, char firstChar, out char? overChar)
		{
			return ReadValue(reader, firstChar, out overChar);
		}
		#endregion

		#region Object (and dictionary) parsing helpers
		/// <summary>
		/// Read a "null" or a '{'
		/// <remarks>Usual usage of this is at the beginning of the parsing of an object, to validate if we can return default(object) or if we should read the properties.</remarks>
		/// </summary>
		/// <exception cref="FormatException">If we read anything else than "null" or '{'</exception>
		/// <exception cref="FormatException">Read beyond end of json input</exception>
		/// <param name="reader"></param>
		/// <param name="firstChar"></param>
		/// <param name="overChar"></param>
		/// <returns></returns>
		public static bool OpenObject(this JsonReader reader, char firstChar, out char? overChar)
		{
			if (ReadEndOfNull(reader, firstChar))
			{
				overChar = null;
				return false;
			}
			else if (IsClosingChar(firstChar))
			{
				overChar = firstChar;
				return false;
			}
			else if (firstChar != '{')
			{
				throw new FormatException($"Cannot read an object which start by '{firstChar}'");
			}
			else
			{
				overChar = null;
				return true;
			}
		}

		/// <summary>
		/// Try to read something like ** "PropName" :**
		/// </summary>
		/// <exception cref="FormatException">If we read a closing char which is not '}' (end of object) or the property name is invalid.</exception>
		/// <exception cref="FormatException">Read beyond end of json input</exception>
		/// <param name="reader"></param>
		/// <param name="propertyName"></param>
		/// <param name="toUpper"></param>
		/// <returns>True if we successfully open a property, false if we reach the end of the object</returns>
		public static bool OpenProperty(this JsonReader reader, out string propertyName, bool toUpper = false)
		{
			while (true)
			{
				var c = reader.ReadChar();
				if (c == '"')
				{
					propertyName = reader.ReadEndOfQuotedValue();

					reader.SkipToChar(':');

					if (toUpper)
					{
						propertyName = propertyName.ToUpperInvariant();
					}

					return true;
				}
				else if (c == '}')
				{
					propertyName = null;
					return false;
				}
				else if (IsClosingChar(c))
				{
					throw new FormatException($"Got an invalid token ('{c}') while reading a property name (expected was either '\"', '}}' or whitespace characters.");
				}
			}
		}

		/// <summary>
		/// Try to read the ending coma ',' of a property
		/// </summary>
		/// <exception cref="FormatException">Read beyond end of json input</exception>
		/// <param name="reader"></param>
		/// <param name="firstChar"></param>
		/// <returns>True if the first non whitespace char is ',', false if we read a closing char instead</returns>
		public static bool CloseProperty(this JsonReader reader, char? firstChar)
		{
			// We already reached the end of the object
			if (firstChar.Equals('}'))
			{
				return false;
			}
			// We already reached the end of the property
			else if (firstChar.Equals(','))
			{
				return true;
			}
			// Looking forward to find the next ','. If we get a closing char which is not ',' (i.e. '}' or ']') this means that we reached the end of the object
			else
			{
				return reader.SkipToCharOrClosingChar(',', out firstChar);
			}
		}

		/// <summary>
		/// While reading properties of an object, try read the next property name and move the reader to the content of the property.
		/// </summary>
		/// <exception cref="FormatException">If we found any char that is not expected while reading the property name (closing char which is not '}', or invalid property name).</exception>
		/// <exception cref="FormatException">Read beyond end of json input</exception>
		/// <param name="reader"></param>
		/// <param name="previousPropertyOverChar">The overChar read while reading the previous property content. It is not used for the first property of an object.</param>
		/// <param name="propertyName">The current property name if any. This must be null for the first property of the object.</param>
		/// <param name="toUpper">Indicates if the <paramref name="propertyName"/> should be upper cased or not.</param>
		/// <returns>True if a property name was found and the content is ready to be read, otherwise false.</returns>
		public static bool MoveToNextProperty(this JsonReader reader, ref char? previousPropertyOverChar, ref string propertyName, bool toUpper)
		{
			// WARNING: previousPropertyOverChar MAY have some garbage for the first property, do not use it for the inital check
			if (propertyName == null)
			{
				previousPropertyOverChar = null;
				return reader.OpenProperty(out propertyName, toUpper);
			}
			else
			{
				var moved = reader.CloseProperty(previousPropertyOverChar)
					&& reader.OpenProperty(out propertyName, toUpper);
				previousPropertyOverChar = null;
				return moved;
			}
		}
		#endregion

		#region Collection parsing helpers
		/// <summary>
		/// Try to open a collection
		/// </summary>
		/// <exception cref="FormatException">If the first char is a whitespace</exception>
		/// <exception cref="FormatException">Read beyond end of json input</exception>
		/// <param name="reader">Source to use</param>
		/// <param name="overChar">Char read which should not be read, or null if not</param>
		/// <returns>The detected type of the collection</returns>
		public static CollectionType OpenCollection(this JsonReader reader, char firstChar, out char? overChar)
		{
			switch (firstChar)
			{
				// Open collection char
				case '[':
					overChar = null;
					return CollectionType.Collection;

				// closing chars
				case ']':
				case '}':
				case ',':
					overChar = firstChar;
					return CollectionType.Null;

				// Single value
				case '{':
					overChar = firstChar;
					return CollectionType.SingleValue;

				// Default : skip whitespace chars
				default:
					if (ReadEndOfNull(reader, firstChar))
					{
						//Null value
						overChar = null;
						return CollectionType.Null;
					}
					else if (IsWhiteSpace(firstChar))
					{
						throw new FormatException("Cannot read an collection at this point. Be sure to always provide a non space char as firsChar (use reader.ReadNonWhiteSpaceChar()).");
					}
					else
					{
						// We got a char which is not 'n' (failed to reade null) and not a whitespace
						// Lets try to consider it as the begining of a single value
						overChar = firstChar;
						return CollectionType.SingleValue;
					}
			}
		}

		/// <summary>
		/// In the context of a <see cref="CollectionType.Collection"/>, this while move the reader to the next value if any.
		/// </summary>
		/// <exception cref="FormatException">If we read a '}' (end of object) instead of the expected end of value ',' or end of collection ']'.</exception>
		/// <exception cref="FormatException">Read beyond end of json input</exception>
		/// <param name="overChar">This is the overChar of the last item read in the collection</param>
		/// <returns>True if another item can be read from this colleciton, else false</returns>
		public static bool MoveToNextCollectionItem(this JsonReader reader, ref char? overChar)
		{
			var c = overChar ?? reader.ReadNonWhiteSpaceChar();
			if (c == ',')
			{
				overChar = null;
				return true;
			}
			else if (c == ']')
			{
				overChar = null;
				return false;
			}
			// This method should be used only for CollectionType.Collection
			else if (c == '}')
			{
				throw new FormatException($"Cannot move to next item. Excpected items separator (',') or end of collection (']') but got '{c}'");
			}
			else // That's the beginning of the next item ? We should probably throw here!
			{
				overChar = c;
				return true;
			}
		}

		public enum CollectionType
		{
			/// <summary>
			/// No collection
			/// </summary>
			Null,

			/// <summary>
			/// A collection with a single item
			/// </summary>
			SingleValue,

			/// <summary>
			/// A collection with * items
			/// </summary>
			Collection
		}
		#endregion

		#region Helpers: ReadValue (Quoted or unquoted)
		/// <summary>
		/// Read next value (Quoted or unqoted). This is the base to read a value type or a string.
		/// </summary>
		/// <exception cref="FormatException">Read beyond end of json input</exception>
		/// <param name="reader">Source to use</param>
		/// <param name="firstChar">The first char of the value. MUST NOT BE A WHITE SPACE char</param>
		/// <param name="overChar">Char read which should not be read, or null if not</param>
		/// <returns>The read value</returns>
		private static string ReadValue(this JsonReader reader, char firstChar, out char? overChar)
		{
			// We got a closing char before any other char, value is null!
			if (IsClosingChar(firstChar))
			{
				overChar = firstChar;
				return null;
			}

			if (firstChar == '"')
			{
				// We have a quoted value : allow closing char escaping end read to next non escaped '"'	
				overChar = null;
				return reader.ReadEndOfQuotedValue();
			}
			else
			{
				// We have an unquoted value : read to next closing char, but do not allow escaping
				return reader.ReadEndOfUnquotedValue(firstChar, out overChar);
			}
		}

		/// <exception cref="FormatException">Read beyond end of json input</exception>
		private static string ReadEndOfQuotedValue(this JsonReader reader)
		{
			var sb = new StringBuilder();

			while (true)
			{
				char c = reader.ReadChar();

				if (c == '\\')
				{
					sb.Append(reader.ReadEscapedChar());
				}
				else if (c == '"')
				{
					return sb.ToString();
				}
				else
				{
					sb.Append(c);
				}
			}
		}

		private static string ReadEndOfUnquotedValue(this JsonReader reader, char firstChar, out char? overChar)
		{
			var sb = new StringBuilder();
			sb.Append(firstChar);

			// As we don't really kown where we should stop, we might reach the end of the stream (eg. when reading json for a single value type like: "true").
			while (reader.TryReadChar(out overChar)
				&& !IsClosingChar(overChar.Value))
			{
				sb.Append(overChar.Value);
			}

			var value = sb.ToString().Trim();

			return IsNull(value) ? null : value;
		}

		/// <exception cref="FormatException">Read beyond end of json input</exception>
		private static char ReadEscapedChar(this JsonReader reader)
		{
			var @char = reader.ReadChar();

			switch (@char)
			{
				case '"':
					return '"';
				case '\\':
					return '\\';
				case '/':
					return '/';
				case 'b':
					return '\b';
				case 'f':
					return '\f';
				case 'n':
					return '\n';
				case 'r':
					return '\r';
				case 't':
					return '\t';
				case 'u':
					var charCode = new[]
						{
							reader.ReadChar(),
							reader.ReadChar(),
							reader.ReadChar(),
							reader.ReadChar(),
						};
					return (char)Int32.Parse(new String(charCode), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
				default:
					throw new FormatException($"Invalid Json (Invalid escape sequence '\\{@char}').");
			}
		}
		#endregion

		/// <summary>
		/// Read from JsonReader until we get a char which is not a WhiteSpace.
		/// </summary>
		/// <exception cref="FormatException">Read beyond end of json input</exception>
		/// <param name="reader"></param>
		/// <returns>The first char which is not a whitespace</returns>
		public static char ReadNonWhiteSpaceChar(this JsonReader reader)
		{
			char c;
			do
			{
				c = reader.ReadChar();
			} while (IsWhiteSpace(c));

			return c;
		}

		private static bool IsWhiteSpace(char c) => Char.IsWhiteSpace(c) || Char.IsSurrogate(c) || c == '\uFEFF';

		/// <summary>
		/// Determine if <see cref="c"/> is a json closing char ('}', ']' or ',').
		/// </summary>
		/// <param name="c">The char</param>
		/// <returns><c>True</c> if c is '}', ']' or ',', else <c>False</c></returns>
		private static bool IsClosingChar(char c)
		{
			return c == ',' || c == '}' || c == ']';
		}

		
		private static bool DefaultReadEndOfNull(JsonReader reader, char firstChar)
		{
			if (firstChar == 'n')
			{
				if (reader.ReadChar() != 'u'
					|| reader.ReadChar() != 'l'
					|| reader.ReadChar() != 'l')
				{
					throw new FormatException("Value start with 'n', but cannot read \"null\".");
				}

				return true;
			}

			return false;
		}

		private const string _null = "null";
		
		private static bool DefaultIsNull(string value)
		{
			return String.IsNullOrWhiteSpace(value) || value.Trim() == _null;
		}

		/// <summary>
		/// Func used to determine if a string value should be consider as null
		/// </summary>
		public static Func<string, bool> IsNull { get; set; }

		/// <summary>
		/// Func responsible to determine if null is present in the reader, starting at the firstChar. Returns 'false' if does not match the null pattern, 'true' if it matches the pattern.
		/// If the format of the answer is not valid,  this method should throw a FormatException.
		/// </summary>
		/// <exception cref="FormatException">Read beyond end of json input</exception>
		public static Func<JsonReader, char, bool> ReadEndOfNull { get; set; }

		static JsonReaderExtension()
		{
			IsNull = DefaultIsNull;
			ReadEndOfNull = DefaultReadEndOfNull;
		}

		#region Helpers: Skip (char / items)
		/// <summary>
		/// Read from source until find the end of a item.
		/// </summary>
		/// <exception cref="FormatException">Read beyond end of json input</exception>
		/// <param name="reader">Source to use</param>
		/// <param name="overChar">Char read which should not be read, or null if not (i.e. a closing char)</param>
		public static void SkipItem(this JsonReader reader, out char? overChar)
		{
			var curlyBracketCount = 0;
			var squareBracketCount = 0;
			var isInString = false;
			var escaped = false;

			do
			{
				var c = reader.ReadChar();

				if (isInString)
				{
					if(escaped)
					{
						escaped = false;
					}
					else if (c == '\"')
					{
						isInString = false;
					}
					else if (c == '\\')
					{
						escaped = true;
					}
				}
				else
				{
					switch (c)
					{
						case '\"':
							isInString = true;
							break;
						case '{':
							curlyBracketCount++;
							break;
						case '}':
							curlyBracketCount--;
							if (curlyBracketCount <= 0 && squareBracketCount <= 0)
							{
								overChar = curlyBracketCount < 0 ? '}' : default(char?);
								return;
							}
							break;

						case '[':
							squareBracketCount++;
							break;
						case ']':
							squareBracketCount--;
							if (curlyBracketCount <= 0 && squareBracketCount <= 0)
							{
								overChar = squareBracketCount < 0 ? ']' : default(char?);
								return;
							}
							break;

						case ',':
							if (curlyBracketCount <= 0 && squareBracketCount <= 0)
							{
								overChar = ',';
								return;
							}
							break;
					}
				}
			} while (true);
		}


		/// <summary>
		/// Read char from <see cref="reader"/> until find the expected <see cref="char"/>.
		/// </summary>
		/// <exception cref="FormatException">Read beyond end of json input</exception>
		/// <param name="reader">Source to use</param>
		/// <param name="char">Char to search</param>
		private static void SkipToChar(this JsonReader reader, char @char)
		{
			while (reader.ReadChar() != @char)
			{
			}
		}

		/// <summary>
		/// Read char from <see cref="reader"/> until find the expected <see cref="char"/>, or a closing char ('}', ']' or ',').
		/// </summary>
		/// <exception cref="FormatException">Read beyond end of json input</exception>
		/// <param name="reader">Source to use</param>
		/// <param name="char">Char to search</param>
		/// <param name="overChar">Char read which should not be read, or null if not (i.e. a closing char)</param>
		/// <returns><c>True</c> if the <see cref="char"/> was found, else <c>false</c>.</returns>
		private static bool SkipToCharOrClosingChar(this JsonReader reader, char @char, out char? overChar)
		{
			while (true)
			{
				var c = reader.ReadChar();
				if (c == @char)
				{
					overChar = null;
					return true;
				}
				else if (IsClosingChar(c))
				{
					overChar = c;
					return false;
				}
			}
		}
		#endregion
	}
}
