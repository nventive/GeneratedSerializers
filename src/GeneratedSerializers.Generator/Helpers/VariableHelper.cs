using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Uno.Extensions;

namespace GeneratedSerializers
{
	public static class VariableHelper
	{
		public static readonly Regex _arrayRegex = new Regex(@"(?<type>[^,\[\]]+)\[\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static readonly Regex _genericRegex = new Regex(@"(?<genericType>[^\[]+)`[0-9]+\[(?<genericParameterTypes>[^\[\]]+)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static readonly Regex _invalidCharsRegex = new Regex(@"[^[0-9][a-z]_]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		// (\s*(?<genericParameterTypes>[^,\[\]]+),?\s?)+

		private static readonly ConcurrentDictionary<string, int> _counts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		public static string GetName<T>() => GetName(typeof(T).Name);

		public static string GetName(string type = null)
		{
			if (type == null)
			{
				type = "__anonymous__";
			}
			else
			{
				while (true)
				{
					if (_arrayRegex.IsMatch(type))
					{
						type = _arrayRegex.Replace(type, m => $"Array_Of_{m.Groups["type"].Value.Split('.').Last()}");
						continue;
					}

					if (_genericRegex.IsMatch(type))
					{
						type = _genericRegex.Replace(type, m => $"{m.Groups["genericType"].Value}_Of_{m.Groups["genericParameterTypes"].Value.Split(',').Select(t => t.Split('.').Last().Trim()).JoinBy("_")}");
						continue;
					}

					type = _invalidCharsRegex.Replace(type, "_");
					break;
				}


			}

			return $"__{type}_{_counts.AddOrUpdate(type, _ => 0, (_, c) => c + 1)}";
		}

		public static string GetName(ITypeSymbol type)
		{
			var typeName = type == null 
				? "__anonymous__" 
				: type.GetSerializedGenericName();

			typeName = _invalidCharsRegex.Replace(typeName, "_");

			return $"__{typeName}_{_counts.AddOrUpdate(typeName, _ => 0, (_, c) => c + 1)}";
		}
	}
}
