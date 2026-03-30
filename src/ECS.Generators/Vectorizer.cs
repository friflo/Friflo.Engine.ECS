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
    public static bool Emit(Query query)
    {
        var search = query.ecsTypes.vectorizeAttribute.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        bool found = false;
        foreach (var attributeData in query.attributes) {
            // if (SymbolEqualityComparer.Default.Equals(types.vectorizeAttribute, attributeData.AttributeClass)) found = true;
            if (attributeData.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == search) found = true;
        }
        if (!found) {
            return false;
        }
        query.vectorTypes = GetVectorTypes(query);
        query.vectorDimension = GetVectorTypeDimension(query.vectorTypes);
        query.laneCount = query.vectorDimension switch {
            2 => 2,
            3 => 3,
            4 => 4,
            _ => -1
        };
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
        return true;
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
                IFieldSymbol? valueField = null;
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
        var paramType   = ParamType.None;
        int dimension = 0;
        switch (valueType.SpecialType) {
            case SpecialType.None:
                var fullTypeName = valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (fullTypeName == "global::System.Numerics.Vector2") {
                    specialType = SpecialType.System_Single; 
                    dimension = 2;
                    paramType = ParamType.Vector;
                }
                else if (fullTypeName == "global::System.Numerics.Vector3") {
                    specialType = SpecialType.System_Single; 
                    dimension = 3;
                    paramType = ParamType.Vector;
                }
                else if (fullTypeName == "global::System.Numerics.Vector4") {
                    specialType = SpecialType.System_Single; 
                    dimension = 4;
                    paramType = ParamType.Vector;
                }
                break;
            case SpecialType.System_Single:
                dimension = 1;
                paramType = ParamType.Scalar;
                break;
        }
        return new VectorType {
            parameter           = parameter,
            fullQualifiedName   = fullQualifiedName,
            isComponent         = isComponent,
            valueType           = valueType,
            valueSpecialType    = specialType, 
            paramType           = paramType,
            dimension           = dimension, 
        };
    }
    
    private static int GetVectorTypeDimension(VectorType[] vectorTypes)
    {
        var dimension = 0;
        foreach (var vectorType in vectorTypes) {
            if (vectorType.dimension == 1) {
                continue;
            }
            if (dimension == 0) {
                dimension = vectorType.dimension;
                continue;
            }
            if (dimension != vectorType.dimension) {
                throw new InvalidOperationException($"Inconsistent parameter dimensions {vectorType.dimension} is not equal to dimension {dimension}");
            }
        }
        return dimension;
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
        var locals = new StringBuilder();
        // --- method signature
        var signature = new StringBuilder();
        foreach (var vectorType in query.vectorTypes) {
            var parameter = vectorType.parameter;
            if (signature.Length > 0) {
                signature.Append(",");
            }
            if (vectorType.isComponent) {
                if (vectorType.paramType == ParamType.Scalar) {
                    Utils.ScalarMask(locals, parameter.Name, query.vectorDimension);
                }
                signature.Append($"\n            Span<{vectorType.fullQualifiedName}> {parameter.Name}");
                continue;
            }
            Utils.AppendRefKind(signature, parameter.RefKind);
            signature.Append($"\n            {vectorType.fullQualifiedName} {parameter.Name}");
            // 
            if (vectorType.paramType == ParamType.Scalar) {
                locals.AppendLine($"            var {parameter.Name}_scalar = Vector256.Create({parameter.Name});");
            } else {
                Utils.InterleaveVector3(locals, parameter.Name, query.vectorDimension);
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
        var elementStep = query.vectorDimension switch {
            2 => 8,  // TODO  Should be 16 for Loop-Unroll: 4 - execution may speedup by 30% 
            3 => 8,
            4 => 8,
            _ => -1,
        };
        int step = 8;
        var vectorizeBlock = VectorizeBlock(query, expressionSyntax.Expression, step);
        var source = $@"
        [SkipLocalsInit]
        private static unsafe int _{query.methodSymbol.Name}_Avx{query.hash}({signature})
        {{
            int i = 0;
            var end = {query.components[0].Name}.Length - {elementStep};
            if (i > end) {{
                return 0;
            }}
{locals}{@fixed}
            {{
                for (; i <= end; i += {elementStep})
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
        var laneCount = query.laneCount;
        source.AppendLine();
        source.AppendLine("                    // 1. Load");
        foreach (var vectorType in query.vectorTypes) {
            if (!vectorType.isComponent) continue;
            var name = vectorType.parameter.Name;
            if (vectorType.paramType == ParamType.Scalar) {
                source.AppendLine($"                    Vector256<float> {name}_scalar = Avx.LoadVector256({name}_ptr);");
                for (int n = 0; n < laneCount; n++) {
                    source.AppendLine($"                    Vector256<float> {name}_{n} = Avx2.PermuteVar8x32({name}_scalar, {name}_mask_{n});");
                }
            } else {
                for (int n = 0; n < laneCount; n++) {
                    source.AppendLine($"                    Vector256<float> {name}_{n} = Avx.LoadVector256({name}_ptr + {n*step});");
                }
            }
            source.AppendLine();
        }
        source.AppendLine("                    // 2. Compute");
        var lanes = new StringBuilder[laneCount];
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
        var left = Utils.GetMemberName(assignmentExpressionSyntax.Left).Identifier.Text;
        for (int n = 0; n < laneCount; n++) {
            source.AppendLine($"                    Avx.Store({left}_ptr + {n*step}, {left}_{n});");
        }
        source.AppendLine();
        return source;
    }
    
    private static bool Compute(StringBuilder[] lanes, Query query, ExpressionSyntax syntax)
    {
        if (syntax is AssignmentExpressionSyntax assignment) {
            return Compute_Assignment(lanes, query, assignment);
        }
        if (syntax is BinaryExpressionSyntax binary) {
            return Compute_Binary(lanes, query, binary);
        }
        if (syntax is MemberAccessExpressionSyntax memberAccess) {
            return Compute_MemberAccess(lanes, query, memberAccess);
        }
        if (syntax is IdentifierNameSyntax identifier) {
            return Compute_IdentifierName(lanes, query, identifier);
        }
        return false;
    }
}