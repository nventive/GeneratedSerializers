using System;
using System.IO;
using System.Text;

namespace GeneratedSerializers
{
	/// <summary>
	/// A JsonReader which read its content from a stream
	/// </summary>
	public class StreamJsonReader : JsonReader
	{
		private readonly Stream _stream;
		private readonly Decoder _decoder;
		private readonly int _bufferSize;

		private readonly byte[] _byteBuffer;
		private readonly char[] _charBuffer;

		private int _currentPosition;
		private int _maxPosition;
		private int _lastReadCount;

		public StreamJsonReader(Stream stream, Encoding encoding = null, int bufferSize = 4096)
		{
			_stream = stream;
			_decoder = (encoding ?? Encoding.UTF8).GetDecoder();
			if (stream.CanSeek && bufferSize > stream.Length)
			{
				bufferSize = (int)stream.Length;
			}
			if (bufferSize < 8)
			{
				bufferSize = 8;
			}

			_bufferSize = bufferSize;

			_charBuffer = new char[_bufferSize];
			_byteBuffer = new byte[_bufferSize];
		}

		public override char ReadChar()
		{
			if (_currentPosition >= _maxPosition)
			{
				ReadBuffer();

				if (_currentPosition >= _maxPosition)
				{
					throw new FormatException("Reached end of stream.");
				}
			}
			
			return _charBuffer[_currentPosition++];
		}

		public override bool TryReadChar(out char? c)
		{
			if (_currentPosition >= _maxPosition)
			{
				ReadBuffer();

				if (_currentPosition >= _maxPosition)
				{
					c = null;
					return false;
				}
			}

			c = _charBuffer[_currentPosition++];
			return true;
		}

		private void ReadBuffer()
		{
			_lastReadCount = _stream.Read(_byteBuffer, 0, _bufferSize);
			_maxPosition = _decoder.GetChars(_byteBuffer, 0, _lastReadCount, _charBuffer, 0);
			_currentPosition = 0;
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				_stream.Dispose(); 
			}
		}
	}
}
