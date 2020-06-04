using System;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class CollectionImplementation : ICollectionImplementation
	{
		private readonly Func<string, string> _create;
		private readonly Func<string, string, string> _add;
		private readonly Func<string, string, string> _toContract;

		public CollectionImplementation(
			ITypeSymbol contract,
			ITypeSymbol implementation,
			Func<string, string> create,
			Func<string, string, string> add,
			Func<string, string, string> toContract)
		{
			Contract = contract;
			Implementation = implementation;

			_create = create;
			_add = add;
			_toContract = toContract;
		}

		public ITypeSymbol Contract { get; }

		public ITypeSymbol Implementation { get; }

		public string CreateInstance(string colectionVariable) => _create(colectionVariable);

		public string AddItemToInstance(string colectionVariable, string itemVariable) => _add(colectionVariable, itemVariable);

		public string InstanceToContract(string colectionVariable, string resultVariable) => _toContract(colectionVariable, resultVariable);
	}
}
