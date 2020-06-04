using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeneratedSerializers
{
	public interface IPropertyFinder
	{
		IEnumerable<DeserializationPropertyInfo> GetWritingProperties(ITypeSymbol type);
		IEnumerable<DeserializationPropertyInfo> GetReadingProperties(ITypeSymbol type);

		string GetName(ISymbol symbol);
	}

	public class DeserializationPropertyInfo
	{
		public IPropertySymbol Property { get; set; }

		/// <summary>
		/// The name of the property OR the custom name for the property (if the property has a JsonProperty attibute)
		/// </summary>
		public string PropertyName { get; set; }
	}
}
