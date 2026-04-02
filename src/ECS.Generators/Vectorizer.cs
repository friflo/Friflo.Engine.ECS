// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

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
        var vectorTypes = GetVectorTypes(query);
        if (vectorTypes == null) {
            return false;
        }
        var vectorTypeDimension = GetVectorTypeDimension(query, vectorTypes);
        if (vectorTypeDimension == 0) {
            return false;
        }
        query.vectorTypes = vectorTypes;
        query.vectorDimension = vectorTypeDimension;
        query.laneCount = query.vectorDimension switch {
            // Aiming for loop unroll factor 4 which is typically the Sweet Spot
            1 => 4,
            2 => 4,
            3 => 3,
            4 => 4,
            _ => -1
        };
        foreach (var syntaxReference in query.methodSymbol.DeclaringSyntaxReferences)
        {
            SyntaxNode node = syntaxReference.GetSyntax();
            if (node is MethodDeclarationSyntax methodDeclarationSyntax) {
                var body = methodDeclarationSyntax.Body;
                if (body == null) continue;
                var compute = new StringBuilder();
                foreach (var statement in body.Statements) {
                    if (!EmitCompute(query, compute, statement)) {
                        return false;
                    }
                }
                EmitVectorizedMethod(query, compute, body);
            }
        }
        query.vectorize = true;
        return true;
    }
    
    private static VectorType[]? GetVectorTypes(Query query)
    {
        var result = new List<VectorType>();
        foreach (var parameter in query.parameters)
        {
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
                    query.ReportDiagnosticSymbol(Errors.InvalidComponentType, parameter, type.Name, parameter.Name);
                    return null;
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
                switch (fullTypeName)
                {
                    case "global::System.Numerics.Vector2":
                        specialType = SpecialType.System_Single; 
                        dimension = 2;
                        paramType = ParamType.Vector;
                        break;
                    case "global::System.Numerics.Vector3":
                        specialType = SpecialType.System_Single; 
                        dimension = 3;
                        paramType = ParamType.Vector;
                        break;
                    case "global::System.Numerics.Vector4":
                        specialType = SpecialType.System_Single; 
                        dimension = 4;
                        paramType = ParamType.Vector;
                        break;
                    case "global::System.Numerics.Matrix4x4":
                        specialType = SpecialType.System_Single; 
                        dimension = 4;
                        paramType = ParamType.Matrix4x4;
                        break;
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
    
    private static int GetVectorTypeDimension(Query query, VectorType[] vectorTypes)
    {
        var dimension = 0;
        var success = true;
        IParameterSymbol? currentParameter = null;
        foreach (var vectorType in vectorTypes) {
            if (vectorType.paramType == ParamType.None) {
                success = false;
                query.ReportDiagnosticSymbol(Errors.InvalidParameterType, vectorType.parameter, vectorType.parameter.Type.Name);
            }
            if (!vectorType.isComponent && vectorType.dimension == 1) {
                continue;
            }
            if (dimension == 0) {
                dimension = vectorType.dimension;
                currentParameter = vectorType.parameter;
                continue;
            }
            if (vectorType.dimension > 1 && vectorType.dimension != dimension) {
                query.ReportDiagnosticSymbol(Errors.IncompatibleParameterTypes, null, currentParameter?.Type.Name, vectorType.parameter.Type.Name);
                success = false;
                continue;
            }
        }
        return success ? dimension : 0;
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
    
    private static bool EmitCompute(Query query, StringBuilder compute, StatementSyntax statement)
    {
        var lanes = new StringBuilder[query.laneCount];
        for (int n = 0; n < lanes.Length; n++) {
            lanes[n] = new StringBuilder();
        }
        // Is local declaration - e.g.     var local = value;
        if (statement is LocalDeclarationStatementSyntax localDecl) {
            foreach (var variable in localDecl.Declaration.Variables) {
                var initializerExpression = variable.Initializer?.Value;
                if (initializerExpression != null) {
                    var variableName = variable.Identifier.Text;
                    for (int n = 0; n < lanes.Length; n++) {
                        lanes[n].Append($"{variableName}_{n} = ");
                    }
                    if (!Compute(lanes, query, initializerExpression)) {
                        return false;
                    }
                    for (int n = 0; n < lanes.Length; n++) {
                        lanes[n].Append(";");
                    }
                }
            }
        }
        // Assignment - e.g.     position.value = value;
        if (statement is ExpressionStatementSyntax expressionStatement) {
            var expressionSyntax = expressionStatement.Expression;
            if (!Compute(lanes, query, expressionSyntax)) {
                return false;
            }
        }
        for (int n = 0; n < lanes.Length; n++) {
            compute.AppendLine($"                    {lanes[n]}");
        }
        compute.AppendLine();
        return true;
    }
    
    private static bool EmitVectorizedMethod(Query query, StringBuilder compute, BlockSyntax? body)
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
            switch (vectorType.paramType) {
                case ParamType.Scalar:
                    locals.AppendLine($"            var {parameter.Name}_scalar = Vector256.Create({parameter.Name});");
                    locals.AppendLine();
                    break;
                default:                // TODO  type should be clear here 
                case ParamType.Vector:
                    Utils.InterleaveVector3(locals, parameter.Name, query.vectorDimension);
                    locals.AppendLine();
                    break;
                case ParamType.Matrix4x4:
                    Utils.LoadMatrix(locals, parameter.Name);
                    locals.AppendLine();
                    break;
            }
        }
        // local variables
        if (body != null) {
            foreach (var statement in body.Statements) {
                // Is local declaration - e.g.     var local = value;
                if (statement is LocalDeclarationStatementSyntax localDecl) {
                    foreach (var variable in localDecl.Declaration.Variables) {
                        var variableName = variable.Identifier.Text;
                        for (int n = 0; n < query.laneCount; n++) {
                            locals.AppendLine($"            Vector256<float> {variableName}_{n};");
                        }
                        locals.AppendLine();
                    }
                }
            }
        }
        // const locals
        foreach (var constLocal in query.constLocals) {
            switch (constLocal.paramType) {
                case ParamType.Scalar:
                    locals.AppendLine($"            var {constLocal.name}_scalar = Vector256.Create<float>({constLocal.value});");
                    locals.AppendLine();
                    break;
                default:                // TODO  type should be clear here 
                case ParamType.Vector:
                    locals.AppendLine($"            var {constLocal.name} = {constLocal.value};");
                    Utils.InterleaveVector3(locals, constLocal.name, query.vectorDimension);
                    locals.AppendLine();
                    break;
                case ParamType.Matrix4x4:
                    Utils.LoadMatrix(locals, constLocal.name);
                    locals.AppendLine();
                    break;
            }
        }
        var localBlock = "";
        if (locals.Length > 0) {
            localBlock = $"            // --- Locals\n{locals}";
        }
        
        // --- fixed block
        var @fixed = new StringBuilder();
        foreach (var component in query.components) {
            var type = component.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            @fixed.Append($"            fixed ({type}* {component.Name}_first = {component.Name})");
            @fixed.AppendLine();
        }
        // --- pointer block
        var pointer = new StringBuilder();
        foreach (var component in query.components) {
            pointer.AppendLine();
            pointer.Append($"                    float* {component.Name}_ptr = (float*)({component.Name}_first + i);");
        }
        var elementStep = query.vectorDimension switch {
            1 => 32,
            2 => 16,
            3 => 8,
            4 => 8,
            _ => -1,
        };
        int step = 8;
        var vectorizeBlock = EmitLoopBody(query, compute, body, step);
        if (vectorizeBlock == null) {
            return false;
        }
        var source = $@"
        [SkipLocalsInit]
        private static unsafe int _{query.methodSymbol.Name}_Avx{query.hash}({signature})
        {{
            int i = 0;
            var end = {query.components[0].Name}.Length - {elementStep};
            if (i > end) {{
                return 0;
            }}
{localBlock}{@fixed}            {{
                for (; i <= end; i += {elementStep})
                {{{pointer}
{vectorizeBlock}
                }}
            }}
            return i;
        }}
";
        query.avxMethod = source;
        return true;
    }
    
    private static StringBuilder? EmitLoopBody(Query query, StringBuilder compute, BlockSyntax? body, int step)
    {
        var source = new StringBuilder();
        var laneCount = query.laneCount;
        source.AppendLine();
        source.AppendLine("                    // --- 1. Load");
        foreach (var vectorType in query.vectorTypes) {
            if (!vectorType.isComponent) continue;
            var name = vectorType.parameter.Name;
            if (vectorType.paramType == ParamType.Scalar) {
                switch (query.vectorDimension)
                {
                    case 1:
                        for (int n = 0; n < laneCount; n++) {
                            source.AppendLine($"                    Vector256<float> {name}_{n} = Avx.LoadVector256({name}_ptr + {n*step});");
                        }
                        break;
                    case 2:
                        source.AppendLine(
$"""
                    Vector256<float> {name}_scalar_01 = Avx.LoadVector256({name}_ptr);
                    Vector256<float> {name}_scalar_23 = Avx.LoadVector256({name}_ptr + 8);
                    Vector256<float> {name}_0 = Avx2.PermuteVar8x32({name}_scalar_01, {name}_mask_lo);
                    Vector256<float> {name}_1 = Avx2.PermuteVar8x32({name}_scalar_01, {name}_mask_hi);
                    Vector256<float> {name}_2 = Avx2.PermuteVar8x32({name}_scalar_23, {name}_mask_lo);
                    Vector256<float> {name}_3 = Avx2.PermuteVar8x32({name}_scalar_23, {name}_mask_hi);
""");
                        break;
                    default:
                        source.AppendLine($"                    Vector256<float> {name}_scalar = Avx.LoadVector256({name}_ptr);");
                        for (int n = 0; n < laneCount; n++) {
                            source.AppendLine($"                    Vector256<float> {name}_{n} = Avx2.PermuteVar8x32({name}_scalar, {name}_mask_{n});");
                        }
                        break;
                }
            } else {
                for (int n = 0; n < laneCount; n++) {
                    source.AppendLine($"                    Vector256<float> {name}_{n} = Avx.LoadVector256({name}_ptr + {n*step});");
                }
            }
            source.AppendLine();
        }
        source.AppendLine("                    // --- 2. Compute");
        source.Append(compute);
        if (compute.Length == 0) source.AppendLine();
        
        source.AppendLine("                    // --- 3. Store");
        if (body == null) {
            return source;
        }
        foreach (var statement in body.Statements)
        {
            if (statement is ExpressionStatementSyntax expressionStatement) {
                if (expressionStatement.Expression is not AssignmentExpressionSyntax assignmentExpressionSyntax) {
                    source.AppendLine("                    // found no assignment");
                    continue;
                }
                var left = Utils.GetMemberName(assignmentExpressionSyntax.Left).Identifier.Text;
                for (int n = 0; n < laneCount; n++) {
                    source.AppendLine($"                    Avx.Store({left}_ptr + {n*step}, {left}_{n});");
                }
                source.AppendLine();
            }
        }
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
        if (syntax is InvocationExpressionSyntax invocation) {
            return Compute_Invocation(lanes, query, invocation);
        }
        if (syntax is LiteralExpressionSyntax literal) {
            return Compute_Literal(lanes, query, literal);
        }
        query.ReportDiagnosticSyntax(Errors.OperationUnsupported, syntax, syntax.ToFullString());
        return false;
    }
}