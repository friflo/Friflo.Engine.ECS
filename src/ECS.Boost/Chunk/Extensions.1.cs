// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class ChunkExtensions
{
    public static void Each<TEach, T1>(this Chunks<T1> chunks, TEach each)
        where TEach : IEach<T1>
        where T1 : struct, IComponent
    {
        var entities    = chunks.Entities;
        var start       = entities.Start;
        var length      = entities.Length;
        var span1       = new Span<T1>(chunks.Chunk1.ArchetypeComponents, start, length);
        
        unsafe {
#pragma warning disable CS8500
            fixed (T1*  c1  = span1)
#pragma warning restore CS8500
            {
                for (int i = 0; i < length; i++) {
                    each.Execute(ref c1[i]);
                }
            }
        }
    }
    
    public static void EachEntity<TEachEntity, T1>(this Chunks<T1> chunks, TEachEntity each)
        where TEachEntity : IEachEntity<T1>
        where T1 : struct, IComponent
    {
        var entities    = chunks.Entities;
        var start       = entities.Start;
        var length      = entities.Length;
        var spanIds     = entities.Archetype.EntityIds.Slice             (start, length);
        var span1       = new Span<T1>(chunks.Chunk1.ArchetypeComponents, start, length);
        
        unsafe {
#pragma warning disable CS8500
            fixed (T1*  c1  = span1)
#pragma warning restore CS8500
            fixed (int* ids = spanIds)
            {
                for (int i = 0; i < length; i++) {
                    each.Execute(ref c1[i], ids[i]);
                }
            }
        }
    }
}