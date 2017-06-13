﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AsyncGenerator.Core.Analyzation
{
	public interface ITypeAnalyzationResult : IMemberAnalyzationResult
	{
		INamedTypeSymbol Symbol { get; }

		TypeDeclarationSyntax Node { get; }

		TypeConversion Conversion { get; }

		bool IsPartial { get; }

		/// <summary>
		/// References of types that are used inside this type
		/// </summary>
		IReadOnlyList<ITypeReferenceAnalyzationResult> TypeReferences { get; }

		/// <summary>
		/// References of itself
		/// </summary>
		IReadOnlyList<ITypeReferenceAnalyzationResult> SelfReferences { get; }

		IReadOnlyList<ITypeAnalyzationResult> NestedTypes { get; }

		IReadOnlyList<IMethodAnalyzationResult> Methods { get; }
	}
}