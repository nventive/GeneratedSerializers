using System;
using System.Collections.Generic;
using System.Linq;
using GeneratedSerializers.Extensions;

namespace GeneratedSerializers
{
	public static class WellKnownTypes
	{
		private static readonly string[] _systemTypes = new[]
		{
			"string",

			"long",
			"int",
			"short",
			"ulong",
			"uint",
			"ushort",
			"byte",
			"double",
			"float",
			"decimal",
			"bool",
			"System.DateTime",
			"System.DateTimeOffset",
			"System.TimeSpan",

			"long?",
			"int?",
			"short?",
			"ulong?",
			"uint?",
			"ushort?",
			"byte?",
			"double?",
			"float?",
			"decimal?",
			"bool?",
			"System.DateTime?",
			"System.DateTimeOffset?",
			"System.TimeSpan?",
		};

		private static readonly string[] _generics = new[]
		{
			"{0}[]",
			"System.Collections.Generic.IEnumerable<{0}>",
			"System.Collections.Generic.List<{0}>",
			"System.Collections.Generic.IDictionary<string,{0}>",
			"System.Collections.Generic.Dictionary<string,{0}>",
			"System.Collections.Generic.KeyValuePair<string,{0}>",
		};

		public static string[] Types { get; }

		static WellKnownTypes()
		{
			Types = _systemTypes
				.SelectMany(t => _generics.Select(g => g.InvariantCultureFormat(t)).Concat(t))
				.ToArray();
		}
	}
}
