// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Friflo.Engine.ECS.Generators;

public static class Utils
{
    public static string GetMd5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
    
    public static string GetGenericTypeArguments(INamedTypeSymbol symbol)
    {
        var sb = new StringBuilder();
        foreach (var arg in symbol.TypeArguments) {
            if (sb.Length > 0) {
                sb.Append(", ");
            }
            var name = arg.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            sb.Append(name);
        }
        return sb.ToString();
    }
    
    public static void AppendRefKind(StringBuilder sb, RefKind refKind)
    {
        switch (refKind) {
            case RefKind.Ref:
                sb.Append("ref ");
                break;
            case RefKind.In:
                sb.Append("in ");
                break;
            case RefKind.Out:
                sb.Append("out ");
                break;
        }
    }
    
    public static  IdentifierNameSyntax GetLeft(AssignmentExpressionSyntax assignmentExpressionSyntax)
    {
        if (assignmentExpressionSyntax.Left is MemberAccessExpressionSyntax leftExpressionSyntax) {
            return leftExpressionSyntax.Expression as IdentifierNameSyntax;
        }
        return null;
    }
    
    public static  IdentifierNameSyntax GetRight(AssignmentExpressionSyntax assignmentExpressionSyntax)
    {
        if (assignmentExpressionSyntax.Right is MemberAccessExpressionSyntax leftExpressionSyntax) {
            return leftExpressionSyntax.Expression as IdentifierNameSyntax;
        }
        return null;
    }
}