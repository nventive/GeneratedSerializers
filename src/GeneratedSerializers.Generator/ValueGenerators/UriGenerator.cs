using System;

namespace GeneratedSerializers
{
	public class UriGenerator : TypeGeneratorBase<Uri>
	{
		public override string Read(string target, bool isNullable, IValueSerializationGeneratorContext context)
		{
			var uri = VariableHelper.GetName("uri");
			return $@"
				string {uri};
				{context.Read<string>(uri)}
				if (!string.IsNullOrEmpty({uri}))
				{{
					{target} = new Uri({uri}, UriKind.RelativeOrAbsolute);
				}}";
		}

		public override string Write(string sourceName, string sourceCode, bool isNullable, IValueSerializationGeneratorContext context)
		{
			return context.Write<string>(sourceName, $"{sourceCode}?.{nameof(Uri.OriginalString)}");
		}
	}
}
