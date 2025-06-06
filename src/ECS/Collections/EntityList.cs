﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable UseCollectionExpression
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable InlineTemporaryVariable
// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// A list of entities of a specific <see cref="EntityStore"/> used to apply changes to all entities in the container.<br/>
/// It's recommended to reuse instances of this class to avoid unnecessary allocations.<br/>
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/batch">Example.</a>
/// </summary>
[DebuggerTypeProxy(typeof(EntityListDebugView))]
public sealed class EntityList : IList<Entity>, IReadOnlyList<Entity>
{
#region properties
    /// <summary> Returns the number of entities stored in the container. </summary>
    public              int         Count       => count;
    
    public              int         Capacity    { get => ids.Length; set => SetCapacity(value); }

    /// <summary> Returns the store to which the list entities belong to. </summary>
    public              EntityStore EntityStore => entityStore;
    
    /// <summary> Return the ids of entities stored in the container. </summary>
    public ReadOnlySpan<RawEntity>  Ids         => new (ids, 0, count);
    
    public override     string      ToString()  => $"Count: {count}";
    #endregion
    
#region fields
    [Browse(Never)] internal    RawEntity[] ids;            //  8
    [Browse(Never)] internal    EntityStore entityStore;    //  8
    [Browse(Never)] internal    int         count;          //  4
    #endregion
    
#region general
    /// <summary>
    /// Creates a container for entities returned by a query to perform structural changes.<br/>
    /// This constructor is intended for use in <see cref="QueryEntities.ToEntityList()"/>.
    /// </summary>
    public EntityList()
    {
        ids         = Array.Empty<RawEntity>();
    }

    /// <summary>
    /// Creates a container to store entities of the given <paramref name="store"/>.
    /// </summary>
    public EntityList(EntityStore store)
    {
        entityStore = store;
        ids         = Array.Empty<RawEntity>();
    }
    
    /// <summary>
    /// Set the <paramref name="store"/> to which the list entities belong to.<br/>
    /// EntityList must be empty when setting <see cref="EntityStore"/>.
    /// </summary>
    public void SetStore(EntityStore store)
    {
        if (count > 0) throw new ArgumentException("EntityList must be empty when calling SetStore()");
        entityStore = store;
    }
    
    private void SetCapacity(int capacity)
    {
        if (capacity <= ids.Length) {
            return;
        }
        var newIds = new RawEntity[capacity];
        var source = new ReadOnlySpan<RawEntity>  (ids,    0, count) ;
        var target = new Span<RawEntity>          (newIds, 0, count) ;
        source.CopyTo(target);
        ids = newIds;
    }
    #endregion
    
#region add entities
    /// <summary> Removes all entities from the <see cref="EntityList"/>. </summary>
    public void Clear()
    {
        count = 0;
    }
    
    /// <summary>
    /// Adds the given <paramref name="entity"/> to the end of the <see cref="EntityList"/>.
    /// </summary>
    public void Add(Entity entity)
    {
        if (entity.store != entityStore) throw EntityStoreBase.InvalidStoreException(nameof(entity));
        if (ids.Length == count) {
            ResizeIds();
        }
        ids[count++] = entity.RawEntity;
    }
    
    /// <summary>
    /// Adds the entity with the given <paramref name="id"/> to the end of the <see cref="EntityList"/>.
    /// </summary>
    public void Add(int id)
    {
        var store = entityStore;
        if (id < 0 || id >= store.nodes.Length) {
            throw EntityStoreBase.IdOutOfRangeException(store, id);
        }
        store.GetEntityById(1);
        if (ids.Length == count) {
            ResizeIds();
        }
        ids[count++]   = new RawEntity(id, store.nodes[id].revision);
    }
    
    internal void AddInternal(RawEntity id)
    {
        if (ids.Length == count) {
            ResizeIds();
        }
        ids[count++] = id;
    }
    
    /// <summary>
    /// Adds the <paramref name="entity"/> and recursively all child entities of the given <paramref name="entity"/>
    /// to the end of the <see cref="EntityList"/>.
    /// </summary>
    public void AddTree(Entity entity)
    {
        if (entity.store != entityStore) throw EntityStoreBase.InvalidStoreException(nameof(entity));
        AddEntityTree(entity);
    }
    
    private void AddEntityTree(Entity entity)
    {
        AddInternal(entity.RawEntity);
        var store = entityStore;
        foreach (var id in EntityStore.GetChildIds(entity)) {
            var child = new Entity(store, id);
            AddEntityTree(child);
        }
    }
    
    private void ResizeIds() {
        ArrayUtils.Resize(ref ids, Math.Max(8, 2 * count));
    }
    #endregion
    
#region apply entity changes
    /// <summary>
    /// Adds the given <paramref name="tags"/> to all entities in the <see cref="EntityList"/>.
    /// </summary>
    public void ApplyAddTags(in Tags tags)
    {
        int index = 0;
        var store = entityStore;
        foreach (var id in Ids)
        {
            // don't capture store.nodes. Application event handler may resize
            ref var node = ref store.nodes[id.Id]; 
            EntityStoreBase.AddTags(store, tags, id.Id, ref node.archetype, ref node.compIndex, ref index);
        }
    }
    
    /// <summary>
    /// Removes the given <paramref name="tags"/> from all entities in the <see cref="EntityList"/>.
    /// </summary>
    public void ApplyRemoveTags(in Tags tags)
    {
        int index = 0;
        var store = entityStore;
        foreach (var id in Ids)
        {
            // don't capture store.nodes. Application event handler may resize
            ref var node = ref store.nodes[id.Id];
            EntityStoreBase.RemoveTags(store, tags, id.Id, ref node.archetype, ref node.compIndex, ref index);
        }
    }
    
    /// <summary>
    /// Apply the given <paramref name="batch"/> to all entities in the <see cref="EntityList"/>. 
    /// </summary>
    public void ApplyBatch(EntityBatch batch)
    {
        var store = entityStore;
        foreach (var id in Ids) {
            store.ApplyBatchTo(batch, id.Id);
        }
    }
    #endregion
    
#region sort / filter
    /// <summary>
    /// Sort the entities by the component field/property with the given <paramref name="memberName"/>.<br/> 
    /// </summary>
    /// <returns>An array containing all entity id and their field/property value.</returns>
    public ComponentField<TField>[] SortByComponentField<TComponent,TField>(string memberName, SortOrder sortOrder, ComponentField<TField>[] fields = null)
        where TComponent    : struct, IComponent
    {
        return ComponentField<TField>.Sort<TComponent>(this, memberName, sortOrder, fields);
    }
    
    private class IdComparerAsc : IComparer<RawEntity> {
        public int Compare(RawEntity e1, RawEntity e2) => e1.Id - e2.Id;
    }
    
    private class IdComparerDesc : IComparer<RawEntity> {
        public int Compare(RawEntity e1, RawEntity e2) => e2.Id - e1.Id;
    }
    private static readonly IdComparerAsc  IdAsc = new ();
    private static readonly IdComparerDesc IdDesc = new ();
    
#if NET5_0_OR_GREATER
    private static readonly Comparison<RawEntity> IsAscComparison  = (e1, e2) => e1.Id - e2.Id;
    private static readonly Comparison<RawEntity> IsDescComparison = (e1, e2) => e2.Id - e1.Id;
#endif
    /// <summary>
    /// Sort the entities by entity Id.<br/> 
    /// </summary>
    public void SortByEntityId(SortOrder sortOrder)
    {
        if (sortOrder == SortOrder.None) {
            return;
        }
#if NET5_0_OR_GREATER
        var span = new Span<RawEntity>(ids, 0, count) ;
        span.Sort(sortOrder == SortOrder.Ascending ? IsAscComparison : IsDescComparison);
#else
        Array.Sort(ids, 0, count, sortOrder == SortOrder.Ascending ? IdAsc : IdDesc);
#endif
    }
    
    /// <summary>
    /// Removes all entities not matching the passed filter
    /// </summary>
    public void Filter(Func<Entity, bool> filter)
    {
        var length      = count;
        var store       = entityStore;
        var idsLocal    = ids;
        var index       = 0;
        RawEntity rawEntity = default;
        try {
            for (int n = 0; n < length; n++)
            {
                rawEntity = idsLocal[n];
                var entity = new Entity(store, rawEntity.Id, rawEntity.Revision);
                if (filter(entity)) {
                    idsLocal[index++] = rawEntity;
                }
            }
        }
        catch (Exception e) {
            throw new FilterException($"at entity {rawEntity.Id} - {e.GetType().Name}: {e.Message}", e);
        }
        count = index;
    }
    #endregion
    
#region IList<>
    /// <summary> Gets a value indicating whether the <see cref="ICollection"/> is read-only. </summary>
    public bool IsReadOnly => false;

    /// <summary> Return the entity at the given <paramref name="index"/>.</summary>
    public Entity this[int index]
    {
        get => (index >= 0 && index < count) ? new Entity(entityStore, ids[index]) : throw new IndexOutOfRangeException();
        set => ids[index] = new RawEntity(value.Id, entityStore.nodes[value.Id].revision);
    }

    public bool Remove  (Entity item)
    {
        var index = IndexOf(item);
        if (index < 0) {
            return false;
        }
        RemoveAt(index);
        return true;
    }

    public int  IndexOf (Entity item) {
        return Array.IndexOf(ids, item.RawEntity, 0, count);
    }
    
    public bool Contains(Entity item) {
        return Array.IndexOf(ids, item.RawEntity, 0, count) >= 0;
    }

    public void Insert  (int index, Entity item) {
        if (index < 0 || index >= count) throw new IndexOutOfRangeException();
        if (ids.Length == count) {
            ResizeIds();
        }
        var len = count++ - index;
        Array.Copy(ids, index, ids, index + 1, len);
        ids[index] = item.RawEntity;
    }

    public void RemoveAt(int index) {
        if (index < 0 || index >= count) throw new IndexOutOfRangeException();
        var len = --count - index;
        Array.Copy(ids, index + 1, ids, index, len);
    }
    
    /// <summary>
    /// Copies the entities of the <see cref="EntityList"/> to an <see cref="Entity"/>[], starting at the given <paramref name="index"/>
    /// </summary>
    public void CopyTo(Entity[] array, int index)
    {
        var store = entityStore;
        for (int n = 0; n < count; n++) {
            array[index++] = new Entity(store, ids[n]);
        }
    }
    #endregion
    
#region IEnumerator

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="EntityList"/>. 
    /// </summary>
    public EntityListEnumerator             GetEnumerator() => new EntityListEnumerator (this);

    // --- IEnumerable
    IEnumerator                 IEnumerable.GetEnumerator() => new EntityListEnumerator (this);

    // --- IEnumerable<>
    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => new EntityListEnumerator (this);
    #endregion
}

/// <summary>
/// Enumerates the entities of an <see cref="EntityList"/>.
/// </summary>
public struct EntityListEnumerator : IEnumerator<Entity>
{
    private readonly    RawEntity[] ids;        //  8
    private readonly    EntityStore store;      //  8
    private readonly    int         count;      //  4
    private             int         index;      //  4
    private             Entity      current;    // 16
    
    internal EntityListEnumerator(EntityList list) {
        ids     = list.ids;
        count   = list.count;
        store   = list.entityStore;
    }
    
    // --- IEnumerator
    public          void         Reset()    => index = 0;

    readonly object  IEnumerator.Current    => current;

    public   Entity              Current    => current;
    
    // --- IEnumerator
    public bool MoveNext()
    {
        if (index < count) {
            current = new Entity(store, ids[index++]);
            return true;
        }
        current = default;
        return false;
    }
    
    public readonly void Dispose() { }
}

internal sealed class EntityListDebugView
{
    [Browse(RootHidden)]
    internal            Entity[]    Entities => GetEntities();
    
    private readonly    EntityList  list;
    
    internal EntityListDebugView(EntityList list) {
        this.list = list;
    }
    
    private Entity[] GetEntities()
    {
        var ids     = list.ids;
        var store   = list.entityStore;
        var count   = list.count;
        var result  = new Entity[count];
        for (int n = 0; n < count; n++) {
            result[n] = new Entity(store, ids[n]);
        }
        return result;
    }
} 

internal class FilterException : Exception
{
    internal FilterException(string message, Exception innerException)
        : base (message, innerException)
    { }
}