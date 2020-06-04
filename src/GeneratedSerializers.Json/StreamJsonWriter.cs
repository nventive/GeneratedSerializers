using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratedSerializers
{
	/// <summary>
	/// A JsonWriter which writes the content to a Stream
	/// </summary>
	public sealed class StreamJsonWriter : JsonWriter
	{
		private const int _charsPerBuffer = 256;
		private const int _bytesPerBuffer = _charsPerBuffer*4; // With UTF8, the max bytes per char is 4

		private readonly char[] _charBuffer = new char[_charsPerBuffer];
		private readonly byte[] _byteBuffer = new byte[_bytesPerBuffer];

		private readonly Stream _stream;
		private readonly bool _canDisposeStream;
		private readonly Encoder _encoder;

		private int _position = 0;

		/// <summary>
		/// Creates a StreamJsonWriter over a given Stream
		/// </summary>
		/// <param name="stream">Stream to write content to.</param>
		/// <param name="canDisposeStream">Determines if the <paramref name="stream"/> should be disposed at end or not (i.e. when disposing this writer, does the stream should be disposed or not)</param>
		public StreamJsonWriter(Stream stream, bool canDisposeStream = true)
		{
			_stream = stream;
			_canDisposeStream = canDisposeStream;

			_encoder = Encoding.GetEncoder();
		}

		/// <inheritdoc/>
		public override void Write(char value)
		{
			_charBuffer[_position++] = value;

			if (_position == _charsPerBuffer)
			{
				var written = _encoder.GetBytes(_charBuffer, 0, _position, _byteBuffer, 0, false);
				_stream.Write(_byteBuffer, 0, written);

				_position = 0;
			}
		}

		/// <inheritdoc/>
		public override void Flush()
		{
			var written = _encoder.GetBytes(_charBuffer, 0, _position, _byteBuffer, 0, true);
			_stream.Write(_byteBuffer, 0, written);

			_position = 0;
		}

		/// <inheritdoc/>
		public override async Task FlushAsync()
		{
			var written = _encoder.GetBytes(_charBuffer, 0, _position, _byteBuffer, 0, true);
			await _stream.WriteAsync(_byteBuffer, 0, written);

			_position = 0;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Flush();

				if (_canDisposeStream)
				{
					_stream.Dispose(); 
				}
			}

			base.Dispose(disposing);
		}
	}
}
