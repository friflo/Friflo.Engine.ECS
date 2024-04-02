// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.InteropServices;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ComponentKey("trans")]
[StructLayout(LayoutKind.Explicit)]
public struct  Transform : IComponent
{
    [Browse(Never)]
    [Ignore]
    [FieldOffset (0)] public    Matrix4x4   value;  // 64
    
    // --- 1st row
    [FieldOffset (0)] public    float       m11;
    [FieldOffset (4)] public    float       m12;
    [FieldOffset (8)] public    float       m13;
    [FieldOffset(12)] public    float       m14;
    // --- 2nd row    
    [FieldOffset(16)] public    float       m21;
    [FieldOffset(20)] public    float       m22;
    [FieldOffset(24)] public    float       m23;
    [FieldOffset(28)] public    float       m24;
    // --- 3rd row
    [FieldOffset(32)] public    float       m31;
    [FieldOffset(36)] public    float       m32;
    [FieldOffset(40)] public    float       m33;
    [FieldOffset(44)] public    float       m34;
    // --- 4th row
    [FieldOffset(48)] public    float       m41;
    [FieldOffset(52)] public    float       m42;
    [FieldOffset(56)] public    float       m43;
    [FieldOffset(60)] public    float       m44;
}

