using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Friflo.Engine.ECS.Generators;

public static class Vectorizer
{
    public static string Emit(Query query)
    {
        var search = query.ecsTypes.vectorizeAttribute.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        bool found = false;
        foreach (var attributeData in query.attributes) {
            // if (SymbolEqualityComparer.Default.Equals(types.vectorizeAttribute, attributeData.AttributeClass)) found = true;
            if (attributeData.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == search) found = true;
        }
        if (!found) {
            return null;
        }
        query.vectorize = true;
        foreach (var syntaxReference in query.methodSymbol.DeclaringSyntaxReferences) {
            SyntaxNode node = syntaxReference.GetSyntax();
            if (node is MethodDeclarationSyntax methodDeclarationSyntax) {
                var body = methodDeclarationSyntax.Body;
                if (body == null) continue;
                foreach (var statement in body.Statements) {
                    if (statement is ExpressionStatementSyntax expressionStatement) {
                        TraverseStatement(query, expressionStatement);
                    }
                }
            }
        }
        return null;
    }
    
    public static string EmitVectorizeBlock(Query query)
    {
        if (!query.vectorize) {
            return "";
        }
        var sb = new StringBuilder();
        foreach (var component in query.components) {
            if (sb.Length > 0) {
                sb.Append(", ");
            }
            sb.Append(component.Name);
            sb.Append("Span");
        }
        var source = $@"
                if (!vectorized) goto EntityLoop;
                if (Avx.IsSupported) {{
                    n = _{query.methodSymbol.Name}_Avx{query.hash}({sb});
                }}
            EntityLoop:";
        return source;
    }
    
    private static void TraverseStatement(Query query, ExpressionStatementSyntax syntax)
    {
        if (syntax.Expression is AssignmentExpressionSyntax assignmentExpressionSyntax) {
            var left  = assignmentExpressionSyntax.Left;
            var right = assignmentExpressionSyntax.Right;
        }

        var signature = new StringBuilder();
        foreach (var component in query.components) {
            if (signature.Length > 0) {
                signature.Append(", ");
            }
            var type = component.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            signature.Append($"Span<{type}> {component.Name}");
        }
        var @fixed = new StringBuilder();
        foreach (var component in query.components) {
            var type = component.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            @fixed.AppendLine();
            @fixed.Append($"            fixed ({type}* {component.Name}_ptr = {component.Name})");
        }
        var pointer = new StringBuilder();
        foreach (var component in query.components) {
            pointer.AppendLine();
            pointer.Append($"                    float* {component.Name}_ptr_scalar = (float*)({component.Name}_ptr + i);");
        }
        var vectorizeBlock = VectorizeBlock(query);
        var source = $@"
        private static unsafe int _{query.methodSymbol.Name}_Avx{query.hash}({signature})
        {{
            int i = 0;
            var end = {query.components[0].Name}.Length - 8;{@fixed}
            {{
                for (; i <= end; i += 8)
                {{{pointer}
{vectorizeBlock}
                }}
            }}
            return i;
        }}
";
        query.avxMethod = source;
    }
    
    private static StringBuilder VectorizeBlock(Query query)
    {
        var source = new StringBuilder();
        var components = query.components;
        source.AppendLine();
        source.AppendLine("                    // 1. Load");
        foreach (var component in components) {
            for (int n = 0; n < 3; n++) {
                source.AppendLine($"                    Vector256<float> {component.Name}_{n} = Avx.LoadVector256({component.Name}_ptr_scalar);");
            }
            source.AppendLine();
        }
        source.AppendLine("                    // 2. Compute");
        source.AppendLine("                    // ...");
        source.AppendLine();
        
        source.AppendLine("                    // 3. Store");
        foreach (var component in components) {
            for (int n = 0; n < 3; n++) {
                source.AppendLine($"                    Avx.Store({component.Name}_ptr_scalar, {component.Name}_{n});");
            }
            source.AppendLine();
        }
        return source;
    }

}