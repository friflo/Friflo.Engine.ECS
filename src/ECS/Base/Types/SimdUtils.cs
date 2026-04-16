// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable CheckNamespace

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Friflo.Engine.ECS;

internal static class SimdUtils
{
    internal static bool IsSoA<T>() => GetFieldCountSoA<T>() > 0;
    
    internal static int GetFieldCountSoA<T>()
    {
        var type = typeof(T);
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(SoAAttribute)) {
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
        foreach (var field in fields) {
            if (field.Name != "value" && field.Name != "Value") {
                continue;
            }
            if (field.FieldType == typeof(float))   { step = 32; }
            if (field.FieldType == typeof(Vector2)) { step = 16; }
            if (field.FieldType == typeof(Vector3)) { step =  8; }
            if (field.FieldType == typeof(Vector4)) { step =  8; }
        }
        return step;
    }
}