// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class QueryExtensions
{
    public static TEach Each<TEach, T1,T2,T3,T4>(this ArchetypeQuery<T1,T2,T3,T4> query, TEach each)
        where TEach : IEach<T1, T2, T3, T4>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        foreach (var chunk in query.Chunks) {
            chunk.Each(ref each);
        }
        return each;
    }
    
    public static TEachEntity EachEntity<TEachEntity, T1,T2,T3,T4>(this ArchetypeQuery<T1,T2,T3,T4> query, TEachEntity each)
        where TEachEntity : IEachEntity<T1, T2, T3, T4>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        foreach (var chunk in query.Chunks) {
            chunk.EachEntity(ref each);
        }
        return each;
    }
}