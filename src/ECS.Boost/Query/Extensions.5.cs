// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class QueryExtensions
{
    public static void For<TEach, T1,T2,T3,T4,T5>(this ArchetypeQuery<T1,T2,T3,T4,T5> query, TEach each)
        where TEach : IEach<T1, T2, T3, T4, T5>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
    {
        using var e = query.Chunks.GetEnumerator();
        while (e.MoveNext())
        {
            var cur         = e.Current;
            var entities    = cur.Entities;
            var length      = entities.Length;
            var span1       = cur.Chunk1.Span;
            var span2       = cur.Chunk2.Span;
            var span3       = cur.Chunk3.Span;
            var span4       = cur.Chunk4.Span;
            var span5       = cur.Chunk5.Span;
            
            unsafe {
#pragma warning disable CS8500
                fixed (T1*  c1  = span1)
                fixed (T2*  c2  = span2)
                fixed (T3*  c3  = span3)
                fixed (T4*  c4  = span4)
                fixed (T5*  c5  = span5)
#pragma warning restore CS8500
                {
                    for (int i = 0; i < length; i++) {
                        each.Execute(ref c1[i], ref c2[i], ref c3[i], ref c4[i], ref c5[i]);
                    }
                }
            }
        }
    }
}