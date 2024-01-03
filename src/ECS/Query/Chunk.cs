// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct Chunk<T>
    where T : struct, IComponent
{
    public              Span<T>     Values      => new(values, 0, length);
    public override     string      ToString()  => $"Length: {length}";

    private readonly    T[]         values;     //  8
    public  readonly    int         length;     //  4
    
    internal Chunk(T[] values, T[] copy, int length) {
        this.length      = length;
        if (copy == null) {
            this.values = values;
        } else {
            Array.Copy(values, copy, length);
            this.values = copy;
        }
    }
}

// ReSharper disable InconsistentNaming
public static class ChunkExtensions
{
    public static Span<Vector3>     AsVector3   (this Chunk<Position>  position)    => MemoryMarshal.Cast<Position, Vector3>    (position   .Values);
    public static Span<Quaternion>  AsQuaternion(this Chunk<Rotation>  rotation)    => MemoryMarshal.Cast<Rotation, Quaternion> (rotation   .Values);
    public static Span<Vector3>     AsVector3   (this Chunk<Scale3>    scale)       => MemoryMarshal.Cast<Scale3,   Vector3>    (scale      .Values);
    public static Span<Matrix4x4>   AsMatrix4x4 (this Chunk<Transform> transform)   => MemoryMarshal.Cast<Transform,Matrix4x4>  (transform  .Values);
}