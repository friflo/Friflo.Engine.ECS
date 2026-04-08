// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Friflo.Vectorization.Generators;

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

        // 1. Phase: generate AoS
        if (!TraverseBody(query)) {
            return false;
        }
        if (query.requireSoA) {
            // 2. Phase: generate SoA
            if (!TraverseBodySoA(query)) {
                return false;
            }
        }
        query.vectorize = true;
        return true;
    }
    
    private static bool TraverseBodySoA(Query query)
    {
        // Reset query state created by previous traversal. Generated code require Deinterleave() / Interleave()
        query.useSoA = true;
        query.avxMethod = "";
        query.lanes = null;
        query.paramTypes.Clear();
        query.locals.Clear();
        query.computeTemp.Clear();
        query.computeTempCount = 0;
        query.constLocalsCount = 0;
        
        // 2. Phase: generate SoA
        return TraverseBody(query);
    }
    
    private static bool TraverseBody(Query query)
    {
        foreach (var type in query.vectorTypes) {
            query.AddParam(type.parameter.Name, type.isComponent, type.isScalar, true, type.dimension);
        }
        foreach (var syntaxReference in query.methodSymbol.DeclaringSyntaxReferences)
        {
            SyntaxNode node = syntaxReference.GetSyntax();
            if (node is MethodDeclarationSyntax methodDeclarationSyntax) {
                var body = methodDeclarationSyntax.Body;
                if (body == null) continue;
                var compute = new StringBuilder();
                foreach (var statement in body.Statements) {
                    if (!EmitCompute(query, null!, compute, statement)) {
                        return false;
                    }
                    compute.Append(query.computeTemp);
                    query.computeTemp.Clear();
                    var statementText = Regex.Replace(statement.ToString(), @"\s+", " ").Trim();;
                    compute.AppendLine($"                    // {statementText}");
                    var lanes = query.lanes;
                    for (int n = 0; n < lanes.Length; n++) {
                        compute.AppendLine($"                    {lanes[n]}");
                    }
                    compute.AppendLine();
                }
                EmitVectorizedMethod(query, compute, body);
            }
        }
        return true;
    }
    
    private static VectorType[]? GetVectorTypes(Query query)
    {
        var result = new List<VectorType>();
        foreach (var parameter in query.parameters)
        {
            var type = parameter.Type;
            var typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
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
                var vectorType = CreateVectorType(parameter, typeName, true, valueField.Type);
                result.Add(vectorType);
            } else { 
                var vectorType = CreateVectorType(parameter, typeName, false, parameter.Type);
                result.Add(vectorType);
            }
        }
        return result.ToArray();
    }
    
    private static (SpecialType specialType, int dimension, ParamType paramType)
        GetTypeDim(ITypeSymbol? valueType)
    {
        if (valueType == null) {
            return (SpecialType.None, 0,  ParamType.None);
        }
        var specialType = valueType.SpecialType;
        switch (specialType) {
            case SpecialType.None:
                var fullTypeName = valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                switch (fullTypeName)
                {
                    case "global::System.Numerics.Vector2":     return (SpecialType.System_Single, 2,  ParamType.Vector);
                    case "global::System.Numerics.Vector3":     return (SpecialType.System_Single, 3,  ParamType.Vector);
                    case "global::System.Numerics.Vector4":     return (SpecialType.System_Single, 4,  ParamType.Vector);
                    case "global::System.Numerics.Matrix4x4":   return (SpecialType.System_Single, 4,  ParamType.Matrix4x4);
                }
                break;
            case SpecialType.System_Single:
                return (SpecialType.None, 1,  ParamType.Scalar);
        }
        return (specialType, 0,  ParamType.None);
    }
    
    private static VectorType CreateVectorType(IParameterSymbol parameter, string fullQualifiedName, bool isComponent, ITypeSymbol valueType)
    {
        bool isScalar   = !isComponent;
        var (specialType, dimension, paramType) = GetTypeDim(valueType);
        if (dimension == 3) {
            isScalar    = false;
        }
        return new VectorType {
            parameter           = parameter,
            fullQualifiedName   = fullQualifiedName,
            isComponent         = isComponent,
            isScalar            = isScalar,  
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
    
    private static bool EmitCompute(Query query, StringBuilder[] lanes, StringBuilder compute, StatementSyntax statement)
    {
        // Is local declaration - e.g.     var local = value;
        if (statement is LocalDeclarationStatementSyntax localDecl) {
            foreach (var variable in localDecl.Declaration.Variables) {
                var initializerExpression = variable.Initializer?.Value;
                if (initializerExpression != null) {
                    var variableName = variable.Identifier.Text;
                    var symbol = query.semanticModel.GetDeclaredSymbol(variable);
                    lanes = CreateLanes(query, symbol, variableName);
                    for (int n = 0; n < lanes.Length; n++) {
                        lanes[n].Append($"{variableName}_{n} = ");
                    }
                    if (!Compute(lanes, query, initializerExpression)) {
                        return false;
                    }
                    lanes.Append(";");
                }
            }
            return true;
        }
        // Assignment - e.g.     position.value = value;
        if (statement is ExpressionStatementSyntax expressionStatement) {
            var expressionSyntax = expressionStatement.Expression;
            return Compute(lanes, query, expressionSyntax);
        }
        query.ReportDiagnosticSyntax(Errors.StatementUnsupported, statement, statement.ToFullString());
        return false;
    }
    
    private static void EmitVectorizedMethod(Query query, StringBuilder compute, BlockSyntax? body)
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
            signature.Append("\n            ");
            Utils.AppendRefKind(signature, parameter.RefKind);
            signature.Append($"{vectorType.fullQualifiedName} {parameter.Name}");
            //
            switch (vectorType.paramType) {
                case ParamType.Scalar:
                    locals.AppendLine($"            var {parameter.Name}_scalar = Vector256.Create({parameter.Name});");
                    locals.AppendLine();
                    break;
                default:                // TODO  type should be clear here 
                case ParamType.Vector:
                    Utils.InterleaveVector3(locals, parameter.Name, query);
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
        locals.Append(query.locals);

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

        Utils.TrimEnd(vectorizeBlock);

        var source = $@"
        [SkipLocalsInit]
        private static unsafe int _{query.methodSymbol.Name}_Avx{query.hash}({signature})
        {{
            int i = 0;
            var end = {query.components[0].Name}.Length - {elementStep};
            if (i > end) {{
                return 0;
            }}
            // Vector layout: {(query.useSoA ? "SoA" : "AoS")}
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
    }
    
    private static StringBuilder EmitLoopBody(Query query, StringBuilder compute, BlockSyntax? body, int step)
    {
        var source = new StringBuilder();
        source.AppendLine();
        source.AppendLine("                    // --- 1. Load");
        foreach (var vectorType in query.vectorTypes) {
            EmitLoadComponentVector(source, query, vectorType, step);
        }
        source.AppendLine("                    // --- 2. Compute");
        source.Append(compute);
        if (compute.Length == 0) source.AppendLine();
        
        source.AppendLine("                    // --- 3. Store");
        if (body == null) {
            return source;
        }
        var assignmentVariables = new List<string>();
        foreach (var statement in body.Statements) {
            if (statement is ExpressionStatementSyntax expressionStatement) {
                if (expressionStatement.Expression is not AssignmentExpressionSyntax assignmentExpressionSyntax) {
                    // source.AppendLine("                    // found no assignment");
                    continue;
                }
                var name = Utils.GetMemberName(assignmentExpressionSyntax.Left).Identifier.Text;
                assignmentVariables.Add(name);
            }
        }
        foreach (var vectorType in query.vectorTypes) {
            if (!assignmentVariables.Contains(vectorType.parameter.Name)) {
                continue;
            }
            EmitStoreComponentVector(source, query, vectorType, step);
        }
        return source;
    }
    
    private static void EmitLoadComponentVector(StringBuilder source, Query query, VectorType vectorType, int step)
    {
        if (!vectorType.isComponent) {
            return;
        }
        var laneCount = query.laneCount;
        var name = vectorType.parameter.Name;
        var typeName = vectorType.parameter.Type.Name;
        if (vectorType.paramType == ParamType.Scalar)
        {
            switch (query.vectorDimension)
            {
                case 1:
                    for (int n = 0; n < laneCount; n++) {
                        source.AppendLine($"                    Vector256<float> {name}_{n} = Avx.LoadVector256({name}_ptr + {n*step,2});  // {typeName}");
                    }
                    break;
                case 2:
                    if (vectorType.dimension == 1) {  // SOA
                        source.AppendLine(
$"""
                    Vector256<float> {name}_0 = Avx.LoadVector256({name}_ptr);      // {typeName}
                    Vector256<float> {name}_1 = Avx.LoadVector256({name}_ptr + 8);  // {typeName}
""");
                    } else {
                        source.AppendLine(
$"""
                    Vector256<float> {name}_scalar_01 = Avx.LoadVector256({name}_ptr);      // {typeName}
                    Vector256<float> {name}_scalar_23 = Avx.LoadVector256({name}_ptr + 8);  // {typeName}
                    Vector256<float> {name}_0 = Avx2.PermuteVar8x32({name}_scalar_01, {name}_mask_lo);
                    Vector256<float> {name}_1 = Avx2.PermuteVar8x32({name}_scalar_01, {name}_mask_hi);
                    Vector256<float> {name}_2 = Avx2.PermuteVar8x32({name}_scalar_23, {name}_mask_lo);
                    Vector256<float> {name}_3 = Avx2.PermuteVar8x32({name}_scalar_23, {name}_mask_hi);
""");
                    }
                    break;
                case 3:
                case 4:
                    if (vectorType.dimension == 1) {  // SOA
                        source.AppendLine(
$"""
                    Vector256<float> {name}_0 = Avx.LoadVector256({name}_ptr);      // {typeName}
""");
                    } else {
                        source.AppendLine($"                    Vector256<float> {name}_scalar = Avx.LoadVector256({name}_ptr);  // {typeName}");
                        for (int n = 0; n < laneCount; n++) {
                            source.AppendLine($"                    Vector256<float> {name}_{n} = Avx2.PermuteVar8x32({name}_scalar, {name}_mask_{n});");
                        }
                    }
                    break;
                /* case 4:
                    source.AppendLine($"                    Vector256<float> {name}_scalar = Avx.LoadVector256({name}_ptr);  // {typeName}");
                    for (int n = 0; n < laneCount; n++) {
                        source.AppendLine($"                    Vector256<float> {name}_{n} = Avx2.PermuteVar8x32({name}_scalar, {name}_mask_{n});");
                    }
                    break; */
            }
        } else {
            for (int n = 0; n < laneCount; n++) {
                source.AppendLine($"                    Vector256<float> {name}_{n} = Avx.LoadVector256({name}_ptr + {n*step,2});   // {typeName}");
            }
        }
        if (query.useSoA && vectorType.dimension > 1) {  // SOA
            switch (query.vectorDimension) {
                case 2:
                    source.AppendLine($"                    ({name}_0, {name}_1) = AvxVector2.Deinterleave({name}_0, {name}_1);");
                    source.AppendLine($"                    ({name}_2, {name}_3) = AvxVector2.Deinterleave({name}_2, {name}_3);");
                    break;
                case 3:
                    source.AppendLine($"                    ({name}_0, {name}_1, {name}_2) = AvxVector3.Deinterleave({name}_0, {name}_1, {name}_2);");
                    break;
                case 4:
                    source.AppendLine($"                    ({name}_0, {name}_1, {name}_2, {name}_3) = AvxVector4.Deinterleave({name}_0, {name}_1, {name}_2, {name}_3);");
                    break;
            }
        }
        source.AppendLine();
    }
    
    private static void EmitStoreComponentVector(StringBuilder source, Query query, VectorType vectorType, int step)
    {
        if (!vectorType.isComponent) {
            return;
        }
        var name = vectorType.parameter.Name;
        if (query.useSoA) {
            switch (vectorType.dimension) {
                case 1:
                    break;
                case 2:
                    source.AppendLine($"                    ({name}_0, {name}_1) = AvxVector2.Interleave({name}_0, {name}_1);");
//                    if (vectorType.paramType == ParamType.Vector) {
                        source.AppendLine($"                    ({name}_2, {name}_3) = AvxVector2.Interleave({name}_2, {name}_3);");
//                    }
                    break;
                case 3:
                    source.AppendLine($"                    ({name}_0, {name}_1, {name}_2) = AvxVector3.Interleave({name}_0, {name}_1, {name}_2);");
                    break;
                case 4:
                    source.AppendLine($"                    ({name}_0, {name}_1, {name}_2, {name}_3) = AvxVector4.Interleave({name}_0, {name}_1, {name}_2, {name}_3);");
                    break;
            }
        }
        var laneCount = query.laneCount;
        if (query.vectorDimension == 2 && vectorType.dimension == 1) {
            laneCount = 2; //       TODO vech support for Vector3 & Vector4
        }
        if (vectorType.paramType == ParamType.Scalar)
        {
            switch (vectorType.dimension)
            {
                case 1:
                    for (int n = 0; n < laneCount; n++) {
                        source.AppendLine($"                    Avx.Store({name}_ptr + {n*step,2}, {name}_{n});");
                    }
                    break;
                case 2:
                    for (int n = 0; n < laneCount; n++) {
                        source.AppendLine($"                    Avx.Store({name}_ptr + {n*step,2}, {name}_{n});");
                    } /*
                    source.AppendLine(
$"""
                    Vector256<float> {name}_scalar_01 = Avx.LoadVector256({name}_ptr);
                    Vector256<float> {name}_scalar_23 = Avx.LoadVector256({name}_ptr + 8);
                    Vector256<float> {name}_0 = Avx2.PermuteVar8x32({name}_scalar_01, {name}_mask_lo);
                    Vector256<float> {name}_1 = Avx2.PermuteVar8x32({name}_scalar_01, {name}_mask_hi);
                    Vector256<float> {name}_2 = Avx2.PermuteVar8x32({name}_scalar_23, {name}_mask_lo);
                    Vector256<float> {name}_3 = Avx2.PermuteVar8x32({name}_scalar_23, {name}_mask_hi);
"""); */
                    break;
                default:
                    for (int n = 0; n < laneCount; n++) {
                        source.AppendLine($"                    Avx.Store({name}_ptr + {n*step,2}, {name}_{n});");
                    }
                    /* source.AppendLine($"                    Vector256<float> {name}_scalar = Avx.LoadVector256({name}_ptr);");
                    for (int n = 0; n < laneCount; n++) {
                        source.AppendLine($"                    Vector256<float> {name}_{n} = Avx2.PermuteVar8x32({name}_scalar, {name}_mask_{n});");
                    } */
                    break;
            }
        } else {
            for (int n = 0; n < laneCount; n++) {
                source.AppendLine($"                    Avx.Store({name}_ptr + {n*step,2}, {name}_{n});");
            }
        }
        source.AppendLine();
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