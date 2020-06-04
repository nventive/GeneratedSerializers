using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace GeneratedSerializers
{
	public interface IStaticSerializerResolver
	{
		/// <summary>
		/// Determines if we are able to generate a resolution code for a given type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		bool IsResolvable(ITypeSymbol type);

		/// <summary>
		/// Gets the code to use to resolve the instance of <see cref="IStaticSerializer{T}"/>.
		/// </summary>
		string GetResolve(ITypeSymbol type);
	}

	/// <summary>
	/// A generator which is able to generate a <see cref="IStaticSerializer{T}"/> for a given T.
	/// </summary>
	public interface ISerializerGenerator
	{
		string Generate(ITypeSymbol type);
	}

	public class CompositeStaticSerializerResolver : IStaticSerializerResolver
	{
		private readonly IStaticSerializerResolver[] _resolvers;

		private ImmutableDictionary<ITypeSymbol, IStaticSerializerResolver> _resolverByType =
			ImmutableDictionary<ITypeSymbol, IStaticSerializerResolver>.Empty;

		public CompositeStaticSerializerResolver(params IStaticSerializerResolver[] resolvers)
		{
			_resolvers = resolvers;
		}

		private IStaticSerializerResolver GetResolver(ITypeSymbol type)
		{
			return ImmutableInterlocked.GetOrAdd(
				ref _resolverByType,
				type,
				t => _resolvers.First(r => r.IsResolvable(t)));
		}

		public bool IsResolvable(ITypeSymbol type)
		{
			return GetResolver(type) != null;
		}

		public string GetResolve(ITypeSymbol type)
		{
			var resolver = GetResolver(type) ?? throw new InvalidOperationException($"No resolver for type {type}.");
			return resolver.GetResolve(type);
		}
	}
}
