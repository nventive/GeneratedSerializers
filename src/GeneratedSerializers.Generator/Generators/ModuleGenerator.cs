using System;

namespace GeneratedSerializers
{
	public class ModuleGenerator
	{
		private readonly string _namespace;
		private readonly SourceFileMetadata _generatedCodeMeta;

		public ModuleGenerator(string @namespace, SourceFileMetadata generatedCodeMeta)
		{
			_namespace = @namespace;
			_generatedCodeMeta = generatedCodeMeta;
		}

		public string Generate(string serializerClassName, string moduleClassName)
		{
			return $@"
				{_generatedCodeMeta.FileHeader}

				using System;
				using GeneratedSerializers;

				namespace {_namespace}
				{{
					{_generatedCodeMeta.ClassAttributes}
					public partial class {moduleClassName}
					{{
						static partial void InitSerializer(Action<Func<ISerializer, ISerializer>> serializerRegister)
						{{
							serializerRegister(innerSerializer => new {serializerClassName}(innerSerializer, innerSerializer as IObjectSerializer));
						}}

						private static IObjectSerializer GetSerializer(Func<IObjectSerializer> fallbackSerializer = null)
							=> new {serializerClassName}(fallbackObjectSerializer: fallbackSerializer?.Invoke());
					}}
				}}";
		}
	}
}
