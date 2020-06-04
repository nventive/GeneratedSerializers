using System;

namespace GeneratedSerializers
{
	/// <summary>
	/// A JsonReader which read its content from a string
	/// </summary>
	public class StringJsonReader : JsonReader
	{
		private readonly string _json;
		private readonly int _length;
		private int _position;

		public StringJsonReader(string json)
		{
			_json = json;
			_length = json.Length;
		}

		public override char ReadChar()
		{
			if (_position == _length)
			{
				throw new FormatException("Reached end of json input.");
			}
			else
			{
				return _json[_position++];
			}
		} 
		public override bool TryReadChar(out char? c)
		{
			if (_position == _length)
			{
				c = null;
				return false;
			}
			else
			{
				c = _json[_position++];
				return true;
			}
		}
	}
}
