using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	/// <summary>
	/// Base class to create generator for a given type
	/// </summary>
	public abstract class TypeGeneratorBase : IValueSerializationGenerator
	{
		private readonly string[] _supportedTypes;

		protected TypeGeneratorBase(params string[] supportedTypes)
		{
			_supportedTypes = supportedTypes;
		}

		protected TypeGeneratorBase(params Type[] supportedTypes)
			: this(supportedTypes.Select(t => t.ToString()).ToArray())
		{
		}

		public string GetRead(string target, IPropertySymbol targetProperty, IValueSerializationGeneratorContext context) => GetRead(
			$"{target}.{targetProperty.Name}", 
			targetProperty.Type,
			context);

		public string GetRead(string target, ITypeSymbol targetType, IValueSerializationGeneratorContext context)
		{
			if (_supportedTypes.Contains(targetType.GetDeclarationGenericFullName(), StringComparer.OrdinalIgnoreCase))
			{
				return Read(target, false, context);
			}
			else if (targetType.IsNullable(out targetType) && _supportedTypes.Contains(targetType.GetDeclarationGenericFullName(), StringComparer.OrdinalIgnoreCase))
			{
				return Read(target, true, context);
			}
			else
			{
				return null;
			}
		}

		public virtual string Read(string target, bool isNullable, IValueSerializationGeneratorContext context) => null;

		public string GetWrite(string sourceName, string source, IPropertySymbol sourceProperty, IValueSerializationGeneratorContext context) => GetWrite(
			sourceName,
			$"{source}.{sourceProperty.Name}",
			sourceProperty.Type,
			context);

		public string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, IValueSerializationGeneratorContext context)
		{
			if (_supportedTypes.Contains(sourceType.GetDeclarationGenericFullName(), StringComparer.OrdinalIgnoreCase))
			{
				return Write(sourceName, sourceCode, false, context);
			}
			else if (sourceType.IsNullable(out sourceType) && _supportedTypes.Contains(sourceType.GetDeclarationGenericFullName(), StringComparer.OrdinalIgnoreCase))
			{
				return Write(sourceName, sourceCode, true, context);
			}
			else
			{
				return null;
			}
		}

		public virtual string Write(string sourceName, string sourceCode, bool isNullable, IValueSerializationGeneratorContext context) => null;
	}

	/// <summary>
	/// Base class to create generator for a given type
	/// </summary>
	/// <typeparam name="TType"></typeparam>
	public abstract class TypeGeneratorBase<TType> : TypeGeneratorBase
	{
		protected TypeGeneratorBase()
			: base(typeof(TType))
		{
			
		}
	}
}
