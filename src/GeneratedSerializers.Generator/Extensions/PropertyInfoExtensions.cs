using Microsoft.CodeAnalysis;
using Uno.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace GeneratedSerializers
{
	public static class PropertyInfoExtensions
	{
		public static bool IsAttributePresent(this PropertyInfo property, string attributeClassFullname)
		{
			Attribute toSearchAttribute = null;
			CustomAttributeData reflectionOnlyToSearchAttribute = null;
			try
			{
				toSearchAttribute =
					property.GetCustomAttributes()
						.FirstOrDefault(p => p.GetType().FullName == attributeClassFullname);
			}
			catch (InvalidOperationException)
			{
				// This property belongs to a type that is loaded into the reflection-only context. 
				// GetCustomAttributes not working if using Reflection only API when loading assembly.
				toSearchAttribute = null;
				reflectionOnlyToSearchAttribute =
					CustomAttributeData.GetCustomAttributes(property)
						.FirstOrDefault(p => p.AttributeType.FullName == attributeClassFullname);
			}
			return toSearchAttribute != null || reflectionOnlyToSearchAttribute != null;
		}

		public static bool IsNullable(this IPropertySymbol property) => property.Type.IsNullable();

		public static bool IsNullable(this IPropertySymbol property, out ITypeSymbol nullableType) => property.Type.IsNullable(out nullableType);

		public static bool IsDictionary(this IPropertySymbol property) => property.Type.IsDictionary();
		
		public static bool IsDictionary(this IPropertySymbol property, out ITypeSymbol dictionaryDataType) => property.Type.IsDictionary(out dictionaryDataType);

		public static bool IsCollection(this IPropertySymbol property) => property.Type.IsCollection();

		public static bool IsCollection(this IPropertySymbol property, out ITypeSymbol collectionItemType) => property.Type.IsCollection(out collectionItemType);

		public static bool AreGetterAndSetterAvailable(this IPropertySymbol prop)
		{
			var getter = prop.GetMethod;
			var setter = prop.SetMethod;

			if (prop.IsReadOnly || prop.IsWriteOnly || getter == null || setter == null)
			{
				return false;
			}

			if (getter.DeclaredAccessibility == Accessibility.Private || getter.DeclaredAccessibility == Accessibility.Protected)
			{
				return false;
			}

			if (setter.DeclaredAccessibility == Accessibility.Private || setter.DeclaredAccessibility == Accessibility.Protected)
			{
				return false;
			}

			return true;
		}

		public static bool IsGetterAvailable(this IPropertySymbol prop)
		{
			var getter = prop.GetMethod;

			if (prop.IsWriteOnly || getter == null)
			{
				return false;
			}

			return getter.DeclaredAccessibility != Accessibility.Private && getter.DeclaredAccessibility != Accessibility.Protected;
		}
	}
}
