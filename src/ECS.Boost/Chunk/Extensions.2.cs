// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class ChunkExtensions
{
    public static void Each<TEach, T1,T2>(this Chunks<T1,T2> chunks, ref TEach each)
        where TEach : IEach<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        var entities    = chunks.Entities;
        var start       = entities.Start;
        var length      = entities.Length;
        var span1       = chunks.Chunk1.Span;
        var span2       = chunks.Chunk2.Span;
        
        unsafe {
#pragma warning disable CS8500
            fixed (T1*  c1  = span1)
            fixed (T2*  c2  = span2)
#pragma warning restore CS8500
            {
                for (int i = 0; i < length; i++) {
                    // all spanX.Length == length. spanX[i] is always in bounds.
                    each.Execute(ref c1[i], ref c2[i]);
                }
            }
        }
    }
    
    public static void EachEntity<TEachEntity, T1,T2>(this Chunks<T1,T2> chunks, ref TEachEntity each)
        where TEachEntity : IEachEntity<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        var entities    = chunks.Entities;
        var start       = entities.Start;
        var length      = entities.Length;
        var spanIds     = entities.Ids;
        var span1       = chunks.Chunk1.Span;
        var span2       = chunks.Chunk2.Span;
        
        unsafe {
#pragma warning disable CS8500
            fixed (T1*  c1  = span1)
            fixed (T2*  c2  = span2)
#pragma warning restore CS8500
            fixed (int* ids = spanIds)
            {
                for (int i = 0; i < length; i++) {
                    // all spanX.Length == length. spanX[i] is always in bounds.
                    each.Execute(ref c1[i], ref c2[i], ids[i]);
                }
            }
        }
    }
}