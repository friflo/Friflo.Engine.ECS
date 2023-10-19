// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.InteropServices;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[StructComponent("pos")]
[StructLayout(LayoutKind.Explicit)]
public struct  Position : IStructComponent
{
    [Browse(Never)]
    [Ignore]
    [FieldOffset(0)] public     Vector3 value;  // 12
    //
    [FieldOffset(0)] public     float   x;      // (4)
    [FieldOffset(4)] public     float   y;      // (4)
    [FieldOffset(8)] public     float   z;      // (4)

    public override string ToString() => $"{x}, {y}, {z}";

    public Position (float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    
    public static bool operator ==  (in Position p1, in Position p2) => p1.value == p2.value;
    public static bool operator !=  (in Position p1, in Position p2) => p1.value != p2.value;
}