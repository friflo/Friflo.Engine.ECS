// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#entitybatch---entitylist">Example.</a>
/// </summary>
[DebuggerTypeProxy(typeof(EntityListDebugView))]
public sealed class EntityList : IList<Entity>
{
#region properties
    /// <summary> Returns the number of entities stored in the container. </summary>
    public              int         Count       => count;
    
    public              int         Capacity    { get => ids.Length; set => SetCapacity(value); }

    /// <summary> Returns the store to which the list entities belong to. </summary>
    public              EntityStore EntityStore => entityStore;
    
    /// <summary> Return the ids of entities stored in the container. </summary>
    public ReadOnlySpan<int>        Ids         => new (ids, 0, count);
    
    public override     string      ToString()  => $"Count: {count}";
    #endregion
    
#region fields
    [Browse(Never)] internal    int[]       ids;            //  8
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
        ids         = Array.Empty<int>();
    }

    /// <summary>
    /// Creates a container to store entities of the given <paramref name="store"/>.
    /// </summary>
    public EntityList(EntityStore store)
    {
        entityStore = store;
        ids         = Array.Empty<int>();
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
        var newIds = new int[capacity];
        var source = new ReadOnlySpan<int>  (ids,    0, count) ;
        var target = new Span<int>          (newIds, 0, count) ;
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
        ids[count++] = entity.Id;
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
        ids[count++] = id;
    }
    
    internal void AddInternal(int id)
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
        AddInternal(entity.Id);
        foreach (var id in EntityStore.GetChildIds(entity)) {
            var child = new Entity(entityStore, id);
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
            ref var node = ref store.nodes[id]; 
            EntityStoreBase.AddTags(store, tags, id, ref node.archetype, ref node.compIndex, ref index);
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
            ref var node = ref store.nodes[id];
            EntityStoreBase.RemoveTags(store, tags, id, ref node.archetype, ref node.compIndex, ref index);
        }
    }
    
    /// <summary>
    /// Apply the given <paramref name="batch"/> to all entities in the <see cref="EntityList"/>. 
    /// </summary>
    public void ApplyBatch(EntityBatch batch)
    {
        var store = entityStore;
        foreach (var id in Ids) {
            store.ApplyBatchTo(batch, id);
        }
    }
    #endregion
    
#region IList<>
    /// <summary> Gets a value indicating whether the <see cref="ICollection"/> is read-only. </summary>
    public bool IsReadOnly => false;

    /// <summary> Return the entity at the given <paramref name="index"/>.</summary>
    public Entity this[int index]
    {
        get => new Entity(entityStore, ids[index]);
        set => ids[index] = value.Id;
    }
    /// <summary> not implemented </summary>
    [ExcludeFromCodeCoverage] public bool Remove  (Entity item)             => throw new NotImplementedException();
    /// <summary> not implemented </summary>
    [ExcludeFromCodeCoverage] public int  IndexOf (Entity item)             => throw new NotImplementedException();
    /// <summary> not implemented </summary>
    [ExcludeFromCodeCoverage] public void Insert  (int index, Entity item)  => throw new NotImplementedException();
    /// <summary> not implemented </summary>
    [ExcludeFromCodeCoverage] public void RemoveAt(int index)               => throw new NotImplementedException();
    /// <summary> not implemented </summary>
    [ExcludeFromCodeCoverage] public bool Contains(Entity item)             => throw new NotImplementedException();
    
    /// <summary>
    /// Copies the entities of the <see cref="EntityList"/> to an <see cref="Entity"/>[], starting at the given <paramref name="index"/>
    /// </summary>
    public void CopyTo(Entity[] array, int index)
    {
        for (int n = 0; n < count; n++) {
            array[index++] = new Entity(entityStore, ids[n]);
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
    private readonly    int[]       ids;        //  8
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
