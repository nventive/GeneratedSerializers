using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace GeneratedSerializers
{
	/// <summary>
	/// Generates IStaticSerializer classes for value types (in order to make them resolvable from the ISerializerProvider)
	/// </summary>
	public class ValueTypeSerializersGenerator : IStaticSerializerResolver
	{
		private readonly string _nameSpace;
		private readonly SourceFileMetadata _generatedCodeMeta;

		public static readonly string[] Types = new[]
		{
			"string",

			"long",
			"int",
			"short",
			"ulong",
			"uint",
			"ushort",
			"byte",
			"double",
			"float",
			"decimal",
			"bool",
			"System.DateTime",
			"System.DateTimeOffset",
			"System.TimeSpan",

			"long?",
			"int?",
			"short?",
			"ulong?",
			"uint?",
			"ushort?",
			"byte?",
			"double?",
			"float?",
			"decimal?",
			"bool?",
			"System.DateTime?",
			"System.DateTimeOffset?",
			"System.TimeSpan?",
		};

		private readonly GeneratorContext _context;
		private readonly ITypeSymbol[] _typeSymbols;

		public ValueTypeSerializersGenerator(string nameSpace, RoslynMetadataHelper roselyn, bool useTryParseOrDefault, SourceFileMetadata generatedCodeMeta)
		{
			_nameSpace = nameSpace;
			_generatedCodeMeta = generatedCodeMeta;
			_context = new GeneratorContext(
				roselyn, 
				new ReadContext("reader", "firstChar", "overChar"),
				new WriteContext("writer"),
				new DateTimeGenerator(useTryParseOrDefault),
				new JsonReaderGenerator(useTryParseOrDefault),
				new JsonWriterGenerator());

			_typeSymbols = Types
				.Select(roselyn.GetTypeByFullName)
				.ToArray();
		}

		public string Generate()
		{
			var sb = new StringBuilder($@"
				{_generatedCodeMeta.FileHeader}

				using System;
				using System.Collections.Generic;
				using System.Linq;
				using System.Text;
				using System.Threading.Tasks;
				using GeneratedSerializers;

				namespace {_nameSpace}
				{{");

			foreach (var type in _typeSymbols)
			{
				var className = GetClassName(type);
				sb.AppendLine($@"
					{_generatedCodeMeta.ClassAttributes}
					internal sealed class {className}: IStaticSerializer<{type.ToDisplayString()}>
					{{
						private static IStaticSerializer<{type.ToDisplayString()}> _instance;
						public static IStaticSerializer<{type.ToDisplayString()}> Instance => _instance ?? (_instance = new {className}());

						private {className}() {{ }}

						object IStaticSerializer.Deserialize(JsonReader reader, char firstChar, out char? overChar) => Deserialize(reader, firstChar, out overChar);
						public {type.ToDisplayString()} Deserialize(JsonReader reader, char firstChar, out char? overChar)
						{{
							var value = default({type});
							{_context.GetRead("value", type)}
							return value;
						}}

						void IStaticSerializer.Serialize(JsonWriter writer, object value) => Serialize(writer, ({type.ToDisplayString()})value);

						public void Serialize(JsonWriter writer, {type.ToDisplayString()} value)
						{{
							{_context.GetWrite(null, "value", type)}
						}}
					}}");
			}

			sb.AppendLine("}");

			return sb.ToString();
		}

		public bool IsResolvable(ITypeSymbol type) => _typeSymbols.Contains(type);

		public string GetResolve(ITypeSymbol type) => $"{_nameSpace}.{GetClassName(type)}.Instance";

		private string GetClassName(ITypeSymbol type) => $"{type.GetSerializedGenericFullName()}_StaticSerializer";
	}
}
