using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneratedSerializers;
using GeneratedSerializers.Extensions;

namespace Microsoft.CodeAnalysis
{
	/// <summary>
	/// Roslyn symbol extensions
	/// </summary>
	public static class SymbolExtensions
	{
		public static IEnumerable<IPropertySymbol> GetProperties(this INamedTypeSymbol symbol) => symbol.GetMembers().OfType<IPropertySymbol>();

		public static IEnumerable<IEventSymbol> GetAllEvents(this INamedTypeSymbol symbol)
		{
			do
			{
				foreach (var member in GetEvents(symbol))
				{
					yield return member;
				}

				symbol = symbol.BaseType;

				if (symbol == null)
				{
					break;
				}

			} while (symbol.Name != "Object");
		}

		public static IEnumerable<IEventSymbol> GetEvents(INamedTypeSymbol symbol) => symbol.GetMembers().OfType<IEventSymbol>();

		/// <summary>
		/// Determines if the symbol inherits from the specified type.
		/// </summary>
		/// <param name="symbol">The current symbol</param>
		/// <param name="typeName">A potential base class.</param>
		public static bool Is(this INamedTypeSymbol symbol, string typeName)
		{
			do
			{
				if (symbol.ToDisplayString() == typeName)
				{
					return true;
				}

				symbol = symbol.BaseType;

				if (symbol == null)
				{
					break;
				}

			} while (symbol.Name != "Object");

			return false;
		}

		/// <summary>
		/// Determines if the symbol inherits from the specified type.
		/// </summary>
		/// <param name="symbol">The current symbol</param>
		/// <param name="other">A potential base class.</param>
		public static bool Is(this INamedTypeSymbol symbol, INamedTypeSymbol other)
		{
			do
			{
				if (Equals(symbol, other))
				{
					return true;
				}

				symbol = symbol.BaseType;

				if (symbol == null)
				{
					break;
				}

			} while (symbol.Name != "Object");

			return false;
		}

		public static bool IsPublic(this ISymbol symbol) => symbol.DeclaredAccessibility == Accessibility.Public;

		/// <summary>
		/// Returns true if the symbol can be accessed from the current module
		/// </summary>
		public static bool IsLocallyPublic(this ISymbol symbol, IModuleSymbol currentSymbol) =>
			symbol.DeclaredAccessibility == Accessibility.Public 
			||
			(
				symbol.Locations.Any(l => Equals(l.MetadataModule, currentSymbol))
				&& symbol.DeclaredAccessibility == Accessibility.Internal
			);

		public static IEnumerable<IMethodSymbol> GetMethods(this INamedTypeSymbol resolvedType)
		{
			return resolvedType.GetMembers().OfType<IMethodSymbol>();
		}

		public static IEnumerable<IFieldSymbol> GetFields(this INamedTypeSymbol resolvedType)
		{
			return resolvedType.GetMembers().OfType<IFieldSymbol>();
		}

		public static IEnumerable<IFieldSymbol> GetFieldsWithAttribute(this ITypeSymbol resolvedType, string name)
		{
			return resolvedType
				.GetMembers()
				.OfType<IFieldSymbol>()
				.Where(f => f.FindAttribute(name) != null);
		}

		public static IEnumerable<IFieldSymbol> GetFieldsWithAttributeShortName(this ITypeSymbol resolvedType, string shortName)
		{
			return resolvedType
				.GetMembers()
				.OfType<IFieldSymbol>()
				.Where(f => f.FindAttributeByShortName(shortName) != null);
		}

		public static AttributeData FindAttribute(this ISymbol property, string attributeClassFullName)
		{
			return property.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == attributeClassFullName);
		}

		public static AttributeData FindAttributeByShortName(this ISymbol property, string attributeShortName)
		{
			return property.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == attributeShortName);
		}

		public static AttributeData FindAttribute(this ISymbol property, INamedTypeSymbol attributeClassSymbol)
		{
			return property.GetAttributes().FirstOrDefault(a => Equals(a.AttributeClass, attributeClassSymbol));
		}

		public static AttributeData FindAttributeFlattened(this ISymbol property, INamedTypeSymbol attributeClassSymbol)
		{
			return property.GetAllAttributes().FirstOrDefault(a => Equals(a.AttributeClass, attributeClassSymbol));
		}

		/// <summary>
		/// Returns the element type of the IEnumerable, if any.
		/// </summary>
		/// <param name="resolvedType"></param>
		/// <returns></returns>
		public static ITypeSymbol EnumerableOf(this ITypeSymbol resolvedType) => resolvedType.IsCollection(out var type) ? type : null;

		/// <summary>
		/// Returns the element type of the IDictionnary, if any.
		/// </summary>
		/// <param name="resolvedType"></param>
		/// <returns></returns>
		public static ITypeSymbol DictionnaryOf(this ITypeSymbol resolvedType) => resolvedType.IsDictionary(out var type) ? type : null;

		public static IEnumerable<INamedTypeSymbol> GetAllInterfaces(this ITypeSymbol symbol, bool includeCurrent = true)
		{
			if (symbol == null)
			{
				yield break;
			}

			if (includeCurrent && symbol.TypeKind == TypeKind.Interface)
			{
				yield return (INamedTypeSymbol)symbol;
			}

			do
			{
				foreach (var intf in symbol.Interfaces)
				{
					yield return intf;

					foreach (var innerInterface in intf.GetAllInterfaces())
					{
						yield return innerInterface;
					}
				}

				symbol = symbol.BaseType;

				if (symbol == null)
				{
					break;
				}

			} while (symbol.Name != "Object");
		}

		public static bool IsNullable(this ITypeSymbol type)
		{
			return ((type as INamedTypeSymbol)?.IsGenericType ?? false)
				&& type.OriginalDefinition.ToDisplayString().Equals("System.Nullable<T>", StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsNullable(this ITypeSymbol type, out ITypeSymbol nullableType)
		{
			if (type.IsNullable())
			{
				nullableType = ((INamedTypeSymbol)type).TypeArguments.First();
				return true;
			}

			nullableType = null;
			return false;
		}

		public static ITypeSymbol NullableOf(this ITypeSymbol type)
		{
			return type.IsNullable()
				? ((INamedTypeSymbol)type).TypeArguments.First()
				: null;
		}

		public static IEnumerable<INamedTypeSymbol> GetNamespaceTypes(this INamespaceSymbol sym)
		{
			foreach (var child in sym.GetTypeMembers())
			{
				yield return child;
			}

			foreach (var ns in sym.GetNamespaceMembers())
			{
				foreach (var child2 in GetNamespaceTypes(ns))
				{
					yield return child2;
				}
			}
		}
		private static readonly Dictionary<string, string> _fullNamesMaping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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

		public static string GetFullName(this INamespaceOrTypeSymbol type)
		{
			if (type is IArrayTypeSymbol arrayType)
			{
				return $"{arrayType.ElementType.GetFullName()}[]";
			}

			if ((type as ITypeSymbol).IsNullable(out var t))
			{
				return $"System.Nullable`1[{t.GetFullName()}]";
			}

			var name = type.ToDisplayString();
			
			return _fullNamesMaping.GeneratedSerializerGetValueOrDefault(name, name);
		}

		public static string GetFullMetadataName(this INamespaceOrTypeSymbol symbol)
		{
			ISymbol s = symbol;
			var sb = new StringBuilder(s.MetadataName);

			var last = s;
			s = s.ContainingSymbol;

			if (s == null)
			{
				return symbol.GetFullName();
			}

			while (!IsRootNamespace(s))
			{
				if (s is ITypeSymbol && last is ITypeSymbol)
				{
					sb.Insert(0, '+');
				}
				else
				{
					sb.Insert(0, '.');
				}
				sb.Insert(0, s.MetadataName);

				s = s.ContainingSymbol;
			}

			var namedType = symbol as INamedTypeSymbol;

			if (namedType?.TypeArguments.Any() ?? false)
			{
				var genericArgs = namedType.TypeArguments.Select(GetFullMetadataName).JoinBy(",");
				sb.Append($"[{ genericArgs }]");
			}

			return sb.ToString();
		}

		private static bool IsRootNamespace(ISymbol s) => s is INamespaceSymbol symbol && symbol.IsGlobalNamespace;

		/// <summary>
		/// Return attributes on the current type and all its ancestors
		/// </summary>
		public static IEnumerable<AttributeData> GetAllAttributes(this ISymbol symbol)
		{
			while (symbol != null)
			{
				foreach (var attribute in symbol.GetAttributes())
				{
					yield return attribute;
				}

				symbol = (symbol as INamedTypeSymbol)?.BaseType;
			}
		}

		/// <summary>
		/// Return properties of the current type and all of its ancestors
		/// </summary>
		public static IEnumerable<IPropertySymbol> GetAllInstanceProperties(this ITypeSymbol symbol)
		{
			while (symbol != null)
			{
				foreach (var property in symbol.GetMembers().OfType<IPropertySymbol>())
				{
					if (!property.IsStatic)
					{
						yield return property;
					}
				}

				symbol = symbol.BaseType;
			}
		}
	}
}
