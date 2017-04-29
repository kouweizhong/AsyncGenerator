﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncGenerator.Analyzation;
using AsyncGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AsyncGenerator.Transformation.Internal
{
	partial class ProjectTransformer
	{
		private DocumentTransformationResult TransformDocument(IDocumentAnalyzationResult documentResult)
		{
			var rootNode = documentResult.Node;
			var endOfLineTrivia = rootNode.DescendantTrivia().First(o => o.IsKind(SyntaxKind.EndOfLineTrivia));
			var result = new DocumentTransformationResult(documentResult);
			// Annotate so that the annotation exposed is valid
			rootNode = rootNode.WithAdditionalAnnotations(new SyntaxAnnotation(result.Annotation));

			foreach (var typeResult in documentResult.GlobalNamespace.Types.Where(o => o.Conversion != TypeConversion.Ignore))
			{
				var typeSpanStart = typeResult.Node.SpanStart;
				var typeSpanLength = typeResult.Node.Span.Length;
				var typeNode = rootNode.DescendantNodesAndSelf()
					.OfType<TypeDeclarationSyntax>()
					.First(o => o.SpanStart == typeSpanStart && o.Span.Length == typeSpanLength);
				var transformResult = TransformType(typeResult);
				result.TransformedTypes.Add(transformResult);
				rootNode = rootNode.ReplaceNode(typeNode, typeNode.WithAdditionalAnnotations(new SyntaxAnnotation(transformResult.Annotation)));
			}

			foreach (var namespaceResult in documentResult.Namespaces.OrderBy(o => o.Node.SpanStart))
			{
				var namespaceSpanStart = namespaceResult.Node.SpanStart;
				var namespaceSpanLength = namespaceResult.Node.Span.Length;
				var namespaceNode = rootNode.DescendantNodesAndSelf()
					.OfType<NamespaceDeclarationSyntax>()
					.First(o => o.SpanStart == namespaceSpanStart && o.Span.Length == namespaceSpanLength);
				var transformResult = TransformNamespace(namespaceResult);
				result.TransformedNamespaces.Add(transformResult);
				rootNode = rootNode.ReplaceNode(namespaceNode, namespaceNode.WithAdditionalAnnotations(new SyntaxAnnotation(transformResult.Annotation)));
			}

			// Save the orignal node that was only annotated
			var originalAnnotatedNode = rootNode;

			var transformResults = result.TransformedNamespaces
				.Cast<TransformationResult>()
				.Union(result.TransformedTypes)
				.ToList();

			var newMembers = transformResults
				.OrderBy(o => o.OriginalStartSpan)
				.SelectMany(o => o.GetTransformedNodes())
				.ToList();

			if (!newMembers.Any())
			{
				return result; // the document will not be created
			}
			rootNode = rootNode
				.WithMembers(List(newMembers));

			// Update the original document if required
			foreach (var rewrittenNode in transformResults.Where(o => o.OriginalModifiedNode != null).OrderByDescending(o => o.OriginalStartSpan))
			{
				if (result.OriginalModifiedNode == null)
				{
					result.OriginalModifiedNode = originalAnnotatedNode;
				}
				result.OriginalModifiedNode = result.OriginalModifiedNode
					.ReplaceNode(result.OriginalModifiedNode
						.GetAnnotatedNodes(rewrittenNode.Annotation).First(), rewrittenNode.OriginalModifiedNode);
			}

			// Add auto-generated comment
			var token = rootNode.DescendantTokens().First();
			rootNode = rootNode.ReplaceToken(token, token.AddAutoGeneratedTrivia(endOfLineTrivia));

			result.TransformedNode = rootNode;


			var regionRewriter = new DirectiveTransformer();
			regionRewriter.Transform(result);

			return result;
		}
	}
}
