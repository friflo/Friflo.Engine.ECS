// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class ChunkExtensions
{
    public static void Each<TEach, T1,T2,T3,T4>(this Chunks<T1,T2,T3,T4> chunks, ref TEach each)
        where TEach : IEach<T1, T2, T3, T4>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        var entities    = chunks.Entities;
        var start       = entities.Start;
        var length      = entities.Length;
        var span1       = new Span<T1>(chunks.Chunk1.ArchetypeComponents, start, length);
        var span2       = new Span<T2>(chunks.Chunk2.ArchetypeComponents, start, length);
        var span3       = new Span<T3>(chunks.Chunk3.ArchetypeComponents, start, length);
        var span4       = new Span<T4>(chunks.Chunk4.ArchetypeComponents, start, length);
        
        unsafe {
#pragma warning disable CS8500
            fixed (T1*  c1  = span1)
            fixed (T2*  c2  = span2)
            fixed (T3*  c3  = span3)
            fixed (T4*  c4  = span4)
#pragma warning restore CS8500
            {
                for (int i = 0; i < length; i++) {
                    // all spanX.Length == length. spanX[i] is always in bounds.
                    each.Execute(ref c1[i], ref c2[i], ref c3[i], ref c4[i]);
                }
            }
        }
    }
    
    public static void EachEntity<TEachEntity, T1,T2,T3,T4>(this Chunks<T1,T2,T3,T4> chunks, ref TEachEntity each)
        where TEachEntity : IEachEntity<T1, T2, T3, T4>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        var entities    = chunks.Entities;
        var start       = entities.Start;
        var length      = entities.Length;
        var spanIds     = entities.Archetype.EntityIds.Slice             (start, length);
        var span1       = new Span<T1>(chunks.Chunk1.ArchetypeComponents, start, length);
        var span2       = new Span<T2>(chunks.Chunk2.ArchetypeComponents, start, length);
        var span3       = new Span<T3>(chunks.Chunk3.ArchetypeComponents, start, length);
        var span4       = new Span<T4>(chunks.Chunk4.ArchetypeComponents, start, length);
        
        unsafe {
#pragma warning disable CS8500
            fixed (T1*  c1  = span1)
            fixed (T2*  c2  = span2)
            fixed (T3*  c3  = span3)
            fixed (T4*  c4  = span4)
#pragma warning restore CS8500
            fixed (int* ids = spanIds)
            {
                for (int i = 0; i < length; i++) {
                    // all spanX.Length == length. spanX[i] is always in bounds.
                    each.Execute(ref c1[i], ref c2[i], ref c3[i], ref c4[i], ids[i]);
                }
            }
        }
    }
}