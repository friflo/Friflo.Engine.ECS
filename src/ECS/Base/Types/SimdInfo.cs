// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable CheckNamespace

using System;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Friflo.Engine.ECS;

/// <summary> Component arrays supporting 32-byte alignment are pinned. </summary>
internal enum Layout
{
                            AoS,
    /** 32-byte aligned */  AoSAligned,
    /** 32-byte aligned */  SoA,
    /** 32-byte aligned */  AoSoA
}

public static class SimdInfo<T>
    where T : struct
{
    internal static readonly  Layout  Layout        = SimdUtils.GetLayout<T>();
    
    public static readonly    int     FieldCountSoA = SimdUtils.GetFieldCountSoA<T>();
    
    /// <summary>
    /// Is always a multiple of 8. The enables the stride returned from
    /// <see cref="Chunk{T}.GetComponentSpan"/> enables access to 32 byte aligned memory for all lanes.
    /// </summary>
    public static readonly    int     ComponentStep = SimdUtils.GetComponentStep<T>();
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
        if (fieldType == typeof(float))     return 1;
        if (fieldType == typeof(Vector2))   return 2;
        if (fieldType == typeof(Vector3))   return 3;
        if (fieldType == typeof(Vector4))   return 4;
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
            case 1: return Layout.AoSAligned;
            case 2: return Layout.AoSAligned;
            case 4: return Layout.AoSAligned;
        }
        return Layout.AoS;
    }
    
    /// <summary> Returns the increment size used in a SIMD Loop. Unit: components </summary>
    internal static int  GetComponentStep<T>() where T : struct
    {
        var fieldCount = SimdInfo<T>.FieldCountSoA;
        // Important Requirement! step must always be a power of two (8, 16, 32).
        switch (fieldCount)
        {
            case 1: return 32; // float   (SIMD increment: 32 floats)
            case 2: return 16; // Vector2 (SIMD increment: 32 floats)
            case 3: return 8;  // Vector3 (SIMD increment: 24 floats)
            case 4: return 8;  // Vector4 (SIMD increment: 32 floats)
        }
        return 0; // for AoS components not used in SIMD
    }
    
    /// <summary>
    /// Calculates the required capacity for a component array. Adds additional space for types used by SIMD.
    /// SIMD includes padding to the next step boundary plus a full 'Crumple Zone' for safety.
    /// </summary>
    /// <returns>The total number of components to allocate.</returns>
    /*
    The returned component capacity (unit: components) ensures there is enough additional space for padding + 'Crumple Zone'.
    Crucial for the additional space is the Unroll size used in the SIMD loop created by the generator.
    "step" (unit components) relates to the increment (unit float) of the float* pointer for float, Vector2, Vector3, Vector4 in a single SIMD iteration.
    E.g.   factor_ptr += 8;    velocity_ptr += 32;    position_ptr += 24;
    Generator component steps (SIMD increment):  float: 32 (32)  Vector2: 16 (32)  Vector3: 8 (24)  Vector4: 8 (32)
    */
    internal static int CalcCapacity<TComponent>(int count) where TComponent : struct
    {
        var step = SimdInfo<TComponent>.ComponentStep; // Assigned with GetComponentStep<T>()
        // Handle non-math components: Just return the count as-is.
        if (step <= 0) return count;

        // 1. Calculate the Padding (Round up to the nearest multiple of 'step')
        int paddedCount = (count + step - 1) & ~(step - 1);

        // 2. Add the Crumple Zone (One full extra 'step' of safety)
        return paddedCount + step;
    }
}