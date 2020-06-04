using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public static class SymbolExtensions
	{
		private const string CustomDeserializerAttributeName = "CustomDeserializerAttribute";

		public static IReadOnlyDictionary<INamedTypeSymbol, INamedTypeSymbol> KnownCustomDeserializers
		{
			get;
			internal set;
		}

		public static INamedTypeSymbol FindCustomDeserializerType(this ISymbol symbol)
		{
			var attribute = symbol.FindAttributeByShortName(CustomDeserializerAttributeName);

			if (attribute == null)
			{
				return GetKnownCustomDeserializer(symbol);
			}

			var customDeserializerType = attribute.ConstructorArguments.Select(parameter => (INamedTypeSymbol)parameter.Value).FirstOrDefault()
				?? attribute.NamedArguments.Where(arg => arg.Key.Equals("type", StringComparison.OrdinalIgnoreCase)).Select(arg => (INamedTypeSymbol)arg.Value.Value).FirstOrDefault();

			return customDeserializerType ?? GetKnownCustomDeserializer(symbol);
		}

		private static INamedTypeSymbol GetKnownCustomDeserializer(
			ISymbol symbol)
		{
			INamedTypeSymbol symbolType;

			switch (symbol)
			{
				case IPropertySymbol property:
					symbolType = property.Type as INamedTypeSymbol;
					break;
				case IFieldSymbol field:
					symbolType = field.Type as INamedTypeSymbol;
					break;
				default:
					return null;
			}

			if (symbolType == null)
			{
				return null;
			}

			if (KnownCustomDeserializers.TryGetValue(symbolType.OriginalDefinition, out var deserializer))
			{
				return deserializer.IsGenericType
					? deserializer.Construct(symbolType.TypeArguments.ToArray())
					: deserializer;
			}

			return null;
		}
	}
}
