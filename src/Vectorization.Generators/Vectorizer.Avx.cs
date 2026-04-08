// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ForCanBeConvertedToForeach
namespace Friflo.Vectorization.Generators;

public static partial class Vectorizer
{
    private static bool Compute_MemberAccess(StringBuilder[] lanes, Query query, MemberAccessExpressionSyntax memberAccess)
    {
        if (memberAccess.Expression is not IdentifierNameSyntax identifierNameSyntax) {
            return false;
        }
        var symbolInfo = query.semanticModel.GetSymbolInfo(memberAccess);
        var symbol = symbolInfo.Symbol;
        var isStatic = symbol != null && symbol.IsStatic;
        if (isStatic)
        {
            // var value = symbol!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var value = $"{symbol.ContainingType.ToDisplayString()}.{symbol.Name}"; 
            var name = query.AddConst();
            /* if (symbol is IPropertySymbol typeSymbol) {
                var paramType = typeSymbol.Type.SpecialType == SpecialType.System_Single ? ParamType.Scalar : ParamType.Vector;
                query.paramTypes.Add(name, paramType);
            } */ 
            query.locals.AppendLine($"            var {name} = {value}; // static");
            var isScalar = Utils.InterleaveVector3(query.locals, name, query);
            query.AddParam(name, false, isScalar, false, 0);
            query.locals.AppendLine();
            
            for (int n = 0; n < lanes.Length; n++) {
                var vectorName = query.GetVectorName(name, n);
                lanes[n].Append(vectorName);
            }
        } else {
            var name = identifierNameSyntax.Identifier.Text;
            if (query.paramTypes.TryGetValue(name, out var paramType)) { // SOA
                if (paramType.dimension == 1 && query.vectorDimension > 1) {
                    query.requireSoA = true;
                }
            }
            for (int i = 0; i < lanes.Length; i++) {
                var vectorName = query.GetVectorName(name, i);
                lanes[i].Append(vectorName);
            }
        }
        return true;
    }
    
    private static bool Compute_IdentifierName(StringBuilder[] lanes, Query query, IdentifierNameSyntax identifierName)
    {
        var name = identifierName.Identifier.Text;
        for (int i = 0; i < lanes.Length; i++) {
            var vectorName = query.GetVectorName(name, i);
            lanes[i].Append(vectorName);
        }
        return true;
    }
    
    private static StringBuilder[] CreateLanes(Query query, ISymbol? symbol, string parameterName)
    {
        var laneCount = query.laneCount;
        ITypeSymbol? typeSymbol = null;
        if (symbol is ILocalSymbol localSymbol) {
            typeSymbol = localSymbol.Type;
        }
        if (symbol is IFieldSymbol fieldSymbol) {
            typeSymbol = fieldSymbol.Type;
        }
        // SOA
        var (specialType, dimension, dimParamType) = GetTypeDim(typeSymbol);
        if (query.useSoA && !query.paramTypes.ContainsKey(parameterName)) {
            query.AddParam(parameterName, false, true, false, dimension);    
        }
        if (query.useSoA && dimension == 1) {
            laneCount = 2;
        }
        if (query.paramTypes.TryGetValue(parameterName, out var paramType)) {
            if (query.vectorDimension == 2 && paramType.dimension == 1) {
                laneCount = 2; // TODO  assign lane count for Vector3 & Vector4
            }
        }
        var lanes = query.lanes = new StringBuilder[laneCount];
        for (int n = 0; n < laneCount; n++) {
            lanes[n] = new StringBuilder();
        }
        return lanes;
    }
    
    private static bool Compute_Assignment(StringBuilder[] lanes, Query query, AssignmentExpressionSyntax assignment)
    {
        var kind = assignment.Kind();
        var avxOperation = kind switch
        {
            SyntaxKind.SimpleAssignmentExpression   => "",
            SyntaxKind.AddAssignmentExpression      => "Avx.Add",
            SyntaxKind.SubtractAssignmentExpression => "Avx.Subtract",
            SyntaxKind.MultiplyAssignmentExpression => "Avx.Multiply",
            SyntaxKind.DivideAssignmentExpression   => "Avx.Divide",
            _                                       => null
        };
        if (avxOperation is null) {
            query.ReportDiagnosticSyntax(Errors.OperationUnsupported, assignment);
            return false;
        }
        var leftIdentifier = Utils.GetMemberName(assignment.Left).Identifier;
        var left = leftIdentifier.Text;
        var leftSymbol = query.semanticModel.GetSymbolInfo(assignment.Left).Symbol;
        lanes = CreateLanes(query, leftSymbol, left);
        // FMA is a "Cheat Code" for:    (vel * dt) + pos    ->    Fma.MultiplyAdd(vel, dt, pos);
        if (kind == SyntaxKind.AddAssignmentExpression && 
            assignment.Right is BinaryExpressionSyntax assignBinary && assignBinary.Kind() is SyntaxKind.MultiplyExpression)
        {
            for (int i = 0; i < lanes.Length; i++) {
                var vectorName = query.GetVectorName(left, i);
                lanes[i].Append($"{vectorName} = Fma.MultiplyAdd(");
            }
            if (!Compute(lanes, query, assignBinary.Left)) {
                return false;
            }
            lanes.Append(", ");
            if (!Compute(lanes, query, assignBinary.Right)) {
                return false;
            }
            for (int i = 0; i < lanes.Length; i++) {
                var vectorName = query.GetVectorName(left, i);
                lanes[i].Append($", {vectorName});");
            }
            return true;
        }
        for (int i = 0; i < lanes.Length; i++) {
            var vectorName = query.GetVectorName(left, i);
            if (kind == SyntaxKind.SimpleAssignmentExpression) {
                lanes[i].Append($"{vectorName} = ");
            } else {
                lanes[i].Append($"{vectorName} = {avxOperation}({vectorName}, ");
            }
        }
        if (!Compute(lanes, query, assignment.Right)) {
            return false;
        }
        lanes.Append(kind == SyntaxKind.SimpleAssignmentExpression ? ";" : ");");
        return true;
    }

    private static bool Compute_Binary(StringBuilder[] lanes, Query query, BinaryExpressionSyntax binary)
    {
        var kind = binary.Kind();
        var avxOperation = kind switch
        {
            SyntaxKind.AddExpression      => "Add",
            SyntaxKind.SubtractExpression => "Subtract",
            SyntaxKind.MultiplyExpression => "Multiply",
            SyntaxKind.DivideExpression   => "Divide",
            _                             => null
        };
        if (avxOperation is null) {
            query.ReportDiagnosticSyntax(Errors.OperationUnsupported, binary);
            return false;
        }

        // is reciprocal square root:     left / Sqrt(right) 
        if (kind == SyntaxKind.DivideExpression) {
            if (binary.Right is InvocationExpressionSyntax rightInvocation &&
                GetMethodName(query, rightInvocation) == "System.MathF.Sqrt(float)")
            {
                lanes.Append("Avx.Multiply(Avx.ReciprocalSqrt(");
                if (!Compute(lanes, query, rightInvocation.ArgumentList.Arguments[0].Expression)) {
                    return false;
                }
                lanes.Append("), ");
                if (!Compute(lanes, query, binary.Left)) {
                    return false;
                }
                lanes.Append(")");
                return true;
            }
        }
        // FMA is a "Cheat Code" for:    (vel * dt) + pos    ->    Fma.MultiplyAdd(vel, dt, pos);
        if (kind == SyntaxKind.AddExpression && 
            binary.Left is BinaryExpressionSyntax multiplyBinary && multiplyBinary.Kind() is SyntaxKind.MultiplyExpression)
        {
            lanes.Append("Fma.MultiplyAdd(");
            if (!Compute(lanes, query, multiplyBinary.Left)) {
                return false;
            }
            lanes.Append(", ");
            if (!Compute(lanes, query, multiplyBinary.Right)) {
                return false;
            }
            lanes.Append(", ");
            if (!Compute(lanes, query, binary.Right)) {
                return false;
            }
            lanes.Append(")");
            return true;
        }
        for (int i = 0; i < lanes.Length; i++) {
            lanes[i].Append($"Avx.{avxOperation}(");
        }
        if (!Compute(lanes, query, binary.Left)) {
            return false;
        }
        lanes.Append(", ");
        if (!Compute(lanes, query, binary.Right)) {
            return false;
        }
        lanes.Append(")");
        return true;
    }

    private static string? GetMethodName(Query query, InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var symbolInfo = query.semanticModel.GetSymbolInfo(memberAccess);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol) {
                return methodSymbol.ToDisplayString();
            }
        }
        return null;
    }

    private static bool Compute_Literal(StringBuilder[] lanes, Query query, LiteralExpressionSyntax literal)
    {
        var name = query.AddConst();
        query.locals.AppendLine($"            var {name}_scalar = Vector256.Create<float>({literal.Token.Text}); // literal");
        query.locals.AppendLine();
        for (int n = 0; n < lanes.Length; n++) {
            lanes[n].Append($"{name}_scalar");
        }
        return true;
    }

    private static bool Compute_Invocation(StringBuilder[] lanes, Query query, InvocationExpressionSyntax invocation)
    {
        var methodName = GetMethodName(query, invocation);
        var methodReduced = methodName?.Replace("System.Numerics.Vector2", "Vector")
                                       .Replace("System.Numerics.Vector3", "Vector")
                                       .Replace("System.Numerics.Vector4", "Vector");
        var argList = invocation.ArgumentList;
        switch (methodReduced)
        {
            case "System.MathF.Sin(float)":         return Method_Scalar    (lanes, query, "MathUtils.SinMathF",    argList);
            case "System.MathF.Cos(float)":         return Method_Scalar    (lanes, query, "MathUtils.CosMathF",    argList);
            case "System.MathF.Tan(float)":         return Method_Scalar    (lanes, query, "MathUtils.TanMathF",    argList);
            case "System.MathF.Asin(float)":        return Method_Scalar    (lanes, query, "MathUtils.AsinMathF",   argList);
            case "System.MathF.Acos(float)":        return Method_Scalar    (lanes, query, "MathUtils.AcosMathF",   argList);
            case "System.MathF.Atan(float)":        return Method_Scalar    (lanes, query, "MathUtils.AtanMathF",   argList);
            case "System.MathF.Atan2(float, float)":return Method_Scalar    (lanes, query, "MathUtils.Atan2MathF",  argList);
            case "System.MathF.Asinh(float)":       return Method_Scalar    (lanes, query, "MathUtils.AsinhMathF",  argList);
            case "System.MathF.Acosh(float)":       return Method_Scalar    (lanes, query, "MathUtils.AcoshMathF",  argList);
            case "System.MathF.Atanh(float)":       return Method_Scalar    (lanes, query, "MathUtils.AtanhMathF",  argList);
            
            case "Vector.Abs(Vector)":
            case "System.MathF.Abs(float)":         return Method_Abs       (lanes, query,                          argList);
            case "Vector.Truncate(Vector)":
            case "System.MathF.Truncate(float)":    return Method_Truncate  (lanes, query,                          argList);
            case "Vector.Round(Vector)":
            case "System.MathF.Round(float)":       return Method_Round     (lanes, query,                          argList);
            case "System.MathF.Floor(float)":       return Method_Floor     (lanes, query,                          argList);
            case "System.MathF.Ceiling(float)":     return Method_Ceiling   (lanes, query,                          argList);
            
            case "System.MathF.Exp(float)":         return Method_Scalar    (lanes, query, "Vector256.Exp",         argList);
            case "System.MathF.Log(float)":         return Method_Scalar    (lanes, query, "Vector256.Log",         argList);
            case "System.MathF.Log10(float)":       return Method_Scalar    (lanes, query, "MathUtils.Log10MathF",  argList);
            case "System.MathF.Log2(float)":        return Method_Scalar    (lanes, query, "Vector256.Log2",        argList);
            case "System.MathF.Pow(float, float)":  return Method_Scalar    (lanes, query, "MathUtils.PowMathF",    argList);
            case "System.MathF.Sqrt(float)":        return Method_Scalar    (lanes, query, "Avx.Sqrt",              argList);
            
            case "System.MathF.Min(float, float)":
            case "Vector.Min(Vector, Vector)":              return Method_MinMax(lanes, query, "Min", argList);
            
            case "System.MathF.Max(float, float)":
            case "Vector.Max(Vector, Vector)":              return Method_MinMax(lanes, query, "Max", argList);
            
            case "System.Math.Clamp(float, float, float)":
            case "Vector.Clamp(Vector, Vector, Vector)":    return Method_Clamp     (lanes, query, argList);
            
            case "Vector.Lerp(Vector, Vector, float)":
            case "Vector.Lerp(Vector, Vector, Vector)":     return Method_Lerp      (lanes, query, argList);
            
            // --- methods require Deinterleave
            case "Vector.Cross(Vector, Vector)":            return Method_Cross     (lanes, query, argList);
            
            case "Vector.Normalize(Vector)":                return Method_Normalize (lanes, query, argList);
            
            case "Vector.Transform(Vector, System.Numerics.Matrix4x4)":
                return Method_Vector4_Transform(lanes, query, argList);
        }
        query.ReportDiagnosticSyntax(Errors.OperationUnsupported, invocation, invocation.ToFullString());
        return false;
    }

    private static bool Method_Vector4_Transform(StringBuilder[] lanes, Query query, ArgumentListSyntax argumentSyntax)
    {
        if (query.vectorDimension != 4) {
            return false;
        }
        /* for (int n = 0; n < lanes.Length; n++) {
            lanes[n].Append($"AvxUtils.TransformVector4PairAVX2(default, default, default, default, default)");
        }
        return true; */
        var args = argumentSyntax.Arguments;
        lanes.Append("AvxUtils.TransformVector4PairAVX(");
        if (!Compute(lanes, query, args[0].Expression)) {
            return false;
        }
        if (args[1].Expression is IdentifierNameSyntax identifierNameSyntax) {
            var matrixName = identifierNameSyntax.Identifier.Text;
            for (int n = 0; n < lanes.Length; n++) {
                lanes[n].Append($", {matrixName}_0, {matrixName}_1, {matrixName}_2, {matrixName}_3)");
            }
        }
        return true;
    }

    private static bool Method_MinMax(StringBuilder[] lanes, Query query, string op, ArgumentListSyntax argumentSyntax)
    {
        var args = argumentSyntax.Arguments;
        for (int n = 0; n < lanes.Length; n++) {
            lanes[n].Append($"Avx.{op}(");
        }
        if (!Compute(lanes, query, args[0].Expression)) {
            return false;
        }
        lanes.Append(", ");
        if (!Compute(lanes, query, args[1].Expression)) {
            return false;
        }
        lanes.Append(")");
        return true;
    }
    
    private static bool Method_Clamp(StringBuilder[] lanes, Query query, ArgumentListSyntax argumentSyntax)
    {
        var args = argumentSyntax.Arguments;
        lanes.Append("Avx.Min(");
        if (!Compute(lanes, query, args[2].Expression)) {
            return false;
        }
        lanes.Append(", Avx.Max(");
        if (!Compute(lanes, query, args[1].Expression)) {
            return false;
        }
        lanes.Append(", ");
        if (!Compute(lanes, query, args[0].Expression)) {
            return false;
        }
        lanes.Append("))");
        return true;
    }
    
    private static bool Method_Lerp(StringBuilder[] lanes, Query query, ArgumentListSyntax argumentSyntax)
    {
        var args = argumentSyntax.Arguments;
        lanes.Append("Fma.MultiplyAdd(");
        if (!Compute(lanes, query, args[2].Expression)) {
            return false;
        }
        lanes.Append(", Avx.Subtract(");
        if (!Compute(lanes, query, args[1].Expression)) {
            return false;
        }
        lanes.Append(", ");
        if (!Compute(lanes, query, args[0].Expression)) {
            return false;
        }
        lanes.Append("), ");
        if (!Compute(lanes, query, args[0].Expression)) {
            return false;
        }
        lanes.Append(")");
        return true;
    }

    private static bool Method_Abs(StringBuilder[] lanes, Query query, ArgumentListSyntax argumentSyntax)
    {
        var name = query.AddConst();
        query.locals.AppendLine($"            var {name} = Vector256.Create(0x7FFFFFFF).AsSingle(); // Abs()");
        query.locals.AppendLine();
        lanes.Append("Avx.And(");
        var args = argumentSyntax.Arguments;
        if (!Compute(lanes, query, args[0].Expression)) {
            return false;
        }
        for (int n = 0; n < lanes.Length; n++) {
            lanes[n].Append($", {name})");
        }
        return true;
    }
    
    private static bool Method_Truncate(StringBuilder[] lanes, Query query, ArgumentListSyntax argumentSyntax)
    {
        lanes.Append("Vector256.Truncate(");    // alternative: Avx.RoundToNearestInteger(v, 0x03 | 0x08);
        var args = argumentSyntax.Arguments;
        if (!Compute(lanes, query, args[0].Expression)) {
            return false;
        }
        lanes.Append(")");
        return true;
    }
    
    private static bool Method_Floor(StringBuilder[] lanes, Query query, ArgumentListSyntax argumentSyntax)
    {
        lanes.Append("Vector256.Floor(");       // alternative: Avx.RoundToNearestInteger(value, 0x01 | 0x08);
        var args = argumentSyntax.Arguments;
        if (!Compute(lanes, query, args[0].Expression)) {
            return false;
        }
        lanes.Append(")");
        return true;
    }
    
    private static bool Method_Ceiling(StringBuilder[] lanes, Query query, ArgumentListSyntax argumentSyntax)
    {
        lanes.Append("Vector256.Ceiling(");     // alternative:  Avx.RoundToNearestInteger(value, 0x02 | 0x08);
        var args = argumentSyntax.Arguments;
        if (!Compute(lanes, query, args[0].Expression)) {
            return false;
        }
        lanes.Append(")");
        return true;
    }
    
    private static bool Method_Round(StringBuilder[] lanes, Query query, ArgumentListSyntax argumentSyntax)
    {
        lanes.Append("Vector256.Round(");       // alternative:  Avx.RoundToNearestInteger(value, 0x00 | 0x08);
        var args = argumentSyntax.Arguments;
        if (!Compute(lanes, query, args[0].Expression)) {
            return false;
        }
        lanes.Append(")");
        return true;
    }

    private static bool Method_Scalar(StringBuilder[] lanes, Query query, string method, ArgumentListSyntax argumentSyntax)
    {
        for (int n = 0; n < lanes.Length; n++) {
            lanes[n].Append($"{method}(");
        }
        var args = argumentSyntax.Arguments;
        for (int i = 0; i < args.Count; i++)
        {
            if (i > 0) {
                lanes.Append(", ");
            }
            if (!Compute(lanes, query, args[i].Expression)) {
                return false;
            }
        }
        lanes.Append(")");
        return true;
    }
    
    private static bool Method_Cross(StringBuilder[] lanes, Query query, ArgumentListSyntax argumentSyntax)
    {
        query.requireSoA = true;
        var args = argumentSyntax.Arguments;
        if (!Compute_AddTemp(query, args[0].Expression, "Cross arg[0]", out var a)) {
            return false;
        }
        if (!Compute_AddTemp(query, args[1].Expression, "Cross arg[1]", out var b)) {
            return false;
        }
        if (query.vectorDimension == 2) {
            lanes[0].Append($"Fma.MultiplySubtract({a}_0, {b}_1, Avx.Multiply({a}_1, {b}_0))");
            lanes[1].Append($"Fma.MultiplySubtract({a}_2, {b}_3, Avx.Multiply({a}_3, {b}_2))");
        }
        if (query.vectorDimension == 3 || query.vectorDimension == 4) {
            lanes[0].Append($"Fma.MultiplySubtract({a}_1, {b}_2, Avx.Multiply({a}_2, {b}_1))");
            lanes[1].Append($"Fma.MultiplySubtract({a}_2, {b}_0, Avx.Multiply({a}_0, {b}_2))");
            lanes[2].Append($"Fma.MultiplySubtract({a}_0, {b}_1, Avx.Multiply({a}_1, {b}_0))");
            if (query.vectorDimension == 4) {
                lanes[3].Append($"Avx.Multiply({a}_3, {b}_3)");
            }
        }
        return true;
    }
    
    private static bool Method_Normalize(StringBuilder[] lanes, Query query, ArgumentListSyntax argumentSyntax)
    {
        query.requireSoA = true;
        var args = argumentSyntax.Arguments;
        if (!Compute_AddTemp(query, args[0].Expression, "Normalize arg[0]", out var arg0)) {
            return false;
        }
        var result = query.AddTemp();
        switch (query.vectorDimension)
        {
            case 3:
                query.computeTemp.AppendLine($"                    var ({result}_0, {result}_1, {result}_2) = AvxVector3.Normalize({arg0}_0, {arg0}_1, {arg0}_2);");
                lanes[0].Append($"{result}_0");
                lanes[1].Append($"{result}_1");
                lanes[2].Append($"{result}_2");
                return true;
            case 4:
                query.computeTemp.AppendLine($"                    var ({result}_0, {result}_1, {result}_2, {result}_3) = AvxVector4.Normalize({arg0}_0, {arg0}_1, {arg0}_2, {arg0}_3);");
                lanes[0].Append($"{result}_0");
                lanes[1].Append($"{result}_1");
                lanes[2].Append($"{result}_2");
                lanes[3].Append($"{result}_3");
                return true;
        }
        return false;
    }
    
    private static bool Compute_AddTemp(Query query, ExpressionSyntax expressionSyntax, string comment, out string temp)
    {
        temp = query.AddTemp();
        var tempLanes = new StringBuilder[query.laneCount];
        query.computeTemp.AppendLine($"                    //   {comment}");
        for (int n = 0; n < tempLanes.Length; n++) {
            tempLanes[n] = new StringBuilder();
            tempLanes[n].Append($"                    Vector256<float> {temp}_{n} = ");
        }
        if (!Compute(tempLanes, query, expressionSyntax)) {
            return false;
        }
        tempLanes.Append(";");
        for (int n = 0; n < tempLanes.Length; n++) {
            query.computeTemp.Append(tempLanes[n]);
            query.computeTemp.AppendLine();
        }
        query.computeTemp.AppendLine();
        return true;
    } 

}