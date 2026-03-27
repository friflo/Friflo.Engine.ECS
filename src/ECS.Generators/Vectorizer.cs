using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Friflo.Engine.ECS.Generators;

public static class Vectorizer
{
    public static string Emit(IMethodSymbol methodSymbol, ImmutableArray<AttributeData> attributes, Types types)
    {
        var search = types.vectorizeAttribute.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        bool found = false;
        foreach (var attributeData in attributes) {
            // if (SymbolEqualityComparer.Default.Equals(types.vectorizeAttribute, attributeData.AttributeClass)) found = true;
            if (attributeData.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == search) found = true;
        }
        if (!found) {
            return null;
        }
        foreach (var syntaxReference in methodSymbol.DeclaringSyntaxReferences) {
            SyntaxNode node = syntaxReference.GetSyntax();
            if (node is MethodDeclarationSyntax methodDeclarationSyntax) {
                var body = methodDeclarationSyntax.Body;
                if (body == null) continue;
                foreach (var statement in body.Statements) {
                    if (statement is ExpressionStatementSyntax expressionStatement) {
                        Traverse(expressionStatement);
                    }
                }
            }
        }
        return null;
    }
    
    private static void Traverse(ExpressionStatementSyntax syntax)
    {
        if (syntax.Expression is AssignmentExpressionSyntax assignmentExpressionSyntax) {
            var left  = assignmentExpressionSyntax.Left;
            var right = assignmentExpressionSyntax.Right;
        }
    }
}