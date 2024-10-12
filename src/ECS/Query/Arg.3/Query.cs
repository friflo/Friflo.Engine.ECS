﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Provide the state of an <paramref name="entity"/> within <see cref="ArchetypeQuery{T1,T2,T3}.ForEachEntity"/>.
/// </summary>
public delegate void ForEachEntity<T1, T2, T3>(ref T1 component1, ref T2 component2, ref T3 component3, Entity entity)
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent;

/// <summary>
/// Provide the state of an entity within <see cref="ArchetypeQuery{T1,T2,T3}.ForEachComponents"/>.
/// </summary>
public delegate void ForEachComponents<T1, T2, T3>(ref T1 component1, ref T2 component2, ref T3 component3)
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent;


/// <summary>
/// A query instance use to retrieve the given component types.
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/general#query-entities">Example.</a>
/// </summary>
public sealed class ArchetypeQuery<T1, T2, T3> : ArchetypeQuery
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
{
    /// <inheritdoc  cref="ArchetypeQuery.AllTags"/>
    public new ArchetypeQuery<T1, T2, T3> AllTags       (in Tags tags) { SetHasAllTags(tags);       return this; }
    /// <inheritdoc  cref="ArchetypeQuery.AnyTags"/>
    public new ArchetypeQuery<T1, T2, T3> AnyTags       (in Tags tags) { SetHasAnyTags(tags);       return this; }
    /// <inheritdoc  cref="ArchetypeQuery.WithDisabled"/>
    public new ArchetypeQuery<T1, T2, T3> WithDisabled  ()             { SetWithDisabled();         return this; }
    /// <inheritdoc  cref="ArchetypeQuery.WithoutAllTags"/>
    public new ArchetypeQuery<T1, T2, T3> WithoutAllTags(in Tags tags) { SetWithoutAllTags(tags);   return this; }
    /// <inheritdoc  cref="ArchetypeQuery.WithoutAnyTags"/>
    public new ArchetypeQuery<T1, T2, T3> WithoutAnyTags(in Tags tags) { SetWithoutAnyTags(tags);   return this; }
    
    /// <inheritdoc  cref="ArchetypeQuery.AllComponents"/>
    public new ArchetypeQuery<T1, T2, T3> AllComponents       (in ComponentTypes componentTypes) { SetHasAllComponents(componentTypes);       return this; }
    /// <inheritdoc  cref="ArchetypeQuery.AnyComponents"/>
    public new ArchetypeQuery<T1, T2, T3> AnyComponents       (in ComponentTypes componentTypes) { SetHasAnyComponents(componentTypes);       return this; }
    /// <inheritdoc  cref="ArchetypeQuery.WithoutAllComponents"/>
    public new ArchetypeQuery<T1, T2, T3> WithoutAllComponents(in ComponentTypes componentTypes) { SetWithoutAllComponents(componentTypes);   return this; }
    /// <inheritdoc  cref="ArchetypeQuery.WithoutAnyComponents"/>
    public new ArchetypeQuery<T1, T2, T3> WithoutAnyComponents(in ComponentTypes componentTypes) { SetWithoutAnyComponents(componentTypes);   return this; }
    
    /// <inheritdoc cref="QueryFilter.HasValue{TComponent,TValue}"/>
    public new ArchetypeQuery<T1, T2, T3> HasValue    <TComponent,TValue>(TValue value)           where TComponent : struct, IIndexedComponent<TValue>
    { base.HasValue    <TComponent, TValue>(value);    return this; }
    
    /// <inheritdoc cref="QueryFilter.ValueInRange{TComponent,TValue}"/>
    public new ArchetypeQuery<T1, T2, T3> ValueInRange<TComponent,TValue>(TValue min, TValue max) where TComponent : struct, IIndexedComponent<TValue> where TValue : IComparable<TValue>
    { base.ValueInRange<TComponent, TValue>(min, max); return this; }
    
    /// <inheritdoc  cref="ArchetypeQuery.FreezeFilter"/>
    public new ArchetypeQuery<T1, T2, T3> FreezeFilter() { SetFreezeFilter();   return this; }
    
    internal ArchetypeQuery(EntityStoreBase store, in Signature<T1, T2, T3> signature, QueryFilter filter)
        : base(store, signature.signatureIndexes, filter) {
    }
    
    /// <summary>
    /// Return the <see cref="Chunk{T}"/>'s storing the components and entities of an <see cref="ArchetypeQuery{T1,T2,T3}"/>.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#enumerate-query-chunks">Example.</a>
    /// </summary>
    public      QueryChunks    <T1, T2, T3>  Chunks         => new (this);
    
    /// <summary>
    /// Returns a <see cref="QueryJob"/> that enables <see cref="JobExecution.Parallel"/> query execution.  
    /// </summary>
    public QueryJob<T1, T2, T3> ForEach(Action<Chunk<T1>, Chunk<T2>, Chunk<T3>, ChunkEntities> action)  => new (this, action);
    
    /// <summary>
    /// Executes the given <paramref name="lambda"/> for each entity in the query result.
    /// </summary>
    public void ForEachEntity(ForEachEntity<T1, T2, T3> lambda)
    {
        var store = Store;
        foreach (var (chunk1, chunk2, chunk3, entities) in Chunks)
        {
            var span1   = chunk1.Span;
            var span2   = chunk2.Span;
            var span3   = chunk3.Span;
            var ids     = entities.Ids;
            for (int n = 0; n < chunk1.Length; n++) {
                lambda(ref span1[n], ref span2[n], ref span3[n], new Entity(store, ids[n]));    
            }
        }
    }
    /// <summary>
    /// Executes the given <paramref name="lambda"/> for each entity in the query result.
    /// </summary>
    public void ForEachComponents(ForEachComponents<T1, T2, T3> lambda)
    {
        foreach (var (chunk1, chunk2, chunk3, _) in Chunks)
        {
            var span1   = chunk1.Span;
            var span2   = chunk2.Span;
            var span3   = chunk3.Span;
            for (int n = 0; n < chunk1.Length; n++) {
                lambda(ref span1[n], ref span2[n], ref span3[n]);
            }
        }
    }
}