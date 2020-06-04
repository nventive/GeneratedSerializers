using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratedSerializers
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class SerializationPropertyAttribute : Attribute
    {
		public SerializationPropertyAttribute()
		{
		}

		public SerializationPropertyAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; set; }
    }
}
