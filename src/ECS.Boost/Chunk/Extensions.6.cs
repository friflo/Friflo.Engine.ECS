﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class ChunkExtensions
{
    public static void Each<TEach, T1,T2,T3,T4,T5,T6>(this Chunks<T1,T2,T3,T4,T5,T6> chunks, ref TEach each)
        where TEach : IEach<T1, T2, T3, T4, T5, T6 >
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
    {
        var entities    = chunks.Entities;
        var start       = entities.Start;
        var length      = entities.Length;
        var span1       = new Span<T1>(chunks.Chunk1.ArchetypeComponents, start, length);
        var span2       = new Span<T2>(chunks.Chunk2.ArchetypeComponents, start, length);
        var span3       = new Span<T3>(chunks.Chunk3.ArchetypeComponents, start, length);
        var span4       = new Span<T4>(chunks.Chunk4.ArchetypeComponents, start, length);
        var span5       = new Span<T5>(chunks.Chunk5.ArchetypeComponents, start, length);
        var span6       = new Span<T6>(chunks.Chunk6.ArchetypeComponents, start, length);
        
        unsafe {
#pragma warning disable CS8500
            fixed (T1*  c1  = span1)
            fixed (T2*  c2  = span2)
            fixed (T3*  c3  = span3)
            fixed (T4*  c4  = span4)
            fixed (T5*  c5  = span5)
            fixed (T6*  c6  = span6)
#pragma warning restore CS8500
            {
                for (int i = 0; i < length; i++) {
                    // all spanX.Length == length. spanX[i] is always in bounds.
                    each.Execute(ref c1[i], ref c2[i], ref c3[i], ref c4[i], ref c5[i], ref c6[i]);
                }
            }
        }
    }
    
    public static void EachEntity<TEachEntity, T1,T2,T3,T4,T5,T6>(this Chunks<T1,T2,T3,T4,T5,T6> chunks, ref TEachEntity each)
        where TEachEntity : IEachEntity<T1, T2, T3, T4, T5, T6>
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
    {
        var entities    = chunks.Entities;
        var start       = entities.Start;
        var length      = entities.Length;
        var spanIds     = entities.Archetype.EntityIds.Slice             (start, length);
        var span1       = new Span<T1>(chunks.Chunk1.ArchetypeComponents, start, length);
        var span2       = new Span<T2>(chunks.Chunk2.ArchetypeComponents, start, length);
        var span3       = new Span<T3>(chunks.Chunk3.ArchetypeComponents, start, length);
        var span4       = new Span<T4>(chunks.Chunk4.ArchetypeComponents, start, length);
        var span5       = new Span<T5>(chunks.Chunk5.ArchetypeComponents, start, length);
        var span6       = new Span<T6>(chunks.Chunk6.ArchetypeComponents, start, length);
        
        unsafe {
#pragma warning disable CS8500
            fixed (T1*  c1  = span1)
            fixed (T2*  c2  = span2)
            fixed (T3*  c3  = span3)
            fixed (T4*  c4  = span4)
            fixed (T5*  c5  = span5)
            fixed (T6*  c6  = span6)
#pragma warning restore CS8500
            fixed (int* ids = spanIds)
            {
                for (int i = 0; i < length; i++) {
                    // all spanX.Length == length. spanX[i] is always in bounds.  
                    each.Execute(ref c1[i], ref c2[i], ref c3[i], ref c4[i], ref c5[i], ref c6[i], ids[i]);
                }
            }
        }
    }
}