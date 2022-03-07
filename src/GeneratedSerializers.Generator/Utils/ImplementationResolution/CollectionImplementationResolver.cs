using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GeneratedSerializers.Extensions;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class CollectionImplementationResolver : ICollectionImplementationResolver
	{
		private readonly RoslynMetadataHelper _roslyn;

		public CollectionImplementationResolver(RoslynMetadataHelper roslyn, bool preferImmutableCollections = false)
		{
			_roslyn = roslyn;
		}

		#region Collection implementation
		private CollectionImplementation Array(IArrayTypeSymbol contract)
		{
			ITypeSymbol implementation, itemType;
			Func<string, string> toArray;
			if (contract.IsCollectionOfKeyValuePairOfString(out itemType))
			{
				implementation = ((INamedTypeSymbol)_roslyn.GetTypeByFullName(typeof(Dictionary<,>).FullName)).Construct(_roslyn.GetTypeByFullName("string"), itemType);
				toArray = c => $"System.Linq.Enumerable.ToArray({c})";
			}
			else
			{
				implementation = ((INamedTypeSymbol)_roslyn.GetTypeByFullName(typeof(List<>).FullName)).Construct(contract.ElementType);
				toArray = c => $"{c}.ToArray()";
			}

			return new CollectionImplementation(
				contract: contract,
				implementation: contract, // The final implemention type is an array, not the intermediate type (i.e. List or Dic)
				create: c => $"var {c} = new {implementation.GetDeclarationGenericFullName()}();",
				add: (c, i) => $"{c}.Add({i});",
				toContract: (c, r) => $"{r} = {toArray(c)};");
		}

		private CollectionImplementation Default(ITypeSymbol contract, ITypeSymbol implementation = null)
		{
			implementation = implementation ?? contract;

			if (implementation.IsAbstract
				|| implementation.TypeKind == TypeKind.Interface)
			{
				throw new InvalidOperationException("Cannot create a not delegated type for an interface / abstract class.");
			}

			return new CollectionImplementation(
				contract: contract,
				implementation: implementation,
				create: c => $"var {c} = new {implementation.GetDeclarationGenericFullName()}();",
				add: (c, i) => $"{c}.Add({i});",
				toContract: (c, r) => $"{r} = {c};");
		}

		private CollectionImplementation ImmutableUsingBuilder(ITypeSymbol contract, INamedTypeSymbol implementation)
		{
			var nonGenericImmutableType = $"{implementation.ContainingNamespace.GetFullName()}.{implementation.Name}";
			var typeArguments = implementation.TypeArguments.Select(t => t.GetDeclarationGenericFullName()).JoinBy(",");

			return new CollectionImplementation(
				contract: contract,
				implementation: implementation,
				create: c => $"var {c} = {nonGenericImmutableType}.CreateBuilder<{typeArguments}>();",
				add: (c, i) => $"{c}.Add({i});",
				toContract: (c, r) => $"{r} = {c}.ToImmutable();");
		}

		private Func<INamedTypeSymbol, INamedTypeSymbol, CollectionImplementation> ImmutableFromEnumerable(bool reverse = false) => (contract, implementation) =>
		{
			var tempList = ((INamedTypeSymbol)_roslyn.GetTypeByFullName(typeof(List<>).FullName)).Construct(implementation.TypeArguments.ToArray());
			var nonGenericImmutableType = $"{implementation.ContainingNamespace.GetFullName()}.{implementation.Name}";
			var typeArguments = implementation.TypeArguments.Select(t => t.GetDeclarationGenericFullName()).JoinBy(",");

			return new CollectionImplementation(
				contract: contract,
				implementation: implementation,
				create: c => $"var {c} = new {tempList.GetDeclarationGenericFullName()}();",
				add: (c, i) => $"{c}.Add({i});",
				toContract: (c, r) =>
					(reverse ? $"{c}.Reverse();\r\n" : "")
					+ $"{r} = {nonGenericImmutableType}.CreateRange<{typeArguments}>({c});");
		};
		#endregion


		public IEnumerable<IImplementation> GetUnboundedSupportedTypes() => SupportedTypes.Where(c => c.IsValid);

		private TypeConfig[] SupportedTypes => new TypeConfig[]
		{
			// List
			new TypeConfig(_roslyn, typeof (List<>), typeof (List<>), Default),
			new TypeConfig(_roslyn, typeof (IEnumerable<>), typeof (List<>), Default),
			new TypeConfig(_roslyn, typeof (ICollection<>), typeof (List<>), Default),
			new TypeConfig(_roslyn, typeof (IReadOnlyCollection<>), typeof (List<>), Default),
			new TypeConfig(_roslyn, typeof (IList<>), typeof (List<>), Default),
			new TypeConfig(_roslyn, typeof (IReadOnlyList<>), typeof (List<>), Default),

			// Dictionary
			new TypeConfig(_roslyn, typeof (Dictionary<,>), typeof (Dictionary<,>), Default),
			new TypeConfig(_roslyn, typeof (IDictionary<,>), typeof (Dictionary<,>), Default),
			new TypeConfig(_roslyn, typeof (IReadOnlyDictionary<,>), typeof (Dictionary<,>), Default),

			// Immutables
			new TypeConfig(_roslyn, typeof (ImmutableArray<>), typeof (ImmutableArray<>), ImmutableUsingBuilder),

			new TypeConfig(_roslyn, typeof (ImmutableHashSet<>), typeof (ImmutableHashSet<>), ImmutableUsingBuilder),
			new TypeConfig(_roslyn, typeof (IImmutableSet<>), typeof (ImmutableHashSet<>), ImmutableUsingBuilder),

			new TypeConfig(_roslyn, typeof (ImmutableList<>), typeof (ImmutableList<>), ImmutableUsingBuilder),
			new TypeConfig(_roslyn, typeof (IImmutableList<>), typeof (ImmutableList<>), ImmutableUsingBuilder),

			new TypeConfig(_roslyn, typeof (ImmutableQueue<>), typeof (ImmutableQueue<>), ImmutableFromEnumerable()),
			new TypeConfig(_roslyn, typeof (IImmutableQueue<>), typeof (ImmutableQueue<>), ImmutableFromEnumerable()),

			new TypeConfig(_roslyn, typeof (ImmutableSortedSet<>), typeof (ImmutableSortedSet<>), ImmutableUsingBuilder),

			new TypeConfig(_roslyn, typeof (ImmutableStack<>), typeof (ImmutableStack<>), ImmutableFromEnumerable(reverse: true)),
			new TypeConfig(_roslyn, typeof (IImmutableStack<>), typeof (ImmutableStack<>), ImmutableFromEnumerable(reverse: true)),

			// Immutable dictionaries
			new TypeConfig(_roslyn, typeof (ImmutableDictionary<,>), typeof (ImmutableDictionary<,>), ImmutableUsingBuilder),
			new TypeConfig(_roslyn, typeof (IImmutableDictionary<,>), typeof (ImmutableDictionary<,>), ImmutableUsingBuilder),

			new TypeConfig(_roslyn, typeof (ImmutableSortedDictionary<,>), typeof (ImmutableSortedDictionary<,>), ImmutableUsingBuilder),
		};
		

		IImplementation IImplementationResolver.FindImplementation(ITypeSymbol type) => FindImplementation(type);
		public ICollectionImplementation FindImplementation(ITypeSymbol type)
		{
			var array = type as IArrayTypeSymbol;
			if (array != null)
			{
				return Array(array);
			}

			var namedType = type as INamedTypeSymbol;
			if (namedType != null && namedType.IsGenericType && namedType.IsCollection())
			{
				// If we have an IEnumerable<KeyValuePair<string, T>>, we override the requested type to IDictionary<string, T>
				// (Otherwise it we use List<KeyValuePair<string, T>> and we will ne be able access it by key for add items
				ITypeSymbol itemType;
				if (!namedType.IsDictionary()
				    && namedType.IsCollectionOfKeyValuePairOfString(out itemType))
				{
					namedType = ((INamedTypeSymbol)_roslyn.GetTypeByFullName(typeof(Dictionary<,>).FullName)).Construct(_roslyn.GetTypeByFullName("string"), itemType);
				}

				// Search for a well known implementation
				return SupportedTypes.FirstOrDefault(t => t.IsValid && t.CanImplement(namedType))?.GetImplementation(namedType) 
					?? Default(namedType);
			}

			return null;
		}

		private class TypeConfig : IImplementation
		{
			private readonly Type _contractType;
			private readonly INamedTypeSymbol _contract;
			private readonly string _contractFullName;
			private readonly Type _implementationType;
			private readonly INamedTypeSymbol _implementation;
			private readonly Func<INamedTypeSymbol, INamedTypeSymbol, CollectionImplementation> _implementationFactory;

			public TypeConfig(RoslynMetadataHelper roslyn, Type contract, Type implementation, Func<INamedTypeSymbol, INamedTypeSymbol, CollectionImplementation> implementationFactory)
			{
				_contractType = contract;
				_implementationType = implementation;
				_implementationFactory = implementationFactory;

				_contract = (INamedTypeSymbol)roslyn.FindTypeByFullName(contract.FullName);
				_implementation = (INamedTypeSymbol)roslyn.FindTypeByFullName(implementation.FullName);

				_contractFullName = _contract?.ConstructUnboundGenericType().ToDisplayString();
			}

			public bool IsValid => _contract != null && _implementation != null;

			/// <summary>
			/// Unbounded type of the contract
			/// </summary>
			public ITypeSymbol Contract => _contract;

			/// <summary>
			/// Unbounded type of the implementation to use in order to create an instance of <see cref="Contract"/>.
			/// </summary>
			public ITypeSymbol Implementation => _implementation;

			/// <summary>
			/// Indicates if this collection is a dictionary or not
			/// </summary>
			public bool IsDictionary => _implementationType.FullName.EndsWith("`2");

			internal virtual bool CanImplement(INamedTypeSymbol requestedType) => _contractFullName == requestedType.ConstructUnboundGenericType().ToDisplayString();

			internal virtual CollectionImplementation GetImplementation(INamedTypeSymbol requestedType) => _implementationFactory(requestedType, _implementation.Construct(requestedType.TypeArguments.ToArray()));
		}
	}
}
