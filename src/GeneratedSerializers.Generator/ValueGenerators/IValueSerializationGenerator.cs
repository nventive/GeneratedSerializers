using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public interface IValueSerializationGeneratorContext
	{
		RoslynMetadataHelper Roselyn { get; }

		IValueSerializationGenerator ValueGenerator { get; }

		ReadContext Read { get; }

		WriteContext Write { get; }

		// TODO:
		///// <summary>
		///// Gets the property which for which we are generating (de)serialization code. WARNING, this might be null if we are at the root level (StaticSerializer)
		///// </summary>
		//IPropertySymbol Property { get; }
	}

	public class ReadContext
	{
		public ReadContext(string reader, string overChar)
			: this(reader, $"{reader}.ReadNonWhiteSpaceChar()", overChar)
		{
		}

		public ReadContext(string reader, string firstChar, string overChar)
		{
			Reader = reader;
			FirstChar = firstChar;
			OverChar = overChar;
		}

		public string Reader { get; }

		/// <summary>
		/// Gets the first char if any
		/// </summary>
		public string FirstChar { get; private set; }

		public string OverChar { get; }

		public ReadContext IgnoringCurrentFirstChar(bool overHasValue = false)
		{
			return new ReadContext(Reader, OverChar);
		}

		public ReadContext UsingOverAsFirstChar(bool overHasValue = false)
		{
			return new ReadContext(Reader, OverChar)
			{
				FirstChar = overHasValue
					? $"{OverChar}.Value"
					: $"{OverChar} ?? {Reader}.ReadNonWhiteSpaceChar()"
			};
		}
	}

	public class WriteContext
	{
		public WriteContext(string writer, string objectWriter = null)
		{
			Writer = writer;
			Object = objectWriter;
		}

		public string Writer { get; }

		public string Object { get; }
	}

	/// <summary>
	/// Something responsible to generated the code need to serialize or deserialize a value of a given type.
	/// </summary>
	public interface IValueSerializationGenerator
	{
		string GetRead(string target, IPropertySymbol targetProperty, IValueSerializationGeneratorContext context);

		string GetWrite(string sourceName, string source, IPropertySymbol sourceProperty, IValueSerializationGeneratorContext context);

		/// <summary>
		/// Try to generate a read method
		/// <example>target = (targetType)Reader.Read(targetType)</example>
		/// </summary>
		/// <param name="target">The target field where the generated code should store the read value.</param>
		/// <param name="targetType">The type of the target property.</param>
		/// <param name="context"></param>
		/// <returns>The source code of the read method, or null if this generaor cannot handle this type of value.</returns>
		string GetRead(string target, ITypeSymbol targetType, IValueSerializationGeneratorContext context);

		/// <summary>
		/// Try to generate a Write method
		/// <example>Writer.Write("sourceName = (sourceType)sourceCode").</example>
		/// </summary>
		/// <param name="sourceName">The property name of the value</param>
		/// <param name="sourceCode">The source coe which provides access to the value to write.</param>
		/// <param name="sourceType">The type of the value to write</param>
		/// <param name="context"></param>
		/// <returns>The source code of the write method, or null if this generator cannot handle this type of value.</returns>
		string GetWrite(string sourceName, string sourceCode, ITypeSymbol sourceType, IValueSerializationGeneratorContext context);
	}
}
