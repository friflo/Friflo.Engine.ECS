// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Rotation encoded as a <a href="https://en.wikipedia.org/wiki/Quaternion">Quaternion</a>
/// described by the mathematician W.R. Hamilton.
/// </summary>
[ComponentKey("rot")]
[StructLayout(LayoutKind.Explicit)]
[ComponentSymbol("Rℍ")] // ℍ = Hamilton
public struct  Rotation : IComponent, IEquatable<Rotation>
{
    [Browse(Never)]
    [Ignore]
    [FieldOffset (0)] public    Quaternion  value;  // 16
    //
    [FieldOffset (0)] public    float       x;      // (4)
    [FieldOffset (4)] public    float       y;      // (4)
    [FieldOffset (8)] public    float       z;      // (4)
    [FieldOffset(12)] public    float       w;      // (4)
    
    public readonly override string ToString() => $"{x}, {y}, {z}, {w}";
    
    public Rotation (float x, float y, float z, float w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
    
    public          bool    Equals      (Rotation other)                    => value == other.value;
    public static   bool    operator == (in Rotation p1, in Rotation p2)    => p1.value == p2.value;
    public static   bool    operator != (in Rotation p1, in Rotation p2)    => p1.value != p2.value;

    [ExcludeFromCodeCoverage] public override   int     GetHashCode()       => throw new NotImplementedException("to avoid boxing");
    [ExcludeFromCodeCoverage] public override   bool    Equals(object obj)  => throw new NotImplementedException("to avoid boxing");
}