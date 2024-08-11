// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class QueryExtensions
{
    public static TEach Each<TEach, T1,T2>(this ArchetypeQuery<T1,T2> query, TEach each)
        where TEach : IEach<T1, T2>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        foreach (var chunk in query.Chunks) {
            chunk.Each(ref each);
        }
        return each;
    }
    
    public static TEachEntity EachEntity<TEachEntity, T1,T2>(this ArchetypeQuery<T1,T2> query, TEachEntity each)
        where TEachEntity : IEachEntity<T1, T2>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        foreach (var chunk in query.Chunks) {
            chunk.EachEntity(ref each);
        }
        return each;
    }
}