using System;
using System.Collections.Generic;
using System.Linq;
using GeneratedSerializers.Extensions;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class DefaultPropertyFinder : IPropertyFinder
	{
		public IEnumerable<DeserializationPropertyInfo> GetWritingProperties(ITypeSymbol type)
		{
			return type
				.GetAllInstanceProperties()
				.Where(IsAcceptableWritingProperty)
				.Where(prop => !IsIgnored(prop))
				.Where(PassesSecondaryFilter)
				.Select(prop => new DeserializationPropertyInfo {Property = prop, PropertyName = GetName(prop)});
		}

		public IEnumerable<DeserializationPropertyInfo> GetReadingProperties(ITypeSymbol type)
		{
			return type
				.GetAllInstanceProperties()
				.Where(IsAcceptableReadingProperty)
				.Where(prop => !IsIgnored(prop))
				.Where(PassesSecondaryFilter)
				.Select(prop => new DeserializationPropertyInfo { Property = prop, PropertyName = GetName(prop) });
		}

		protected virtual bool IsAcceptableWritingProperty(IPropertySymbol prop)
		{
			if (!prop.AreGetterAndSetterAvailable())
			{
				return false;
			}

			// We should always generate serializer for collections (includes IEnumerable and IDictionary)
			if (prop.IsCollection()) //includes: || prop.IsDictionary())
			{
				return true;
			}

			return !prop.Type.IsAbstract 
				&& prop.Type.TypeKind != TypeKind.Interface;
		}

		protected virtual bool IsAcceptableReadingProperty(IPropertySymbol prop)
		{
			if (!prop.IsGetterAvailable())
			{
				return false;
			}

			// We should always generate serializer for collections (includes IEnumerable and IDictionary)
			if (prop.IsCollection()) //includes: || prop.IsDictionary())
			{
				return true;
			}

			return !prop.Type.IsAbstract
				&& prop.Type.TypeKind != TypeKind.Interface;
		}

		protected virtual bool PassesSecondaryFilter(IPropertySymbol propInfo)
		{
			return true;
		}

		private static bool IsIgnored(IPropertySymbol propInfo)
		{
			return propInfo.FindAttributeByShortName("SerializationIgnoreAttribute") != null
				|| propInfo.FindAttribute("Newtonsoft.Json.JsonIgnoreAttribute") != null;
		}

		public string GetName(ISymbol property)
		{
			var attribute = property.FindAttributeByShortName("SerializationPropertyAttribute")
				?? property.FindAttribute("Newtonsoft.Json.JsonPropertyAttribute");

			if (attribute == null)
			{
				return property.Name;
			}

			return attribute.ConstructorArguments.Select(parameter => (string)parameter.Value).FirstOrDefault()
				?? attribute.NamedArguments.Safe().Where(arg => arg.Key == "Name").Select(arg => (string)arg.Value.Value).FirstOrDefault()
				?? attribute.NamedArguments.Safe().Where(arg => arg.Key == "PropertyName").Select(arg => (string)arg.Value.Value).FirstOrDefault()
				?? property.Name;
		}
	}
}
