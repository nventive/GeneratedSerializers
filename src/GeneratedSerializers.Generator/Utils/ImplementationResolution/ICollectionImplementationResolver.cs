using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	/// <summary>
	/// A <seealso cref="IImplementationResolver"/> deciated to collection collections
	/// </summary>
	public interface ICollectionImplementationResolver : IImplementationResolver
	{
		/// <summary>
		/// Try to find a type to use to create an instance of a given type
		/// </summary>
		/// <param name="contract">The to type for a which an implementation is requested.</param>
		/// <returns>The type to use in order to create an instance of <paramref name="contract"/> if found, null otherwise.</returns>
		new ICollectionImplementation FindImplementation(ITypeSymbol contract);

		/// <summary>
		/// Gets the list of unbounded types that supported by this resolver.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IImplementation> GetUnboundedSupportedTypes();
	}
}
