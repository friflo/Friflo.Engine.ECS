// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct Chunk<T>
    where T : struct, IComponent
{
    public              Span<T>     Span            => new(values, 0, Length);
    
    /// <summary>
    /// Return the components as a <see cref="Span{T}"/> of <see cref="byte"/>'s.
    /// </summary>
    /// <remarks>
    /// Example:<br/>
    /// <code>
    ///     var bytes   = component.AsSpan&lt;byte>();
    ///     var step    = component.StepVector256;
    ///     for (int n = 0; n &lt; bytes.Length; n += step) {
    ///         var slice   = bytes.Slice(n, step);
    ///         var value   = Vector256.Create&lt;byte>(slice);
    ///         var result  = Vector256.Add(value, add);
    ///         result.CopyTo(slice);
    ///     }
    /// </code>
    /// </remarks>
    public              Span<TTo>  AsSpan<TTo>() where TTo : struct
                        => MemoryMarshal.Cast<T, TTo>(new Span<T>(values, 0, Length));
    
    /// <summary>
    /// The step value in a for loop when converting the <see cref="AsSpan{TTo}"/> to a <see cref="Vector256{T}"/><br/>
    /// <br/>
    /// See example at <see cref="AsSpan{TTo}"/>.
    /// </summary>
    public              int         StepVector256   => 32 / Marshal.SizeOf<T>();

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
}

public static class ChunkExtensions
{
    public static Span<Vector3>     AsSpanVector3   (this Span <Position>  position)    => MemoryMarshal.Cast<Position, Vector3>    (position);
    public static Span<Vector3>     AsSpanVector3   (this Chunk<Position>  position)    => MemoryMarshal.Cast<Position, Vector3>    (position   .Span);
    //
    public static Span<Quaternion>  AsSpanQuaternion(this Span <Rotation>  rotation)    => MemoryMarshal.Cast<Rotation, Quaternion> (rotation);
    public static Span<Quaternion>  AsSpanQuaternion(this Chunk<Rotation>  rotation)    => MemoryMarshal.Cast<Rotation, Quaternion> (rotation   .Span);
    //    
    public static Span<Vector3>     AsSpanVector3   (this Span <Scale3>    scale)       => MemoryMarshal.Cast<Scale3,   Vector3>    (scale);
    public static Span<Vector3>     AsSpanVector3   (this Chunk<Scale3>    scale)       => MemoryMarshal.Cast<Scale3,   Vector3>    (scale      .Span);
    //
    public static Span<Matrix4x4>   AsSpanMatrix4x4 (this Span <Transform> transform)   => MemoryMarshal.Cast<Transform,Matrix4x4>  (transform);
    public static Span<Matrix4x4>   AsSpanMatrix4x4 (this Chunk<Transform> transform)   => MemoryMarshal.Cast<Transform,Matrix4x4>  (transform  .Span);
}