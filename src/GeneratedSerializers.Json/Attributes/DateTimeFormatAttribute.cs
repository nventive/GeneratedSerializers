using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratedSerializers
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class DateTimeFormatAttribute : Attribute
	{
		// This is a positional argument
		public DateTimeFormatAttribute(string format)
		{
			Format = format;
		}

		public DateTimeFormatAttribute(string format, DateTimeStyles styles)
		{
			Format = format;
			Styles = styles;
		}

		public DateTimeFormatAttribute(DateTimeStyles styles)
		{
			Styles = styles;
		}

		public string Format { get; }

		public DateTimeStyles? Styles { get; }
	}
}
