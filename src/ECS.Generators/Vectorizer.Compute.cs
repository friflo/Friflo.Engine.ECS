// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Friflo.Engine.ECS.Generators;

public static partial class Vectorizer
{
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
        var right   = Utils.GetMemberName(assignment.Right)?.Identifier.Text;
        if (left is null || right is null) {
            return false;
        }
        for (int i = 0; i < 3; i++) {
            computeLanes[i].Append($"{left}_{i} = Avx.{avxOperation}({left}_{i}, {right}_{i});");
        }
        return true;
    }

}