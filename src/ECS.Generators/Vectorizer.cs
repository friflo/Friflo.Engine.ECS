// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Friflo.Engine.ECS.Generators;

public static partial class Vectorizer
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
        query.vectorTypes = GetVectorTypes(query);
        AnalyzeVectorTypes(query.vectorTypes);
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
    
    private static VectorType[] GetVectorTypes(Query query)
    {
        var result = new List<VectorType>();
        foreach (var parameter in query.parameters)
        {
            bool isEntity = query.ecsTypes.IsEntityParameter(parameter); 
            if (isEntity) {
                continue;
            }
            var type = parameter.Type;
            var name = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            bool isComponent = query.ecsTypes.IsComponent(type);
            if (isComponent) {
                IFieldSymbol valueField = null;
                foreach (var field in type.GetMembers().OfType<IFieldSymbol>()) {
                    if (field.Name == "value" || field.Name == "Value") {
                        valueField = field;
                        break;
                    }
                }
                if (valueField == null) {
                    throw new InvalidOperationException($"missing value field in {name}");
                }
                result.Add(CreateVectorType(parameter, name, true, valueField.Type));
            } else { 
                var vectorType = CreateVectorType(parameter, name, false, parameter.Type);
                query.paramTypes.Add(parameter.Name, vectorType.paramType);
                result.Add(vectorType);
            }
        }
        return result.ToArray();
    }
    
    private static VectorType CreateVectorType(IParameterSymbol parameter, string fullQualifiedName, bool isComponent, ITypeSymbol valueType)
    {
        var specialType = valueType.SpecialType;
        int dimension = 0;
        switch (valueType.SpecialType) {
            case SpecialType.None:
                specialType = SpecialType.System_Single; // TODO assumes Vector3. retrieve correct component type 
                dimension = 3;
                break;
            case SpecialType.System_Single:
                dimension = 1;
                break;
        }
        return new VectorType {
            parameter           = parameter,
            fullQualifiedName   = fullQualifiedName,
            isComponent         = isComponent,
            valueType           = valueType,
            paramType           = new ParamType { specialType = specialType, dimension = dimension }     
        };
    }
    
    private static void AnalyzeVectorTypes(VectorType[] vectorTypes)
    {
        foreach (var vectorType in vectorTypes) {
        }
    }
    
    public static string EmitVectorizeBlock(Query query)
    {
        if (!query.vectorize) {
            return "";
        }
        var sb = new StringBuilder();
        foreach (var vectorType in query.vectorTypes) {
            if (sb.Length > 0) {
                sb.Append(", ");
            }
            var parameter = vectorType.parameter;
            if (vectorType.isComponent) {
                sb.Append(parameter.Name);
                sb.Append("Span");
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
        var locals = new StringBuilder();
        // --- method signature
        var signature = new StringBuilder();
        foreach (var vectorType in query.vectorTypes) {
            var parameter = vectorType.parameter;
            var type = parameter.Type;
            if (signature.Length > 0) {
                signature.Append(",");
            }
            if (vectorType.isComponent) {
                if (vectorType.paramType.dimension == 1) {
                    Utils.ScalarMask(locals, parameter.Name);
                }
                signature.Append($"\n            Span<{vectorType.fullQualifiedName}> {parameter.Name}");
                continue;
            }
            Utils.AppendRefKind(signature, parameter.RefKind);
            signature.Append($"\n            {vectorType.fullQualifiedName} {parameter.Name}");
            // 
            switch (type.SpecialType) {
                case SpecialType.None:
                    Utils.InterleaveVector3(locals, parameter.Name);
                    break;
                case  SpecialType.System_Single:
                    locals.AppendLine($"            var {parameter.Name}_scalar = Vector256.Create({parameter.Name});");
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
{locals}            int i = 0;
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
        var lanes = new StringBuilder[3];
        for (int n = 0; n < lanes.Length; n++) {
            lanes[n] = new StringBuilder();
        }
        if (Compute(lanes, query, expressionSyntax)) {
            for (int n = 0; n < lanes.Length; n++) {
                source.AppendLine($"                    {lanes[n]}");
            }
        } else {
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
    
    private static bool Compute(StringBuilder[] computeLanes, Query query, ExpressionSyntax syntax)
    {
        if (syntax is AssignmentExpressionSyntax assignment) {
            return Compute_Assignment(computeLanes, query, assignment);
        }
        if (syntax is BinaryExpressionSyntax binary) {
            return Compute_Binary(computeLanes, query, binary);
        }
        if (syntax is MemberAccessExpressionSyntax memberAccess) {
            return Compute_MemberAccess(computeLanes, query, memberAccess);
        }
        if (syntax is IdentifierNameSyntax identifier) {
            return Compute_IdentifierName(computeLanes, query, identifier);
        }
        return false;
    }
}