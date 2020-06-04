using System;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	public class GuidGenerator : TypeGeneratorBase<Guid>
	{
		public override string Read(string target, bool isNullable, IValueSerializationGeneratorContext context)
		{
			var guid = VariableHelper.GetName("guid");
			return $@"
				string {guid};
				{context.Read<string>(guid)}
				if (!string.IsNullOrWhiteSpace({guid}))
				{{
					{target} = Guid.Parse({guid});
				}}";
		}

		public override string Write(string sourceName, string sourceCode, bool isNullable, IValueSerializationGeneratorContext context)
		{
			return context.Write<string>(sourceName, isNullable 
				? $"{sourceCode}?.ToString(\"D\")" 
				: $"{sourceCode}.ToString(\"D\")");
		}
	}
}
