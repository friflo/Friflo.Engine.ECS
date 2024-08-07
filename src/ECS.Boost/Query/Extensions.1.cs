﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class QueryExtensions
{
    public static void Each<TEach, T1>(this ArchetypeQuery<T1> query, TEach each)
        where TEach : IEach<T1>
        where T1 : struct, IComponent
    {
        using var e = query.Chunks.GetEnumerator();
        while (e.MoveNext()) {
            e.Current.Each(each);
        }
    }
    
    public static void EachEntity<TEachEntity, T1>(this ArchetypeQuery<T1> query, TEachEntity each)
        where TEachEntity : IEachEntity<T1>
        where T1 : struct, IComponent
    {
        using var e = query.Chunks.GetEnumerator();
        while (e.MoveNext()) {
            e.Current.EachEntity(each);
        }
    }
}