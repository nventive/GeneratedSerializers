using System;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class JsonStaticDeserializerPropertyFinder : DefaultPropertyFinder
	{
		protected override bool PassesSecondaryFilter(IPropertySymbol propInfo)
		{
			return propInfo.FindCustomDeserializerType() == null;
		}
	}
}
