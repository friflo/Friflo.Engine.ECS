// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Friflo.Vectorization.Generators;

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
        if (expressionSyntax is IdentifierNameSyntax identifierNameSyntax) {
            return identifierNameSyntax;
        }
        return null;
    }
    
    public static  void InterleaveVector3(StringBuilder sb, string nm, int vectorDimension)
    {
        switch (vectorDimension) {
            case 1:
                sb.AppendLine($"            Vector128<float> {nm}_0 = Vector128.Create({nm});");
                break;
            case 2:
                sb.AppendLine($"            Vector128<float> {nm}_half = Vector128.Create({nm}.X, {nm}.Y, {nm}.X, {nm}.Y);");
                sb.AppendLine($"            var {nm}_0 = Avx.InsertVector128({nm}_half.ToVector256(), {nm}_half, 1);");
                sb.AppendLine($"            var {nm}_1 = {nm}_0;");
                sb.AppendLine($"            var {nm}_2 = {nm}_0;");
                sb.AppendLine($"            var {nm}_3 = {nm}_0;");
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
            case 1:
                // sb.AppendLine($"            Vector256<int> {name}_mask_0 = Vector256.Create( 0, 1, 2, 3, 4, 5, 6, 7);");
                break;
            case 2:
                sb.AppendLine($"            Vector256<int> {name}_mask_lo = Vector256.Create( 0, 0, 1, 1, 2, 2, 3, 3);");
                sb.AppendLine($"            Vector256<int> {name}_mask_hi = Vector256.Create( 4, 4, 5, 5, 6, 6, 7, 7);");
                sb.AppendLine();
                break;
            case 3:
                sb.AppendLine($"            Vector256<int> {name}_mask_0 = Vector256.Create(0, 0, 0, 1, 1, 1, 2, 2);");
                sb.AppendLine($"            Vector256<int> {name}_mask_1 = Vector256.Create(2, 3, 3, 3, 4, 4, 4, 5);");
                sb.AppendLine($"            Vector256<int> {name}_mask_2 = Vector256.Create(5, 5, 6, 6, 6, 7, 7, 7);");
                sb.AppendLine();
                break;
            case 4:
                sb.AppendLine($"            Vector256<int> {name}_mask_0 = Vector256.Create(0, 0, 0, 0, 1, 1, 1, 1);");
                sb.AppendLine($"            Vector256<int> {name}_mask_1 = Vector256.Create(2, 2, 2, 2, 3, 3, 3, 3);");
                sb.AppendLine($"            Vector256<int> {name}_mask_2 = Vector256.Create(4, 4, 4, 4, 5, 5, 5, 5);");
                sb.AppendLine($"            Vector256<int> {name}_mask_3 = Vector256.Create(6, 6, 6, 6, 7, 7, 7, 7);");
                sb.AppendLine();
                break;
        }
    }

    public static void LoadMatrix(StringBuilder sb, string nm)
    {
        sb.AppendLine($"            // Load Matrix columns into 256-bit registers (each column duplicated)");
        sb.AppendLine($"            // [Col0.x, Col0.y, Col0.z, Col0.w, Col0.x, Col0.y, Col0.z, Col0.w]");
        sb.AppendLine($"            Vector256<float> {nm}_0 = Vector256.Create({nm}.M11, {nm}.M12, {nm}.M13, {nm}.M14, {nm}.M11, {nm}.M12, {nm}.M13, {nm}.M14);");
        sb.AppendLine($"            Vector256<float> {nm}_1 = Vector256.Create({nm}.M21, {nm}.M22, {nm}.M23, {nm}.M24, {nm}.M21, {nm}.M22, {nm}.M23, {nm}.M24);");
        sb.AppendLine($"            Vector256<float> {nm}_2 = Vector256.Create({nm}.M31, {nm}.M32, {nm}.M33, {nm}.M34, {nm}.M31, {nm}.M32, {nm}.M33, {nm}.M34);");
        sb.AppendLine($"            Vector256<float> {nm}_3 = Vector256.Create({nm}.M41, {nm}.M42, {nm}.M43, {nm}.M44, {nm}.M41, {nm}.M42, {nm}.M43, {nm}.M44);");
/*
    Vector256<float> col0 = Vector256.Create(matrix.M11, matrix.M12, matrix.M13, matrix.M14, matrix.M11, matrix.M12, matrix.M13, matrix.M14);
    Vector256<float> col1 = Vector256.Create(matrix.M21, matrix.M22, matrix.M23, matrix.M24, matrix.M21, matrix.M22, matrix.M23, matrix.M24);
    Vector256<float> col2 = Vector256.Create(matrix.M31, matrix.M32, matrix.M33, matrix.M34, matrix.M31, matrix.M32, matrix.M33, matrix.M34);
    Vector256<float> col3 = Vector256.Create(matrix.M41, matrix.M42, matrix.M43, matrix.M44, matrix.M41, matrix.M42, matrix.M43, matrix.M44);
 */
    }

    public static void TrimEnd(StringBuilder stringBuilder)
    {
        var len = stringBuilder.Length - 1;
        while (stringBuilder[len] == '\n' ||
               stringBuilder[len] == '\r')
        {
            stringBuilder.Length = len--;
        }
    }
    
    public  static void Append(this StringBuilder[] sb, string text)
    {
        for (int n = 0; n < sb.Length; n++) {
            sb[n].Append(text);
        }
    }
}