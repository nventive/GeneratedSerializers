using System;

namespace GeneratedSerializers
{
	/// <summary>
	/// Flags a type as JSON Serializable, used by the Static Serializers Code Generator.
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
	public sealed class JsonSerializableTypeAttribute : System.Attribute
	{
		private readonly Type type;
		private readonly object fallbackValueOverride;

		/// <summary>
		/// Creates a new instance of the <see cref="JsonSerializableTypeAttribute"/>.
		/// </summary>
		/// <param name="type">The type to be marked as serializable</param>
		public JsonSerializableTypeAttribute(Type type)
		{
			this.type = type;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type">The type to be marked as serializable</param>
		/// <param name="fallbackValueOverride">The value to override <see cref="FallbackValueAttribute"/>.</param>
		public JsonSerializableTypeAttribute(Type type, object fallbackValueOverride)
		{
			this.type = type;
			this.fallbackValueOverride = type;
		}
	}
}
