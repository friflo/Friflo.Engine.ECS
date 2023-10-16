// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.InteropServices;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[StructComponent("scl3")]
[StructLayout(LayoutKind.Explicit)]
public struct Scale3 : IStructComponent
{
    [Browse(Never)]
    [Ignore]
    [FieldOffset(00)] public    Vector3 value;  // 12
    //
    [FieldOffset(00)] public    float   x;      // (4)
    [FieldOffset(04)] public    float   y;      // (4)
    [FieldOffset(08)] public    float   z;      // (4)

    public override string ToString() => $"{x}, {y}, {z}";

    public Scale3 (float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}