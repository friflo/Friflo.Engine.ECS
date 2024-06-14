// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Provide the state of an <paramref name="entity"/> within <see cref="ArchetypeQuery{T1,T2}.ForEachEntity"/>.
/// </summary>
public delegate void ForEachEntity<T1, T2>(ref T1 component1, ref T2 component2, Entity entity)
    where T1 : struct, IComponent
    where T2 : struct, IComponent;


/// <summary>
/// A query instance use to retrieve the given component types.
/// See <a href="https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-General#query-entities">Example.</a>
/// </summary>
public sealed class ArchetypeQuery<T1, T2> : ArchetypeQuery // : IEnumerable <>  // <- not implemented to avoid boxing
    where T1 : struct, IComponent
    where T2 : struct, IComponent
{
    [Browse(Never)] internal    T1[]    copyT1;
    [Browse(Never)] internal    T2[]    copyT2;
    
    /// <inheritdoc cref="ArchetypeQuery.AllTags"/>
    public new ArchetypeQuery<T1, T2> AllTags       (in Tags tags) { SetHasAllTags(tags);      return this; }
    /// <inheritdoc cref="ArchetypeQuery.AnyTags"/>
    public new ArchetypeQuery<T1, T2> AnyTags       (in Tags tags) { SetHasAnyTags(tags);      return this; }
    /// <inheritdoc cref="ArchetypeQuery.WithDisabled"/>
    public new ArchetypeQuery<T1, T2> WithDisabled  ()             { SetWithDisabled();        return this; }
    /// <inheritdoc cref="ArchetypeQuery.WithoutAllTags"/>
    public new ArchetypeQuery<T1, T2> WithoutAllTags(in Tags tags) { SetWithoutAllTags(tags);  return this; }
    /// <inheritdoc cref="ArchetypeQuery.WithoutAnyTags"/>
    public new ArchetypeQuery<T1, T2> WithoutAnyTags(in Tags tags) { SetWithoutAnyTags(tags);  return this; }
    
    /// <inheritdoc cref="ArchetypeQuery.AllComponents"/>
    public new ArchetypeQuery<T1, T2> AllComponents       (in ComponentTypes componentTypes) { SetHasAllComponents(componentTypes);       return this; }
    /// <inheritdoc cref="ArchetypeQuery.AnyComponents"/>
    public new ArchetypeQuery<T1, T2> AnyComponents       (in ComponentTypes componentTypes) { SetHasAnyComponents(componentTypes);       return this; }
    /// <inheritdoc cref="ArchetypeQuery.WithoutAllComponents"/>
    public new ArchetypeQuery<T1, T2> WithoutAllComponents(in ComponentTypes componentTypes) { SetWithoutAllComponents(componentTypes);   return this; }
    /// <inheritdoc cref="ArchetypeQuery.WithoutAnyComponents"/>
    public new ArchetypeQuery<T1, T2> WithoutAnyComponents(in ComponentTypes componentTypes) { SetWithoutAnyComponents(componentTypes);   return this; }
    
    
    internal new ArchetypeQuery<T1, T2> Has<TComponent,TValue>(TValue value) where TComponent : struct, IIndexedComponent<TValue> { Filter.Has<TComponent, TValue>(value);  return this; }
    
    /// <inheritdoc cref="ArchetypeQuery.FreezeFilter"/>
    public new ArchetypeQuery<T1, T2> FreezeFilter() { SetFreezeFilter();   return this; }
    
    internal ArchetypeQuery(EntityStoreBase store, in Signature<T1, T2> signature, QueryFilter filter)
        : base(store, signature.signatureIndexes, filter) {
    }
    
    public ArchetypeQuery<T1, T2> ReadOnly<T>()
        where T : struct, IComponent
    {
        if (typeof(T1) == typeof(T)) { copyT1 = new T1[ChunkSize]; return this; }
        if (typeof(T2) == typeof(T)) { copyT2 = new T2[ChunkSize]; return this; }
        throw ReadOnlyException(typeof(T));
    }
    
    /// <summary>
    /// Return the <see cref="Chunk{T}"/>'s storing the components and entities of an <see cref="ArchetypeQuery{T1,T2}"/>.<br/>
    /// See <a href="https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Optimization#enumerate-query-chunks">Example.</a>
    /// </summary> 
    public      QueryChunks    <T1,T2>  Chunks                                      => new (this);
    
    /// <summary>
    /// Returns a <see cref="QueryJob"/> that enables <see cref="JobExecution.Parallel"/> query execution.  
    /// </summary>
    public QueryJob<T1, T2> ForEach(Action<Chunk<T1>, Chunk<T2>, ChunkEntities> action)  => new (this, action);
    
    /// <summary>
    /// Executes the given <paramref name="lambda"/> for each entity in the query result.
    /// </summary>
    public void ForEachEntity(ForEachEntity<T1, T2> lambda)
    {
        var store = Store;
        foreach (var (chunk1, chunk2, entities) in Chunks)
        {
            var span1   = chunk1.Span;
            var span2   = chunk2.Span;
            var ids     = entities.Ids;
            for (int n = 0; n < chunk1.Length; n++) {
                lambda(ref span1[n], ref span2[n], new Entity(store, ids[n]));    
            }
        }
    }
}