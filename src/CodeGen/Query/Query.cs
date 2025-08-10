using static CodeGen.Gen;

namespace CodeGen.Query;

static partial class QueryGen {
    
    public static string Query_generator(int count)
    {
        var args = Join(count, n => $"T{n}", ",");
        
    return $$"""
// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Provide the state of an <paramref name="entity"/> within <see cref="ArchetypeQuery{{{args}}}.ForEachEntity"/>.
/// </summary>
public delegate void ForEachEntity<{{args}}>(ref T1 component1, ref T2 component2, Entity entity)
{{Where(count)}};


/// <summary>
/// A query instance use to retrieve the given component types.
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query">Example.</a>
/// </summary>
public sealed class ArchetypeQuery<{{args}}> : ArchetypeQuery // : IEnumerable <>  // <- not implemented to avoid boxing
{{Where(count)}}
{
    /// <inheritdoc cref="ArchetypeQuery.AllTags"/>
    public new ArchetypeQuery<{{args}}> AllTags       (in Tags tags) { SetHasAllTags(tags);      return this; }
    /// <inheritdoc cref="ArchetypeQuery.AnyTags"/>
    public new ArchetypeQuery<{{args}}> AnyTags       (in Tags tags) { SetHasAnyTags(tags);      return this; }
    /// <inheritdoc cref="ArchetypeQuery.WithDisabled"/>
    public new ArchetypeQuery<{{args}}> WithDisabled  ()             { SetWithDisabled();        return this; }
    /// <inheritdoc cref="ArchetypeQuery.WithoutAllTags"/>
    public new ArchetypeQuery<{{args}}> WithoutAllTags(in Tags tags) { SetWithoutAllTags(tags);  return this; }
    /// <inheritdoc cref="ArchetypeQuery.WithoutAnyTags"/>
    public new ArchetypeQuery<{{args}}> WithoutAnyTags(in Tags tags) { SetWithoutAnyTags(tags);  return this; }
    
    /// <inheritdoc cref="ArchetypeQuery.AllComponents"/>
    public new ArchetypeQuery<{{args}}> AllComponents       (in ComponentTypes componentTypes) { SetHasAllComponents(componentTypes);       return this; }
    /// <inheritdoc cref="ArchetypeQuery.AnyComponents"/>
    public new ArchetypeQuery<{{args}}> AnyComponents       (in ComponentTypes componentTypes) { SetHasAnyComponents(componentTypes);       return this; }
    /// <inheritdoc cref="ArchetypeQuery.WithoutAllComponents"/>
    public new ArchetypeQuery<{{args}}> WithoutAllComponents(in ComponentTypes componentTypes) { SetWithoutAllComponents(componentTypes);   return this; }
    /// <inheritdoc cref="ArchetypeQuery.WithoutAnyComponents"/>
    public new ArchetypeQuery<{{args}}> WithoutAnyComponents(in ComponentTypes componentTypes) { SetWithoutAnyComponents(componentTypes);   return this; }
    
    /// <inheritdoc cref="QueryFilter.HasValue{TComponent,TValue}"/>
    public new ArchetypeQuery<{{args}}> HasValue    <TComponent,TValue>(TValue value)           where TComponent : struct, IIndexedComponent<TValue>
    { base.HasValue    <TComponent, TValue>(value);    return this; }
    
    /// <inheritdoc cref="QueryFilter.ValueInRange{TComponent,TValue}"/>
    public new ArchetypeQuery<{{args}}> ValueInRange<TComponent,TValue>(TValue min, TValue max) where TComponent : struct, IIndexedComponent<TValue> where TValue : IComparable<TValue>
    { base.ValueInRange<TComponent, TValue>(min, max); return this; }
    
    
    /// <inheritdoc cref="ArchetypeQuery.FreezeFilter"/>
    public new ArchetypeQuery<{{args}}> FreezeFilter() { SetFreezeFilter();   return this; }
    
    internal ArchetypeQuery(EntityStoreBase store, in Signature<{{args}}> signature, QueryFilter filter)
        : base(store, signature.signatureIndexes, filter, null) {
    }
    
    /// <summary>
    /// Return the <see cref="Chunk{T}"/>'s storing the components and entities of an <see cref="ArchetypeQuery{{{args}}}"/>.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query-optimization#enumerate-query-chunks">Example.</a>
    /// </summary> 
    public      QueryChunks    <{{args}}>  Chunks                                      => new (this);
    
    /// <summary>
    /// Returns a <see cref="QueryJob"/> that enables <see cref="JobExecution.Parallel"/> query execution.  
    /// </summary>
    public QueryJob<{{args}}> ForEach(Action<Chunk<T1>, Chunk<T2>, ChunkEntities> action)  => new (this, action);
    
    /// <summary>
    /// Executes the given <paramref name="lambda"/> for each entity in the query result.
    /// </summary>
    public void ForEachEntity(ForEachEntity<{{args}}> lambda)
    {
        var localStore = Store;
        var nodes = localStore.nodes;
        foreach (var ({{Join(count, n => $"chunk{n}", ", ")}}, entities) in Chunks)
        {
{{Join(count, n => $"            var span{n}   = chunk{n}.Span;\r\n")}}
            var ids     = entities.Ids;
            for (int n = 0; n < chunk1.Length; n++) {
                var id = ids[n];
                lambda({{Join(count, n => $"ref span{n}[n]", ", ")}}, new Entity(localStore, id, nodes[id].revision));
            }
        }
    }
}
""";
    }
}