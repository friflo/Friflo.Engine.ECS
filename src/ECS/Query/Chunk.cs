// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct Chunk<T>
    where T : struct, IComponent
{
    public              Span<T>     Values      => new(values, 0, Length);
    public override     string      ToString()  => $"{typeof(T).Name}[{Length}]";

    private  readonly   T[]         values;     //  8
    public   readonly   int         Length;     //  4
    
    // ReSharper disable once UnusedParameter.Local
    internal Chunk(T[] values, T[] copy, int length) {
        Length = length;
        if (copy == null) {
            this.values = values;
        } else {
            Array.Copy(values, copy, length);
            this.values = copy;
        }
    }
    
    public ref T this[int index] {
        get {
            if (index < Length) {
                return ref values[index];
            }
            throw new IndexOutOfRangeException();
        }
    }


    /*
    internal Chunk(T[] values, T[] copy, int length) {
        Length      = length;
        source      = values;
        this.values = copy ?? values;
    }
    
    internal void Copy() {
        if (source == values) {
            return;
        }
        Array.Copy(source, values, Length);
    } */
}

public static class ChunkExtensions
{
    public static Span<Vector3>     AsSpanVector3   (this Span<Position>   position)    => MemoryMarshal.Cast<Position, Vector3>    (position);
    public static Span<Quaternion>  AsSpanQuaternion(this Span<Rotation>   rotation)    => MemoryMarshal.Cast<Rotation, Quaternion> (rotation);
    public static Span<Vector3>     AsSpanVector3   (this Span<Scale3>     scale)       => MemoryMarshal.Cast<Scale3,   Vector3>    (scale);
    public static Span<Matrix4x4>   AsSpanMatrix4x4 (this Span<Transform>  transform)   => MemoryMarshal.Cast<Transform,Matrix4x4>  (transform);
    //
    public static Span<Vector3>     AsSpanVector3   (this Chunk<Position>  position)    => MemoryMarshal.Cast<Position, Vector3>    (position   .Values);
    public static Span<Quaternion>  AsSpanQuaternion(this Chunk<Rotation>  rotation)    => MemoryMarshal.Cast<Rotation, Quaternion> (rotation   .Values);
    public static Span<Vector3>     AsSpanVector3   (this Chunk<Scale3>    scale)       => MemoryMarshal.Cast<Scale3,   Vector3>    (scale      .Values);
    public static Span<Matrix4x4>   AsSpanMatrix4x4 (this Chunk<Transform> transform)   => MemoryMarshal.Cast<Transform,Matrix4x4>  (transform  .Values);
}