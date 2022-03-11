using System;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Uno;
using GeneratedSerializers.Extensions;
using GeneratedSerializers.Helpers;

namespace GeneratedSerializers
{
	public class RoslynMetadataHelper
	{
		private const string AdditionalTypesFileName = "additionalTypes.cs";

		private Project _project;
		private readonly string[] _additionalTypes;
		private readonly Func<string, ITypeSymbol[]> _findTypesByName;
		private readonly Func<string, ITypeSymbol> _findTypeByFullName;

		public Compilation Compilation { get; }

		public RoslynMetadataHelper(Compilation sourceCompilation, Project project, string[] additionalTypes = null)
		{
			_findTypesByName = Funcs.Create<string, ITypeSymbol[]>(SourceFindTypesByName).AsLockedMemoized();
			_findTypeByFullName = Funcs.Create<string, ITypeSymbol>(SourceFindTypeByFullName).AsLockedMemoized();
			_additionalTypes = additionalTypes ?? new string[0];

			Compilation = GenerateAdditionalTypes(sourceCompilation, project, additionalTypes);
		}

		private Compilation GenerateAdditionalTypes(Compilation sourceCompilation, Project project, string[] additionalTypes)
		{
			if (additionalTypes == null || additionalTypes.Length == 0)
			{
				return sourceCompilation; // nothing to add to compilation
			}

			var sb = new StringBuilder();
			sb.AppendLine("class __test__ {");

			int index = 0;
			foreach (var type in _additionalTypes)
			{
				sb.AppendLine($"{type} __{index};");
			}

			sb.AppendLine("}");

			_project = project.AddDocument(AdditionalTypesFileName, sb.ToString()).Project;

			var compilation = _project.GetCompilationAsync().Result;
			return compilation;
		}

		public ITypeSymbol[] FindTypesByName(string name)
		{
			return name.HasValue() ? _findTypesByName(name) : Array.Empty<ITypeSymbol>();
		}

		private INamedTypeSymbol[] SourceFindTypesByName(string name)
		{
			if (!name.HasValue())
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			// This validation ensure that the project has been loaded.
			Compilation.Validation().NotNull("Compilation");

			var results = SymbolFinder
				.FindDeclarationsAsync(_project, name, ignoreCase: false, filter: SymbolFilter.Type)
				.Result;

			return results
				.OfType<INamedTypeSymbol>()
				.Where(r => r.Kind != SymbolKind.ErrorType && r.TypeArguments.None())
				.ToArray();

		}

		public ITypeSymbol FindTypeByName(string name) => FindTypesByName(name).FirstOrDefault();

		public ITypeSymbol FindTypeByFullName(string fullName) => _findTypeByFullName(fullName);

		private ITypeSymbol SourceFindTypeByFullName(string fullName)
		{
			var symbol = Compilation.GetTypeByMetadataName(fullName);

			if (symbol?.Kind == SymbolKind.ErrorType)
			{
				symbol = null;
			}

			if (symbol == null)
			{
				// This type resolution is required because there is no way (yet) to get a type 
				// symbol from a string for types that are not "simple", like generic types or arrays.

				// We then use a temporary documents that contains all the known
				// additional types from the constructor of this class, then search for symbols through it.

				if (fullName.EndsWith("[]", StringComparison.OrdinalIgnoreCase))
				{
					var type = FindTypeByFullName(fullName.Substring(0, fullName.Length - 2));
					if (type != null)
					{
						type = Compilation.CreateArrayTypeSymbol(type);
						return type;
					}
				}
				else if (fullName.StartsWith("System.Nullable`1["))
				{
					const int prefixLength = 18; // System.Nullable'1[
					const int suffixLength = 1; // ]
					var type = FindTypeByFullName(fullName.Substring(prefixLength, fullName.Length - (prefixLength + suffixLength)));
					if (type != null)
					{
						return ((INamedTypeSymbol) FindTypeByFullName("System.Nullable`1")).Construct(type);
					}
				}

				var tree = Compilation.SyntaxTrees.FirstOrDefault(s => s.FilePath == AdditionalTypesFileName);

				var fieldSymbol = tree?.GetRoot()
					.DescendantNodesAndSelf()
					.OfType<FieldDeclarationSyntax>()
					.FirstOrDefault(f => f.Declaration.Type.ToString() == fullName);

				if (fieldSymbol != null)
				{
					var info = Compilation.GetSemanticModel(tree).GetSymbolInfo(fieldSymbol.Declaration.Type);

					if (info.Symbol != null && info.Symbol.Kind != SymbolKind.ErrorType)
					{
						return info.Symbol as ITypeSymbol;
					}

					var declaredSymbol = Compilation.GetSemanticModel(tree).GetDeclaredSymbol(fieldSymbol.Declaration.Type);

					if (declaredSymbol != null && declaredSymbol.Kind != SymbolKind.ErrorType)
					{
						return declaredSymbol as ITypeSymbol;
					}
				}
			}

			if (symbol?.Kind == SymbolKind.ErrorType)
			{
				return null;
			}

			return symbol;
		}

		public ITypeSymbol GetTypeByFullName(string fullName)
		{
			var symbol = FindTypeByFullName(fullName);

			if (symbol == null)
			{
				throw new InvalidOperationException($"Unable to find type {fullName}");
			}

			return symbol;
		}

		public INamedTypeSymbol GetSpecial(SpecialType specialType) => Compilation.GetSpecialType(specialType);

		public INamedTypeSymbol GetGenericType(string name = "T") =>  Compilation.CreateErrorTypeSymbol(null, name, 0);

		public IArrayTypeSymbol GetArray(ITypeSymbol type) => Compilation.CreateArrayTypeSymbol(type);

		public ITypeSymbol ConstructFromUnbounded(ITypeSymbol unbounded, ITypeSymbol itemType)
		{
			if (unbounded is IArrayTypeSymbol)
			{
				return GetArray(itemType);
			}

			if (!(unbounded is INamedTypeSymbol named))
			{
				throw new InvalidOperationException(
					$"{unbounded.GetDeclarationGenericFullName()} is not supported by static serializers.");
			}

			switch (named.Arity)
			{
				case 0:
					return named;
				case 1:
					return named.Construct(itemType);
				case 2:
					return named.Construct(GetSpecial(SpecialType.System_String), itemType);
				default:
					throw new InvalidOperationException("Static serializer supports only arity 1 or 2 (IEnumerable<T> or IDictionary<string, T>).");
			}
		}
	}
}
