using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace GeneratedSerializers
{
	public interface IPropertySerializerGenerator
	{
		bool CanGenerateDeserializer(IPropertySymbol property);

		string GenerateDeserializer(IPropertySymbol property);


		bool CanGenerateSerializer(IPropertySymbol property);

		string GenerateSerializer(IPropertySymbol property);
	}
}
