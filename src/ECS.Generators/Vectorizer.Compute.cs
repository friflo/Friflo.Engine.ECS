// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Friflo.Engine.ECS.Generators;

public static partial class Vectorizer
{
    private static bool Compute_MemberAccess(StringBuilder[] computeLanes, Query query, MemberAccessExpressionSyntax memberAccess)
    {
        if (memberAccess.Expression is not IdentifierNameSyntax identifierNameSyntax) {
            return false;
        }
        var name = identifierNameSyntax.Identifier.Text;
        for (int i = 0; i < 3; i++) {
            computeLanes[i].Append($"{name}_{i}");
        }
        return true;
    }
    
    private static bool Compute_IdentifierName(StringBuilder[] computeLanes, Query query, IdentifierNameSyntax identifierName)
    {
        var name = identifierName.Identifier.Text;
        if (query.paramTypes.TryGetValue(name, out var type)) {
            if (type == ParamType.Scalar) {
                for (int i = 0; i < 3; i++) {
                    computeLanes[i].Append($"{name}_scalar");
                }
                return true;
            }
        }
        for (int i = 0; i < 3; i++) {
            computeLanes[i].Append($"{name}_{i}");
        }
        return true;
    }
    
    private static bool Compute_Assignment(StringBuilder[] computeLanes, Query query, AssignmentExpressionSyntax assignment)
    {
        var avxOperation = assignment.Kind() switch
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
        var left    = Utils.GetMemberName(assignment.Left)?.Identifier.Text;
        if (left is null) {
            return false;
        }
        for (int i = 0; i < 3; i++) {
            computeLanes[i].Append($"{left}_{i} = Avx.{avxOperation}({left}_{i}, ");
        }
        if (!Compute(computeLanes, query, assignment.Right)) {
            return false;
        }
        for (int i = 0; i < 3; i++) {
            computeLanes[i].Append(");");
        }
        return true;
    }

    private static bool Compute_Binary(StringBuilder[] computeLanes, Query query, BinaryExpressionSyntax binary)
    {
        var avxOperation = binary.Kind() switch
        {
            SyntaxKind.AddExpression      => "Add",
            SyntaxKind.SubtractExpression => "Subtract",
            SyntaxKind.MultiplyExpression => "Multiply",
            SyntaxKind.DivideExpression   => "Divide",
            _                             => null
        };
        if (avxOperation is null) {
            return false;
        }
        for (int i = 0; i < 3; i++) {
            computeLanes[i].Append($"Avx.{avxOperation}(");
        }
        if (!Compute(computeLanes, query, binary.Left)) {
            return false;
        }
        for (int i = 0; i < 3; i++) {
            computeLanes[i].Append(", ");
        }
        if (!Compute(computeLanes, query, binary.Right)) {
            return false;
        }
        for (int i = 0; i < 3; i++) {
            computeLanes[i].Append(")");
        }
        return true;
    }

}