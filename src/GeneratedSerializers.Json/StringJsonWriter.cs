using System;
using System.Linq;
using System.Text;

namespace GeneratedSerializers
{
	/// <summary>
	/// A JsonWriter which writes the content to a Stream
	/// </summary>
	public sealed class StringJsonWriter : JsonWriter
	{
		private readonly StringBuilder _builder;

		/// <summary>
		/// Creates StringJsonWriter with a default <see cref="StringBuilder"/>.
		/// </summary>
		public StringJsonWriter()
			: this(new StringBuilder())
		{
		}

		/// <summary>
		/// Creates StringJsonWriter over a given <see cref="StringBuilder"/>.
		/// </summary>
		public StringJsonWriter(StringBuilder builder)
		{
			_builder = builder;
		}

		/// <inheritdoc/>
		public override void Write(char value) => _builder.Append(value);

		// Note: This method is not required, but as we writes lots of string when writting Json, create a short path
		/// <inheritdoc/>
		public override void Write(string value) => _builder.Append(value);

		/// <summary>
		/// Get the written Json
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return _builder.ToString();
		}
	}
}
