﻿using System.Collections.Generic;
using AsyncGenerator.Core.Plugins;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace AsyncGenerator.Core.Analyzation
{
	/// <summary>
	/// Used by <see cref="IInvocationExpressionAnalyzer"/> in order to customize the analyzation process of a reference
	/// </summary>
	public interface IFunctionReferenceAnalyzation 
	{
		IFunctionAnalyzationResult ReferenceFunctionData { get; }

		SimpleNameSyntax ReferenceNameNode { get; }

		ReferenceLocation ReferenceLocation { get; }

		IMethodSymbol ReferenceSymbol { get; }

		IReadOnlyList<IMethodSymbol> ReferenceAsyncSymbols { get; }
	}
}
