using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public static class TypeExtensions2
	{
		public static string GetSerializedGenericFullName(this ITypeSymbol type)
		{
			var arrayType = type as IArrayTypeSymbol;
			if (arrayType != null && arrayType.ElementType is ITypeSymbol)
			{
				return "ArrayOf{0}".InvariantCultureFormat(arrayType.ElementType.GetSerializedGenericFullName());
			}

			var namedType = type as INamedTypeSymbol;
			var typeName = type.Name;

			var ct = type;

			while (ct.ContainingType != null)
			{
				typeName = $"{ct.ContainingType.Name}_{typeName}";
				ct = ct.ContainingType;
			}

			if (namedType != null 
				&& namedType.IsGenericType 
				&& !namedType.IsUnboundGenericType
			)
			{
				return "{0}Of{1}".InvariantCultureFormat(
					typeName,
					namedType.TypeArguments
						.OfType<ITypeSymbol>()
						.Select(t => t.GetSerializedGenericFullName())
						.JoinBy("_")
					);
			}

			return (type.ContainingNamespace?.ToDisplayString()?.Replace(".", string.Empty).Replace("<global namespace>", "").Append("_") ?? string.Empty) + typeName;
		}

		public static string GetSerializedGenericName(this ITypeSymbol type)
		{
			var arrayType = type as IArrayTypeSymbol;
			if (arrayType != null && arrayType.ElementType is ITypeSymbol)
			{
				return "Array_Of_{0}".InvariantCultureFormat(arrayType.ElementType.GetSerializedGenericName());
			}

			var namedType = type as INamedTypeSymbol;
			if (namedType != null
				&& namedType.IsGenericType
				&& !namedType.IsUnboundGenericType
			)
			{
				return "{0}_Of_{1}".InvariantCultureFormat(
					type.Name,
					namedType
						.TypeArguments
						.OfType<ITypeSymbol>()
						.Select(t => t.GetSerializedGenericName())
						.JoinBy("_")
					);
			}

			return type.Name;
		}

		public static string GetDeclarationGenericName(this ITypeSymbol type)
		{
			var arraySymbol = type as IArrayTypeSymbol;
			if (arraySymbol != null)
			{
				return "{0}[]".InvariantCultureFormat(arraySymbol.ElementType.GetDeclarationGenericName());
			}

			return type.ToDisplayString();
		}

		private static readonly Dictionary<string, string>  _fullNamesMaping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{"string",     typeof(string).ToString()},
			{"long",       typeof(long).ToString()},     
			{"int",        typeof(int).ToString()},      
			{"short",      typeof(short).ToString()},    
			{"ulong",      typeof(ulong).ToString()},    
			{"uint",       typeof(uint).ToString()},     
			{"ushort",     typeof(ushort).ToString()},   
			{"byte",       typeof(byte).ToString()},     
			{"double",     typeof(double).ToString()},   
			{"float",      typeof(float).ToString()},    
			{"decimal",    typeof(decimal).ToString()},  
			{"bool",       typeof(bool).ToString()},
		};

		/// <summary>
		/// Retrun a string that can be used to declare a field of a the given type, using only full namespaces.
		/// <remarks>For NON GENERIC TYPES, string returned are the same that returned by typeof(T).ToString()</remarks>
		/// <remarks>Generic types will be returned using '&lt;' and '&gt;' (eg. System.Collections.Generic.IEnumerable&lt;System.Int32&gt;).</remarks>
		/// <remarks>Array are returned using the "[]" (eg. System.Int32[]).</remarks>
		/// <remarks>System types (including nullable) like "int", "int?", "int[]" and "int?[]" will be mapped to "System.Int32", "System.Nullable&lt;System.Int32&gt;", "System.Int32[]" and "System.Nullable&lt;System.Int32&gt;[]"</remarks>
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string GetDeclarationGenericFullName(this ITypeSymbol type)
		{
			var arraySymbol = type as IArrayTypeSymbol;
			if (arraySymbol != null)
			{
				return $"{arraySymbol.ElementType.GetDeclarationGenericFullName()}[]";
			}

			var name = type.ToDisplayString();

			if (name.EndsWith("?"))
			{
				name = name.Substring(0, name.Length - 1);
				return $"System.Nullable<{_fullNamesMaping.UnoGetValueOrDefault(name, name)}>";
			}
			else
			{
				return _fullNamesMaping.UnoGetValueOrDefault(name, name);
			}
		}


		public static IEnumerable<ITypeSymbol> GetTypeAndAllGenericArguments(this ITypeSymbol type)
		{
			var r = new [] {type};

			var namedType = type as INamedTypeSymbol;

			return namedType?.IsGenericType ?? false
				? r.Concat(namedType
					.TypeArguments
					.OfType<INamedTypeSymbol>()
					.SelectMany(t => t
						.GetTypeAndAllGenericArguments()
					)
				).Distinct()
				: r;
		}
	}
}
