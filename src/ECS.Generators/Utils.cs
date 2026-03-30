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
    
    public static  IdentifierNameSyntax GetMemberName(ExpressionSyntax expressionSyntax)
    {
        if (expressionSyntax is MemberAccessExpressionSyntax leftExpressionSyntax) {
            return leftExpressionSyntax.Expression as IdentifierNameSyntax;
        }
        return null;
    }
    
    public static  void InterleaveVector3(StringBuilder sb, string nm, int vectorDimension)
    {
        switch (vectorDimension) {
            case 2:
                sb.AppendLine($"            Vector128<float> {nm}_half = Vector128.Create({nm}.X, {nm}.Y, {nm}.X, {nm}.Y);");
                sb.AppendLine($"            var {nm}_0 = Avx.InsertVector128({nm}_half.ToVector256(), {nm}_half, 1);");
                sb.AppendLine($"            var {nm}_1 = {nm}_0;");
            //  sb.AppendLine($"            var {nm}_2 = {nm}_0;");
            //  sb.AppendLine($"            var {nm}_3 = {nm}_0;");
                break;
            case 3:
                sb.AppendLine($"            var {nm}_0 = Vector256.Create({nm}.X, {nm}.Y, {nm}.Z, {nm}.X, {nm}.Y, {nm}.Z, {nm}.X, {nm}.Y);");
                sb.AppendLine($"            var {nm}_1 = Vector256.Create({nm}.Z, {nm}.X, {nm}.Y, {nm}.Z, {nm}.X, {nm}.Y, {nm}.Z, {nm}.X);");
                sb.AppendLine($"            var {nm}_2 = Vector256.Create({nm}.Y, {nm}.Z, {nm}.X, {nm}.Y, {nm}.Z, {nm}.X, {nm}.Y, {nm}.Z);");
                break;
            case 4:
                sb.AppendLine($"            Vector128<float> {nm}_half = Vector128.Create({nm}.X, {nm}.Y, {nm}.Z, {nm}.W);");
                sb.AppendLine($"            var {nm}_0 = Avx.InsertVector128({nm}_half.ToVector256(), {nm}_half, 1);");
                sb.AppendLine($"            var {nm}_1 = {nm}_0;");
                sb.AppendLine($"            var {nm}_2 = {nm}_0;");
                sb.AppendLine($"            var {nm}_3 = {nm}_0;");
                break;
        }
    }
    
    public static  void ScalarMask(StringBuilder sb, string name, int vectorDimension)
    {
        switch (vectorDimension) {
            case 2:
                sb.AppendLine($"            Vector256<int> {name}_mask_0 = Vector256.Create( 0, 0, 1, 1, 2, 2, 3, 3);");
                sb.AppendLine($"            Vector256<int> {name}_mask_1 = Vector256.Create( 4, 4, 5, 5, 6, 6, 7, 7);");
            //  sb.AppendLine($"            Vector256<int> {name}_mask_2 = Vector256.Create( 8, 8, 9, 9,10,10,11,11);");
            //  sb.AppendLine($"            Vector256<int> {name}_mask_3 = Vector256.Create(12,12,13,13,14,14,15,15);");
                break;
            case 3:
                sb.AppendLine($"            Vector256<int> {name}_mask_0 = Vector256.Create(0, 0, 0, 1, 1, 1, 2, 2);");
                sb.AppendLine($"            Vector256<int> {name}_mask_1 = Vector256.Create(2, 3, 3, 3, 4, 4, 4, 5);");
                sb.AppendLine($"            Vector256<int> {name}_mask_2 = Vector256.Create(5, 5, 6, 6, 6, 7, 7, 7);");
                break;
            case 4:
                sb.AppendLine($"            Vector256<int> {name}_mask_0 = Vector256.Create(0, 0, 0, 0, 1, 1, 1, 1);");
                sb.AppendLine($"            Vector256<int> {name}_mask_1 = Vector256.Create(2, 2, 2, 2, 3, 3, 3, 3);");
                sb.AppendLine($"            Vector256<int> {name}_mask_2 = Vector256.Create(4, 4, 4, 4, 5, 5, 5, 5);");
                sb.AppendLine($"            Vector256<int> {name}_mask_3 = Vector256.Create(6, 6, 6, 6, 7, 7, 7, 7);");
                break;
        }
    }
}