// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        foreach (var parameter in query.parameters) {
            if (sb.Length > 0) {
                sb.Append(", ");
            }
            bool isComponent = query.ecsTypes.IsComponent(parameter.Type);
            if (isComponent) {
                sb.Append(parameter.Name);
                sb.Append("Span");
                continue;
            }
            bool isEntity = query.ecsTypes.IsEntityParameter(parameter); 
            if (isEntity) {
                continue;
            }
            Utils.AppendRefKind(sb, parameter.RefKind);
            sb.Append(parameter.Name);
        }
        var source = $@"
                if (!vectorized) goto EntityLoop;
                if (Avx.IsSupported) {{
                    n = _{query.methodSymbol.Name}_Avx{query.hash}({sb});
                }}
            EntityLoop:";
        return source;
    }
    
    private static void TraverseStatement(Query query, ExpressionStatementSyntax expressionSyntax)
    {
        if (expressionSyntax.Expression is AssignmentExpressionSyntax assignmentExpressionSyntax) {
            var left  = assignmentExpressionSyntax.Left;
            var right = assignmentExpressionSyntax.Right;
        }
        var passedParams = new StringBuilder();
        // --- method signature
        var signature = new StringBuilder();
        foreach (var parameter in query.parameters) {
            if (signature.Length > 0) {
                signature.Append(",");
            }
            bool isComponent = query.ecsTypes.IsComponent(parameter.Type);
            if (isComponent) {
                var componentType = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                signature.Append($"\n            Span<{componentType}> {parameter.Name}");
                continue;
            }
            bool isEntity = query.ecsTypes.IsEntityParameter(parameter); 
            if (isEntity) {
                continue;
            }
            Utils.AppendRefKind(signature, parameter.RefKind);
            string type = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            signature.Append($"\n            {type} {parameter.Name}");
            // 
            switch (parameter.Type.SpecialType) {
                case  SpecialType.System_Single:
                    passedParams.AppendLine($"            var {parameter.Name}_scalar = Vector256.Create({parameter.Name});");
                    break;
            }
        }
        
        // --- fixed block
        var @fixed = new StringBuilder();
        foreach (var component in query.components) {
            var type = component.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            @fixed.AppendLine();
            @fixed.Append($"            fixed ({type}* {component.Name}_first = {component.Name})");
        }
        // --- pointer block
        var pointer = new StringBuilder();
        foreach (var component in query.components) {
            pointer.AppendLine();
            pointer.Append($"                    float* {component.Name}_ptr = (float*)({component.Name}_first + i);");
        }
        int step = 8;
        var vectorizeBlock = VectorizeBlock(query, expressionSyntax.Expression, step);
        var source = $@"
        private static unsafe int _{query.methodSymbol.Name}_Avx{query.hash}({signature})
        {{
{passedParams}            int i = 0;
            var end = {query.components[0].Name}.Length - {step};{@fixed}
            {{
                for (; i <= end; i += {step})
                {{{pointer}
{vectorizeBlock}
                }}
            }}
            return i;
        }}
";
        query.avxMethod = source;
    }
    
    private static StringBuilder VectorizeBlock(Query query, ExpressionSyntax expressionSyntax, int step)
    {
        var source = new StringBuilder();
        var components = query.components;
        source.AppendLine();
        source.AppendLine("                    // 1. Load");
        foreach (var component in components) {
            for (int n = 0; n < 3; n++) {
                source.AppendLine($"                    Vector256<float> {component.Name}_{n} = Avx.LoadVector256({component.Name}_ptr + {n*step});");
            }
            source.AppendLine();
        }
        source.AppendLine("                    // 2. Compute");
        if (!Compute(source, query, expressionSyntax, step)) {
            source.AppendLine("                    // not supported");
        }
        source.AppendLine();
        
        source.AppendLine("                    // 3. Store");
        if (expressionSyntax is not AssignmentExpressionSyntax assignmentExpressionSyntax) {
            source.AppendLine("                    // found no assignment");
            return source;
        }
        var left = Utils.GetMemberName(assignmentExpressionSyntax.Left)?.Identifier.Text;
        for (int n = 0; n < 3; n++) {
            source.AppendLine($"                    Avx.Store({left}_ptr + {n*step}, {left}_{n});");
        }
        source.AppendLine();
        return source;
    }
    
    private static bool Compute(StringBuilder source, Query query, ExpressionSyntax expressionSyntax, int step)
    {
        if (expressionSyntax is not AssignmentExpressionSyntax assignmentExpressionSyntax) {
            return false;
        }
        var avxOperation = expressionSyntax.Kind() switch
        {
            SyntaxKind.AddAssignmentExpression      => "Add",
            SyntaxKind.SubtractAssignmentExpression => "Subtract",
            SyntaxKind.MultiplyAssignmentExpression => "Multiply",
            SyntaxKind.DivideAssignmentExpression   => "Divide",
            _                                       => null
        };
        if (avxOperation is null) {
            return false;
        }
        var left    = Utils.GetMemberName(assignmentExpressionSyntax.Left)?.Identifier.Text;
        var right   = Utils.GetMemberName(assignmentExpressionSyntax.Right)?.Identifier.Text;
        if (left is null || right is null) {
            return false;
        }
        for (int i = 0; i < 3; i++) {
            source.AppendLine($"                    {left}_{i} = Avx.{avxOperation}({left}_{i}, {right}_{i});");
        }
        return true;
    }

}