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
}