// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class QueryExtensions
{
    public static void For<TEach, T1>(this ArchetypeQuery<T1> query, TEach each)
        where TEach : IEach<T1>
        where T1 : struct, IComponent
    {
        using var e = query.Chunks.GetEnumerator();
        while (e.MoveNext())
        {
            var cur         = e.Current;
            var entities    = cur.Entities;
            var length      = entities.Length;
            var span1       = cur.Chunk1.Span;
            
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
    }
    
    public static void ForEntity<TEachEntity, T1>(this ArchetypeQuery<T1> query, TEachEntity each)
        where TEachEntity : IEachEntity<T1>
        where T1 : struct, IComponent
    {
        using var e = query.Chunks.GetEnumerator();
        while (e.MoveNext())
        {
            var cur         = e.Current;
            var entities    = cur.Entities;
            var length      = entities.Length;
            var spanIds     = entities.Ids;
            var span1       = cur.Chunk1.Span;
            
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
}