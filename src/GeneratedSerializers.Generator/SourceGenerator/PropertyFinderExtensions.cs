using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GeneratedSerializers.Extensions;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public static class PropertyFinderExtensions
	{
		public static IEnumerable<ITypeSymbol> GetNestedTypes(this IPropertyFinder finder, ITypeSymbol[] types)
		{
			var nestedTypes = new List<ITypeSymbol>(types);

			return types
				.OfType<INamedTypeSymbol>()
				.SelectMany(t => InnerGetNestedTypes(finder, t, nestedTypes));
		}

		private static IEnumerable<ITypeSymbol> InnerGetNestedTypes(IPropertyFinder finder, INamedTypeSymbol type, IList<ITypeSymbol> alreadyFoundTypes)
		{
			var writingProperties = finder.GetWritingProperties(type);
			var readingProperties = finder.GetReadingProperties(type);

			var firstLevelNestedTypes = writingProperties
				.Concat(readingProperties)
				.FilterTypes(alreadyFoundTypes);

			return firstLevelNestedTypes
				.Concat(firstLevelNestedTypes.OfType<INamedTypeSymbol>().SelectMany(t => InnerGetNestedTypes(finder, t, alreadyFoundTypes)));
		}

		public static IEnumerable<ITypeSymbol> GetNestedCustomDeserializerTypes(this IPropertyFinder finder, ITypeSymbol[] types)
		{
			var foundTypes = new List<ITypeSymbol>();
			var exploredTypes = new List<ITypeSymbol>(types);

			var sideEffect = types
				.OfType<INamedTypeSymbol>()
				.SelectMany(t => InnerGetNestedCustomDeserializerTypes(finder, t, foundTypes, exploredTypes))
				.ToArray(); //force execution of whole chain.

			return foundTypes;
		} 

		private static IEnumerable<ITypeSymbol> InnerGetNestedCustomDeserializerTypes(
			IPropertyFinder finder,
			INamedTypeSymbol type, 
			IList<ITypeSymbol> foundTypes, 
			IList<ITypeSymbol> exploredTypes
		)
		{
			var writingProperties = finder.GetWritingProperties(type);
			var readingProperties = finder.GetReadingProperties(type);

			var firstLevelNestedTypes = writingProperties
				.Concat(readingProperties)
				.Select(p =>
				{
					var t = p.Property.FindCustomDeserializerType();

					if (t == null)
					{
						return p;
					}

					foundTypes.AddDistinct(t);
					return null;

				})
				.Trim()
				.FilterTypes(exploredTypes);
				

			return firstLevelNestedTypes.Concat(
				firstLevelNestedTypes
					.OfType<INamedTypeSymbol>()
					.SelectMany(t => InnerGetNestedCustomDeserializerTypes(finder, t, foundTypes, exploredTypes))
					.OfType<ITypeSymbol>()
			);
		}

		private static ITypeSymbol[] FilterTypes(
			this IEnumerable<DeserializationPropertyInfo> source,
			IList<ITypeSymbol> exploredTypes)
		{
			var result = new List<ITypeSymbol>();

			foreach (var p in source)
			{
				var t = p.Property.Type;
				if (t == null)
				{
					continue;
				}

				var st = SimplifyType(t);

				if (st.TypeKind == TypeKind.Interface || st.IsAbstract || exploredTypes.Contains(st))
				{
					continue;
				}

				if (st.Kind == SymbolKind.ErrorType)
				{
					var error = st as IErrorTypeSymbol;

					throw new Exception($"Unable to get symbol {st} (for {t}): {error?.ToDisplayString()}");
				}

				exploredTypes.Add(st);

				result.Add(st);
			}

			return result.ToArray();
		}

		private static ImmutableDictionary<ITypeSymbol, ITypeSymbol> _simplifiedTypes = ImmutableDictionary<ITypeSymbol, ITypeSymbol>.Empty;

		private static ITypeSymbol SimplifyType(ITypeSymbol type)
		{
			ITypeSymbol FindSimplifyType(ITypeSymbol forType)
			{
				while (true)
				{
					if (forType.IsDictionary(out var t)
						|| forType.IsCollectionOfKeyValuePairOfString(out t)
						|| forType.IsCollection(out t)
						|| forType.IsNullable(out t))
					{
						forType = t;
						continue;
					}

					return forType;
				}
			}

			return ImmutableInterlocked.GetOrAdd(ref _simplifiedTypes, type, FindSimplifyType);
		}
	}
}
