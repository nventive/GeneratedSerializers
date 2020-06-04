using System;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class JsonCustomDeserializerPropertyFinder : DefaultPropertyFinder
	{
		protected override bool IsAcceptableWritingProperty(IPropertySymbol prop)
		{
			if (!prop.AreGetterAndSetterAvailable())
			{
				return false;
			}

			//Since we are looking into custom deserialization, we are allowing interface and abstract types 
			//(the custom serializer will pick the right concrete type).
			return true;
		}

		protected override bool IsAcceptableReadingProperty(IPropertySymbol prop)
		{
			if (!prop.IsGetterAvailable())
			{
				return false;
			}

			//Since we are looking into custom deserialization, we are allowing interface and abstract types 
			//(the custom serializer will pick the right concrete type).
			return true;
		}
	}
}
