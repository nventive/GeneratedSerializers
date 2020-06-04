using System;
using System.Globalization;

namespace GeneratedSerializers
{
	/// <summary>
	/// Helper to handle the common date format "/Date(123+01)/" which is created by the JsonDataContractSerializer
	/// </summary>
	public static class MicrosoftDateTimeHelper
	{
		/// <summary>
		/// Try to parse a DateTimeOffset from a string using the Microsoft json date-time format
		/// </summary>
		/// <param name="value">The value to parse.</param>
		/// <param name="dateTime">When this method returns, contains the parsed DateTimeOffset, or default(DateTimeOffset) if the value does meet format requirements (eg. "/Date(123+01)/")</param>
		/// <returns>Returns a value that indicates whether the conversion succeeded</returns>
		public static bool TryParseDateTimeOffset(string value, out DateTimeOffset dateTime)
		{
			return TryParse(value, out dateTime);
		}

		/// <summary>
		/// Parse a DateTimeOffset from a string using the Microsoft json date-time format
		/// </summary>
		/// <param name="value">The value to parse.</param>
		/// <returns>The parsed DateTimeOffset.</returns>
		/// <exception cref="FormatException">If the value does meet format requirements (eg. "/Date(123+01)/")</exception>
		public static DateTimeOffset ParseDateTimeOffset(string value)
		{
			DateTimeOffset result;
			if (TryParse(value, out result))
			{
				return result;
			}
			else
			{
				throw new FormatException("Invalid date, Microsoft style JSON DateTime needs to follow pattern '/Date\\((<?unix_timespan>[0-9]+)([+-](<?offset_hours>[0-9]{2})(<?offset_minutes>[0-9]{2})?)'\\)/");
			}
		}

		/// <summary>
		/// Try to parse a DateTime from a string using the Microsoft json date-time format
		/// </summary>
		/// <param name="value">The value to parse.</param>
		/// <param name="dateTime">When this method returns, contains the parsed DateTime, or default(DateTime) if the value does meet format requirements (eg. "/Date(123+01)/").
		/// <remarks>The DateTime is UTC</remarks>
		/// </param>
		/// <returns>Returns a value that indicates whether the conversion succeeded</returns>
		public static bool TryParseDateTime(string value, out DateTime dateTime)
		{
			DateTimeOffset result;
			if (TryParse(value, out result))
			{
				dateTime = result.UtcDateTime;
		
				return true;
			}
			else
			{
				dateTime = default(DateTime);
				return false;
			}
		}

		/// <summary>
		/// Parse a DateTime from a string using the Microsoft json date-time format
		/// </summary>
		/// <param name="value">The value to parse.</param>
		/// <returns>
		/// The parsed DateTime.
		/// <remarks>The DateTime is UTC</remarks>
		/// </returns>
		/// <exception cref="FormatException">If the value does meet format requirements (eg. "/Date(123+01)/")</exception>
		public static DateTime ParseDateTime(string value)
		{
			DateTimeOffset result;
			if (TryParse(value, out result))
			{
				return result.UtcDateTime;
			}
			else
			{
				throw new FormatException("Invalid date, Microsoft style JSON DateTime needs to follow pattern '/Date\\((<?unix_timespan>[0-9]+)([+-](<?offset_hours>[0-9]{2})(<?offset_minutes>[0-9]{2})?)'\\)/");
			}
		}

		public static string ToString(DateTime value)
		{
			return string.Format(CultureInfo.InvariantCulture, "/Date({0})/", DateTimeExtensions.ToUnixTimeMilliseconds(value));
		}

		public static string ToString(DateTimeOffset value)
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"/Date({0}{1})/",
				value.ToUnixTimeMilliseconds(),
				value.ToString("zzz").Replace(":", "") // "-05:00" -> "-0500"
			);
		}

		private const string _prefix = "/Date(";
		private const int _prefixLength = 6;

		private static bool TryParse(string value, out DateTimeOffset result)
		{
			//avoid nullRefenceException
			value = (value ?? string.Empty).Trim();

			if (!value.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase))
			{
				result = default(DateTimeOffset);
				return false;
			}

			long time;

			// LastIndexOf is faster than index of when searching for a char that is more likely to be at the end of string.
			var offsetIndex = value.LastIndexOf('+', value.Length - 1);
			if (offsetIndex != -1)
			{
				// Read positive offset value
				int hours, minutes;
				if (TryParse(value, out time, offsetIndex, out hours, out minutes))
				{
					result = DateTimeExtensions.FromUnixTimeMilliseconds(time, new TimeSpan(hours: hours, minutes: minutes, seconds: 0));
					return true;
				}
				else
				{
					result = default(DateTimeOffset);
					return false;
				}
			}

			// LastIndexOf is faster than index of when searching for a char that is more likely to be at the end of string.
			offsetIndex = value.LastIndexOf('-', value.Length - 1);
			if (offsetIndex != -1)
			{
				// Read negative offset value
				int hours, minutes;
				if (TryParse(value, out time, offsetIndex, out hours, out minutes))
				{
					result = DateTimeExtensions.FromUnixTimeMilliseconds(time, new TimeSpan(hours: -hours, minutes: -minutes, seconds: 0));
					return true;
				}
				else
				{
					result = default(DateTimeOffset);
					return false;
				}
			}

			// No offset
			if (TryParse(value, out time))
			{
				result = DateTimeExtensions.FromUnixTimeMilliseconds(time, TimeSpan.Zero);
				return true;
			}
			else
			{
				result = default(DateTimeOffset);
				return false;
			}
		}

		private static bool TryParse(string value, out long timeMilliSeconds)
		{
			return long.TryParse(value.Substring(_prefixLength, value.Length - _prefixLength - 2), NumberStyles.None, CultureInfo.InvariantCulture, out timeMilliSeconds);
		}

		private static bool TryParse(string value, out long timeMilliSeconds, int offsetIndex, out int offsetHours, out int offsetMinutes)
		{
			// Reads time seconds : long between the prefix and the offset
			if (!long.TryParse(value.Substring(_prefixLength, offsetIndex - _prefixLength), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out timeMilliSeconds))
			{
				offsetHours = offsetMinutes = 0;
				return false;
			}

			if (value.Length - offsetIndex > 2 + 1 /* + or - symbol */ + 2 /* ending: ")/" */)
			{
				// Has hours AND minutes
				return int.TryParse(value.Substring(offsetIndex + 1, 2), NumberStyles.None, CultureInfo.InvariantCulture, out offsetHours)
					&  int.TryParse(value.Substring(offsetIndex + 3, 2), NumberStyles.None, CultureInfo.InvariantCulture, out offsetMinutes);
			}
			else
			{
				// Has only hours
				offsetMinutes = 0;
				return int.TryParse(value.Substring(offsetIndex + 1, 2), NumberStyles.None, CultureInfo.InvariantCulture, out offsetHours);
			}
		}
	}
}
