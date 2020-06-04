using System;
using System.Linq;

namespace GeneratedSerializers
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class CustomDeserializerAttribute : SerializationCustomAttribute
	{
		public CustomDeserializerAttribute(Type type) : base(type)
		{
		}
	}
}
