using System;
using System.Collections.Generic;
using System.Text;
using Uno;
using Uno.Equality;

namespace GeneratedSerializers
{
	/// <summary>
	/// Specialized type to hold RFC 7519 JSON Web Token (JWT) information.
	/// </summary>
	/// <typeparam name="TPayload">The type of the Payload to deserialize</typeparam>
	[GeneratedEquality]
	[Immutable]
	public partial class JwtData<TPayload>
		where TPayload : class
	{
		[EqualityIgnore]
		private readonly IObjectSerializer _jsonSerializer;

		[EqualityIgnore]
		private IDictionary<string, string> _header;

		[EqualityIgnore]
		private TPayload _payload;

		/// <summary>
		/// Initialize a new JWT.
		/// </summary>
		/// <remarks>
		/// Header & Payload will be deserialized only of a JSON serializer is supplied.
		/// </remarks>
		/// <param name="token">The raw token.</param>
		/// <param name="jsonSerializer">Should be a JSON serializer. Using a serializer for another format won't be RFC 7519 compliant.</param>
		public JwtData(string token, IObjectSerializer jsonSerializer = null)
		{
			_jsonSerializer = jsonSerializer;
			Token = token;

			var parts = token?.Split(new[] { '.' });
			RawHeader = parts?.Length > 0 ? Base64DecodeToString(parts[0]) : null;
			RawPayload = parts?.Length > 1 ? Base64DecodeToString(parts[1]) : null;
			Signature = parts?.Length > 2 ? Base64Decode(parts[2]) : null;
		}

		/// <summary>
		/// Represents the raw token as received as constructor parameter
		/// </summary>
		[EqualityHash]
		[EqualityComparerOptions(StringMode = StringComparerMode.EmptyEqualsNull)]
		public string Token { get; }

		/// <summary>
		/// Represents the decoded (but non-deserialized) header part of the JWT - in JSON text
		/// </summary>
		[EqualityIgnore]
		public string RawHeader { get; }

		/// <summary>
		/// Represents the decoded (but non-deserialized) payload part of the JWT - in JSON text
		/// </summary>
		[EqualityIgnore]
		public string RawPayload { get; }

		/// <summary>
		/// Deserialized header
		/// </summary>
		[EqualityIgnore]
		public IDictionary<string, string> Header
			=> _header ?? (_header = _jsonSerializer?.FromString(RawHeader, typeof(IDictionary<string, string>)) as IDictionary<string, string>);

		/// <summary>
		/// Deserialized payload
		/// </summary>
		[EqualityIgnore]
		public TPayload Payload
			=> _payload ?? (_payload = _jsonSerializer?.FromString(RawPayload, typeof(TPayload)) as TPayload);

		/// <summary>
		/// Decoded signature of the JWT
		/// </summary>
		[EqualityIgnore]
		public byte[] Signature { get; }

		private static string Base64DecodeToString(string input)
		{
			return Encoding.UTF8.GetString(Base64Decode(input));
		}

		private static byte[] Base64Decode(string input)
		{
			var output = input?.Replace('-', '+').Replace('_', '/') ?? string.Empty;

			switch (output.Length % 4) // Pad with trailing '='s
			{
				case 0: break; // No pad chars in this case
				case 2:
					output += "==";
					break; // Two pad chars
				case 3:
					output += "=";
					break; // One pad char
				default:
					throw new ArgumentException("Illegal base64url string!", nameof(input));
			}

			return Convert.FromBase64String(output);
		}
	}
}
