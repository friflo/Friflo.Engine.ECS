// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


public static partial class QueryExtension
{
    public static void For<TEach, T1,T2,T3>(this ArchetypeQuery<T1,T2,T3> query, TEach each)
        where TEach : IEach<T1, T2, T3>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
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
            
            unsafe {
#pragma warning disable CS8500
                fixed (T1*  c1  = span1)
                fixed (T2*  c2  = span2)
                fixed (T3*  c3  = span3)
#pragma warning restore CS8500
                {
                    for (int i = 0; i < length; i++) {
                        each.Execute(ref c1[i], ref c2[i], ref c3[i]);
                    }
                }
            }
        }
    }
}