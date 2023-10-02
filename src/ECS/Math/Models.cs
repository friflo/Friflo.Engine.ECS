// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

// --------------------------- Position ---------------------------
[StructComponent("pos")]
[StructLayout(LayoutKind.Explicit)]
public struct  Position
{
    [Browse(Never)]
    [Ignore]
    [FieldOffset(00)] public    Vector3 value;
    //
    [FieldOffset(00)] public    float   x;
    [FieldOffset(04)] public    float   y;
    [FieldOffset(08)] public    float   z;

    public override string ToString() => $"{x}, {y}, {z}";

    public Position (float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

// --------------------------- Rotation ---------------------------
[StructComponent("rot")]
[StructLayout(LayoutKind.Explicit)]
public struct  Rotation
{
    [Browse(Never)]
    [Ignore]
    [FieldOffset(00)] public    Quaternion  value;
    //
    [FieldOffset(00)] public    float       x;
    [FieldOffset(04)] public    float       y;
    [FieldOffset(08)] public    float       z;
    [FieldOffset(12)] public    float       w;
    
    public override string ToString() => $"{x}, {y}, {z}, {w}";
    
    public Rotation (float x, float y, float z, float w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
}

// --------------------------- Scale3 ---------------------------
[StructComponent("scl3")]
[StructLayout(LayoutKind.Explicit)]
public struct Scale3
{
    [Browse(Never)]
    [Ignore]
    [FieldOffset(00)] public    Vector3 value;
    //
    [FieldOffset(00)] public    float   x;
    [FieldOffset(04)] public    float   y;
    [FieldOffset(08)] public    float   z;

    public override string ToString() => $"{x}, {y}, {z}";

    public Scale3 (float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

// --------------------------- Scale ---------------------------
[StructComponent("name")]
public struct EntityName
{
                            public  string              Value   { get => value; set => SetValue(value); }
    [Browse(Never)]         public  ReadOnlySpan<byte>  UTF8    => new (utf8);

    [Browse(Never)][Ignore] private string              value;
    [Browse(Never)][Ignore] private byte[]              utf8;
    
    public override         string              ToString() => $"Name: \"{value}\"";

    public EntityName (string value) {
        Value = value;
    }
    
    private void SetValue(string value) {
        this.value  = value;
        utf8        = Encoding.UTF8.GetBytes(value);
    }
}