using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	/// <summary>
	/// Delegate the implementation of a type to an other.
	/// </summary>
	public interface IImplementation
	{
		/// <summary>
		/// The requested type
		/// </summary>
		ITypeSymbol Contract { get; }

		/// <summary>
		/// The constructible type to use in order to create an instance od <see cref="Contract"/>.
		/// </summary>
		ITypeSymbol Implementation { get; }
	}

	public class Implemtation : IImplementation
	{
		public Implemtation(ITypeSymbol contractAndImplementation)
		{
			Contract = contractAndImplementation;
			Implementation = contractAndImplementation;
		}

		public Implemtation(ITypeSymbol contract, ITypeSymbol implementation)
		{
			Contract = contract;
			Implementation = implementation;
		}


		/// <inheritdoc/>
		public ITypeSymbol Contract { get; }

		/// <inheritdoc/>
		public ITypeSymbol Implementation { get; }
	}
}
