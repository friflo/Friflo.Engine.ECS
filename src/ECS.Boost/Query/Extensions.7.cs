﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class QueryExtensions
{
    public static TEach Each<TEach, T1,T2,T3,T4,T5,T6,T7>(this ArchetypeQuery<T1,T2,T3,T4,T5,T6,T7> query, TEach each)
        where TEach : IEach<T1, T2, T3, T4, T5,T6,T7>
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
        where T7 : struct
    {
        foreach (var chunk in query.Chunks) {
            chunk.Each(ref each);
        }
        return each;
    }
    
    public static TEachEntity EachEntity<TEachEntity, T1,T2,T3,T4,T5,T6,T7>(this ArchetypeQuery<T1,T2,T3,T4,T5,T6,T7> query, TEachEntity each)
        where TEachEntity : IEachEntity<T1, T2, T3, T4, T5, T6,T7>
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
        where T7 : struct
    {
        foreach (var chunk in query.Chunks) {
            chunk.EachEntity(ref each);
        }
        return each;
    }
}