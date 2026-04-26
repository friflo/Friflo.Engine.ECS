// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable CheckNamespace

using System;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Friflo.Engine.ECS;

internal enum Layout
{
                            AoS,
    /** 32-byte aligned */  AoSSimd, 
    /** 32-byte aligned */  SoA,
    /** 32-byte aligned */  AoSoA
}

public static class SimdInfo<T>
    where T : struct
{
    internal static readonly  Layout  Layout          = SimdUtils.GetLayout<T>();
    
    public static readonly    int     FieldCountSoA   = SimdUtils.GetFieldCountSoA<T>();
    
    /// <summary>
    /// Is always a multiple of 8. The enables the stride returned from
    /// <see cref="Chunk{T}.GetStrideSoA"/> enables access to 32 byte aligned memory for all lanes.
    /// </summary>
    public static readonly    int     SimdStep        = SimdUtils.GetSimdStep<T>();
}


internal static class SimdUtils
{
    internal const int LaneWidth = 8;
        
    internal static Layout GetLayout<T>()  where T : struct
    {
        var fieldCount = GetFieldCountSoA<T>();
        if (fieldCount == 0) {
            return Layout.AoS;
        }
        var type = typeof(T);
        return GetLayout(type, fieldCount);
    }

    internal static int GetFieldCountSoA<T>()
    {
        var type        = typeof(T);
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            var dimension = GetDimension(field);
            if (dimension > 0) {
                if (Unsafe.SizeOf<T>() != dimension * 4) {
                    return 0;
                }
                return dimension;
            }
        }
        return 0;
    }
    
    private static int GetDimension(FieldInfo fieldInfo)
    {
        var fieldName = fieldInfo.Name;
        if (fieldName != "value" && fieldName != "Value") {
            return 0;
        }
        var fieldType = fieldInfo.FieldType;
        if (fieldType == typeof(float))   return 1;
        if (fieldType == typeof(Vector2)) return 2;
        if (fieldType == typeof(Vector3)) return 3;
        if (fieldType == typeof(Vector4)) return 4;
        return 0;
    }
    
    private static Layout GetLayout(Type type, int fieldCount)
    {
        foreach (var attr in type.CustomAttributes)
        {
            var attributeType = attr.AttributeType;
            if (attributeType == typeof(AoSoAAttribute)) {
                return Layout.AoSoA;
            }
            if (attributeType == typeof(SoAAttribute)) {
                return Layout.SoA;
            }
        }
        switch (fieldCount) {
            case 1: return Layout.AoSSimd;
            case 4: return Layout.AoSSimd;
        }
        return Layout.AoS;
    }
    
    internal static int  GetSimdStep<T>() where T : struct
    {
        var fieldCount = SimdInfo<T>.FieldCountSoA;
        // Important Requirement!   step must always be a multiple of 8.
        //   This ensures the stride used for SoA lanes enables 32 byte aligned access to lane[1] [2] and [3].
        switch (fieldCount)
        {
            case 1: return 32;
            case 2: return 16;
            case 3: return  8;
            case 4: return  8;
        }
        return 0;
    }
}