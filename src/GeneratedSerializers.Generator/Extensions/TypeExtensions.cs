using System;
using System.Linq;
using Uno.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace GeneratedSerializers
{
	public static class TypeExtensions
	{
		private const string EnumerableTypeName = "System.Collections.Generic.IEnumerable";
		private const string DictionaryTypeName = "System.Collections.Generic.IDictionary";
		private const string ReadOnlyDictionaryTypeName = "System.Collections.Generic.IReadOnlyDictionary";
		private const string KeyValuePairTypeName = "System.Collections.Generic.KeyValuePair";

		public static bool IsDictionary(this ITypeSymbol type)
		{
			ITypeSymbol dictionaryType;
			return type.IsDictionary(out dictionaryType);
		}

		public static bool IsDictionary(this ITypeSymbol type, out ITypeSymbol dictionaryDataType)
		{
			var dictionaryKeyType = type.DictionaryKeyType();
			dictionaryDataType = type.DictionaryDataType();

			return (dictionaryKeyType != null && dictionaryDataType != null && dictionaryKeyType.ToDisplayString() == "string");
		}

		public static bool IsCollectionOfKeyValuePairOfString(this ITypeSymbol type)
		{
			ITypeSymbol _;
			return type.IsCollectionOfKeyValuePairOfString(out _);
		}

		public static bool IsCollectionOfKeyValuePairOfString(this ITypeSymbol type, out ITypeSymbol dictionaryDataType)
		{
			ITypeSymbol itemType;
			INamedTypeSymbol itemNamedType;
			if (IsCollection(type, out itemType)
				&& (itemNamedType = itemType as INamedTypeSymbol) != null
				&& itemNamedType.IsGenericType
			    && itemNamedType.ToDisplayString().StartsWith(KeyValuePairTypeName, StringComparison.OrdinalIgnoreCase)
			    && itemNamedType.TypeArguments[0].ToDisplayString() == "string")
			{
				dictionaryDataType = itemNamedType.TypeArguments[1];
				return true;
			}
			else
			{
				dictionaryDataType = null;
				return false;
			}
		}

		public static bool IsCollection(this ITypeSymbol type)
		{
			ITypeSymbol collectionType;

			return type.IsCollection(out collectionType);
		}

		public static bool IsCollection(this ITypeSymbol type, out ITypeSymbol collectionItemType)
		{
			collectionItemType = type.GetCollectionTypeArguments();

			return (collectionItemType != null && type.ToDisplayString() != "string");
		}

		private static ITypeSymbol GetCollectionTypeArguments(this ITypeSymbol type)
		{
			if (type != null)
			{
				return type
					.GetAllInterfaces(includeCurrent: true)
					.FirstOrDefault(i => i.IsGenericType
						&& i.ToDisplayString().StartsWith(EnumerableTypeName, StringComparison.OrdinalIgnoreCase))
					?.TypeArguments
					.First();
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Get the TValue of a type who implements IDictionary&lt;TKey,TValue&gt;.
		/// </summary>
		/// <returns>The type of TValue or null if <see cref="type"/> is not IDictionary</returns>
		public static ITypeSymbol DictionaryDataType(this ITypeSymbol type)
		{
			return GetDictionaryTypeArguments(type).ElementAtOrDefault(1);
		}

		/// <summary>
		/// Get the TKey of a type who implements IDictionary&lt;TKey,TValue&gt;.
		/// </summary>
		/// <returns>The type of TKey or null if <see cref="type"/> is not IDictionary</returns>
		public static ITypeSymbol DictionaryKeyType(this ITypeSymbol type)
		{
			return GetDictionaryTypeArguments(type).FirstOrDefault();
        }

		private static ImmutableArray<ITypeSymbol> GetDictionaryTypeArguments(this ITypeSymbol type)
		{
			if (type != null)
			{
				// Note: ReadOnlyDictionanry includes ImmutableDictionary
				return type
					.GetAllInterfaces(includeCurrent: true)
					.FirstOrDefault(i => i.IsGenericType 
						&& (i.ToDisplayString().StartsWith(DictionaryTypeName, StringComparison.OrdinalIgnoreCase) || i.ToDisplayString().StartsWith(ReadOnlyDictionaryTypeName, StringComparison.OrdinalIgnoreCase)))
					.SelectOrDefault(i => i.TypeArguments, ImmutableArray<ITypeSymbol>.Empty);
			}
			else
			{
				return ImmutableArray<ITypeSymbol>.Empty;
            }
		}
	}
}
