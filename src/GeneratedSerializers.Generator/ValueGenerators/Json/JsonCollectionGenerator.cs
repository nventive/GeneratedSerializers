using System;
using System.Linq;
using GeneratedSerializers.Extensions;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	/// <summary>
	/// A generator which can generate code for read or write JSON collection.
	///	<remarks>
	/// This handles the generation of write method for Collection and Dictionary, but handles generation of read of for the dictionaries. 
	/// Collection are handled by the JsonReaderGenerator or the the RecursiveStaticGeneratorSerializer.
	/// </remarks>
	/// </summary>
	public class JsonCollectionGenerator : IValueSerializationGenerator
	{
		private readonly ICollectionImplementationResolver _collectionResolver;

		public JsonCollectionGenerator(ICollectionImplementationResolver collectionResolver)
		{
			_collectionResolver = collectionResolver;
		}

		public string GetRead(string target, IPropertySymbol targetProperty, IValueSerializationGeneratorContext context) => GetRead(
			$"{target}.{targetProperty.Name}",
			targetProperty.Type,
			context);

		public string GetWrite(string sourceName, string source, IPropertySymbol sourceProperty, IValueSerializationGeneratorContext context) => GetWrite(
			sourceName,
			$"{source}.{sourceProperty.Name}",
			sourceProperty.Type,
			context);

		public string GetRead(string target, ITypeSymbol targetType, IValueSerializationGeneratorContext context)
		{
			ITypeSymbol itemType;

			if (targetType.IsDictionary(out itemType)
				|| targetType.IsCollectionOfKeyValuePairOfString(out itemType))
			{
				return ReadDictionary(target, targetType, itemType, context);
			}
			else if (targetType.IsCollection(out itemType))
			{
				return ReadCollection(target, targetType, itemType, context);
			}
			else
			{
				return null;
			}
		}

		public string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, IValueSerializationGeneratorContext context)
		{
			ITypeSymbol itemType;

			if (sourceType.IsDictionary(out itemType)
				|| sourceType.IsCollectionOfKeyValuePairOfString(out itemType))
			{
				return WriteDictionary(sourceName, sourceCode, itemType, context);
			}
			else if (sourceType.IsCollection(out itemType))
			{
				return WriteCollection(sourceName, sourceCode, itemType, context);
			}
			else
			{
				return null;
			}
		}

		#region Read
		public string ReadDictionary(string target, ITypeSymbol dictionaryType, ITypeSymbol itemType, IValueSerializationGeneratorContext context)
		{
			var implementation = _collectionResolver.FindImplementation(dictionaryType);

			var dictionary = VariableHelper.GetName(implementation.Implementation);
			var key = VariableHelper.GetName<string>();

			// Note: the {context.Read.OverChar} is always null after the MoveToNextProperty so we don't need to propagate it as FirstChar
			//		 This means that we don't need to specify the .UsingOverAsFirstChar() on the context for context.GetRead($"{dictionary}[{key}]", itemType).

			return $@"
					if ({context.Read.Reader}.OpenObject({context.Read.FirstChar}, out {context.Read.OverChar}))
					{{
						{implementation.CreateInstance(dictionary)}
						var {key} = default(string);

						while ({context.Read.Reader}.MoveToNextProperty(ref {context.Read.OverChar}, ref {key}, toUpper: false))
						{{
							{context.IgnoringCurrentFirstChar().GetRead($"{dictionary}[{key}]", itemType)}
						}}

						{implementation.InstanceToContract(dictionary, target)}
					}}";
		}

		public string ReadCollection(string target, ITypeSymbol collectionType, ITypeSymbol itemType, IValueSerializationGeneratorContext context)
		{
			var itemTypeName = itemType.GetDeclarationGenericFullName();

			var implementation = _collectionResolver.FindImplementation(collectionType);

			var collection = VariableHelper.GetName(implementation.Implementation);
			var item = VariableHelper.GetName(itemType);
			var collectionKind = VariableHelper.GetName("collectionType");

			return $@"
					var {collectionKind} = {context.Read.Reader}.OpenCollection({context.Read.FirstChar}, out {context.Read.OverChar});
					if({collectionKind} == JsonReaderExtension.CollectionType.SingleValue)
					{{
						{implementation.CreateInstance(collection)}
						var {item} = default({itemTypeName});

						{context.UsingOverAsFirstChar(overHasValue: true).GetRead(item, itemType)}
						{implementation.AddItemToInstance(collection, item)}
						
						{implementation.InstanceToContract(collection, target)}
					}}
					else if({collectionKind} == JsonReaderExtension.CollectionType.Collection)
					{{
						// If collection type is Collection, then the overChar returned by OpenCollection is null.

						{implementation.CreateInstance(collection)}
						var {item} = default({itemTypeName});

						while ({context.Read.Reader}.MoveToNextCollectionItem(ref {context.Read.OverChar}))
						{{
							{context.UsingOverAsFirstChar().GetRead(item, itemType)}
							{implementation.AddItemToInstance(collection, item)}
						}}

						{implementation.InstanceToContract(collection, target)}
					}}";
		}
		#endregion

		#region Write
		public string WriteCollection(string sourceName, string sourceCode, ITypeSymbol itemType, IValueSerializationGeneratorContext context)
		{
			var itemTypeName = itemType.GetDeclarationGenericFullName();
			var value = VariableHelper.GetName(itemType);
			var enumerator = VariableHelper.GetName("enumerator");
			if (sourceName.IsNullOrWhiteSpace())
			{
				return $@"
					var {value} = {sourceCode};
					if ({value} == null)
					{{
						{context.Write.Writer}.WriteNullValue();
					}}
					else
					{{
						{context.Write.Writer}.Write('[');
						var {enumerator} = {value}.GetEnumerator();
						if ({enumerator}.MoveNext())
						{{
							{context.GetWrite(null, $"(({itemTypeName}){enumerator}.Current)", itemType)};
						}}

						while ({enumerator}.MoveNext())
						{{
							{context.Write.Writer}.Write(',');
							{context.GetWrite(null, $"(({itemTypeName}){enumerator}.Current)", itemType)};
						}}
						{context.Write.Writer}.Write(']');
					}}";
			}
			else
			{
				return $@"
					var {value} = {sourceCode};
					if ({value} != null)
					{{
						{context.Write.Object}.WritePropertyName(""{sourceName}"");
						{context.Write.Writer}.Write('[');
						var {enumerator} = {value}.GetEnumerator();
						if ({enumerator}.MoveNext())
						{{
							{context.GetWrite(null, $"(({itemTypeName}){enumerator}.Current)", itemType)};
						}}

						while ({enumerator}.MoveNext())
						{{
							{context.Write.Writer}.Write(',');
							{context.GetWrite(null, $"(({itemTypeName}){enumerator}.Current)", itemType)};
						}}
						{context.Write.Writer}.Write(']');
					}}";
			}


		}

		public static string WriteDictionary(string sourceName, string sourceCode, ITypeSymbol itemType, IValueSerializationGeneratorContext context)
		{
			var itemTypeName = $"System.Collections.Generic.KeyValuePair<string, {itemType.GetDeclarationGenericFullName()}>";
            var value = VariableHelper.GetName(itemType);
			var enumerator = VariableHelper.GetName("enumerator");
			if (sourceName.IsNullOrWhiteSpace())
			{
				return $@"
					var {value} = {sourceCode};
					if ({value} == null)
					{{
						{context.Write.Writer}.WriteNullValue();
					}}
					else
					{{
						{context.Write.Writer}.Write('{{');
						var {enumerator} = {value}.GetEnumerator();
						if ({enumerator}.MoveNext())
						{{
							{context.Write.Writer}.WritePropertyName((({itemTypeName}){enumerator}.Current).Key);
							{context.GetWrite(null, $"(({itemTypeName}){enumerator}.Current).Value", itemType)};
						}}

						while ({enumerator}.MoveNext())
						{{
							{context.Write.Writer}.Write(',');
							{context.Write.Writer}.WritePropertyName((({itemTypeName}){enumerator}.Current).Key);
							{context.GetWrite(null, $"(({itemTypeName}){enumerator}.Current).Value", itemType)};
						}}
						{context.Write.Writer}.Write('}}');
					}}";
			}
			else
			{
				return $@"
					var {value} = {sourceCode};
					if ({value} != null)
					{{
						{context.Write.Object}.WritePropertyName(""{sourceName}"");
						{context.Write.Writer}.Write('{{');
						var {enumerator} = {value}.GetEnumerator();
						if ({enumerator}.MoveNext())
						{{
							{context.Write.Writer}.WritePropertyName((({itemTypeName}){enumerator}.Current).Key);
							{context.GetWrite(null, $"(({itemTypeName}){enumerator}.Current).Value", itemType)};
						}}

						while ({enumerator}.MoveNext())
						{{
							{context.Write.Writer}.Write(',');
							{context.Write.Writer}.WritePropertyName((({itemTypeName}){enumerator}.Current).Key);
							{context.GetWrite(null, $"(({itemTypeName}){enumerator}.Current).Value", itemType)};
						}}
						{context.Write.Writer}.Write('}}');
					}}";
			}
		} 
		#endregion
	}
}
