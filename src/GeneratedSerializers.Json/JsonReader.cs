using System;

namespace GeneratedSerializers
{
	/// <summary>
	/// A reader to decicated to read Json content
	/// </summary>
	public abstract class JsonReader : IDisposable
	{
		public abstract char ReadChar();

		public abstract bool TryReadChar(out char? c);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
		}

		~JsonReader()
		{
			Dispose(false);
		}
	}
}
