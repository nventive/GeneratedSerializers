using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	/// <summary>
	/// A resolver which can provide a type which implements another given type.
	/// </summary>
	public interface IImplementationResolver
	{
		/// <summary>
		/// Try to find a type to use to create an instance of a given type
		/// </summary>
		/// <param name="contract">The to type for a which an implementation is requested.</param>
		/// <returns>The type to use in order to create an instance of <paramref name="contract"/> if found, null otherwise.</returns>
		IImplementation FindImplementation(ITypeSymbol contract);
	}
}
