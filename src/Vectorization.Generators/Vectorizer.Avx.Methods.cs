// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ForCanBeConvertedToForeach
namespace Friflo.Vectorization.Generators;

public static partial class Vectorizer
{
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
            
            case "Vector.Length()":                         return Method_Length    (lanes, query, invocation);
            
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
            case 2:
                query.computeTemp.AppendLine($"                    var ({result}_0, {result}_1) = AvxVector2.Normalize({arg0}_0, {arg0}_1);");
                query.computeTemp.AppendLine($"                    var ({result}_2, {result}_3) = AvxVector2.Normalize({arg0}_2, {arg0}_3);");
                lanes[0].Append($"{result}_0");
                lanes[1].Append($"{result}_1");
                lanes[2].Append($"{result}_2");
                lanes[3].Append($"{result}_3");
                return true;
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
    
    private static bool Method_Length(StringBuilder[] lanes, Query query, InvocationExpressionSyntax invocation)
    {
        query.requireSoA = true;
        if (!Compute_AddTemp(query, invocation.Expression, "Length this", out var arg0)) {
            return false;
        }
        switch (query.vectorDimension)
        {
            case 2:
                return false;
            case 3:
                lanes[0].Append($"AvxVector3.Length({arg0}_0, {arg0}_1, {arg0}_2)");
                return true;
            case 4:
                lanes[0].Append($"AvxVector4.Length({arg0}_0, {arg0}_1, {arg0}_2, {arg0}_3)");
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