// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable CheckNamespace

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Friflo.Engine.ECS;

internal enum Layout
{
    AoS,
    SoA,
    AoSoA
}

public static class SimdInfo<T>
    where T : struct
{
    internal static readonly  Layout  Layout          = SimdUtils.GetLayout<T>();
    
    public static readonly    int     FieldCountSoA   = SimdUtils.GetFieldCountSoA<T>(out _);
    
    /// <summary>
    /// Is always a multiple of 8. The enables the stride returned from
    /// <see cref="Chunk{T}.GetStrideSoA"/> enables access to 32 byte aligned memory for all lanes.
    /// </summary>
    public static readonly    int     SimdStep        = SimdUtils.GetSimdStep<T>();
}


internal static class SimdUtils
{
    internal const int LaneWidth = 8;
        
    internal static Layout GetLayout<T>()
    {
        GetFieldCountSoA<T>(out var layout);
        return layout;
    }

    internal static int GetFieldCountSoA<T>(out Layout layout)
    {
        var type = typeof(T);
        layout = Layout.AoS;
        foreach (var attr in type.CustomAttributes)
        {
            var attributeType = attr.AttributeType;
            if          (attributeType == typeof(SoAAttribute)) {
                layout = Layout.SoA;
            } else if   (attributeType == typeof(AoSoAAttribute)) {
                layout = Layout.AoSoA;
            } else {
                layout = Layout.AoS;
                continue;
            }
            var fields = type.GetFields();
            var dimension = 0;
            foreach (var field in fields) {
                if (field.Name != "value" && field.Name != "Value") {
                    continue;
                }
                if (field.FieldType == typeof(Vector2)) { dimension = 2; }
                if (field.FieldType == typeof(Vector3)) { dimension = 3; }
                if (field.FieldType == typeof(Vector4)) { dimension = 4; }
                
                if (Unsafe.SizeOf<T>() != dimension * 4) {
                    dimension = 0;
                }
            }
            return dimension;
        }
        return 0;
    }
    
    internal static int  GetSimdStep<T>()
    {
        var type = typeof(T);
        var fields = type.GetFields();
        var step = 0;
        var size = 0;
        foreach (var field in fields) {
            if (field.Name != "value" && field.Name != "Value") {
                continue;
            }
            // Important Requirement!   step must always be a multiple of 8.
            //   This ensures the stride used for SoA lanes enables 32 byte aligned access to lane[1] [2] and [3].
            if (field.FieldType == typeof(float))   { step = 32; size =  4; }
            if (field.FieldType == typeof(Vector2)) { step = 16; size =  8; }
            if (field.FieldType == typeof(Vector3)) { step =  8; size = 12; }
            if (field.FieldType == typeof(Vector4)) { step =  8; size = 16; }
            
            if (Unsafe.SizeOf<T>() != size) {
                step = 0;
            }
        }
        return step;
    }
}