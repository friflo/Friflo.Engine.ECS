// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable InlineTemporaryVariable
// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[DebuggerTypeProxy(typeof(EntityListDebugView))]
public sealed class EntityList : IEnumerable<Entity>
{
#region properties
    public              int         Count       => count;
    public ReadOnlySpan<int>        Ids         => new (ids, 0, count);
    public override     string      ToString()  => $"Count: {count}";
    #endregion
    
#region fields
    internal            int[]       ids;            //  8
    internal readonly   EntityStore entityStore;    //  8
    internal            int         count;          //  4
    #endregion
    
    public EntityList(EntityStore store)
    {
        entityStore = store;
        ids         = new int[8];
    }
#region add entities
    
    public void Clear() {
        count = 0;
    }
    
    public void AddEntity(int entityId) {
        if (ids.Length == count) {
            ArrayUtils.Resize(ref ids, 2 * count);
        }
        ids[count++] = entityId;
    }
    
    public void AddEntityTree(Entity entity)
    {
        AddEntity(entity.Id);
        ref var node = ref entity.store.nodes[entity.Id];
        foreach (var child in new ChildEntities (entity.store, node.childIds, node.childCount))
        {
            AddEntityTree(child);
        }
    }
    #endregion
    
    public void AddTags(in Tags tags)
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
    
    public void RemoveTags(in Tags tags)
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
    /// Apply the given entity <paramref name="batch"/> to all entities in the list. 
    /// </summary>
    public void ApplyBatch(EntityBatch batch)
    {
        foreach (var entity in this) {
            entity.store.ApplyBatchTo(batch, entity.Id);
        }
    }
    
    public Entity this[int index] => new Entity(entityStore, ids[index]);
    
    public EntityListEnumerator             GetEnumerator() => new EntityListEnumerator (this);

    // --- IEnumerable
    IEnumerator                 IEnumerable.GetEnumerator() => new EntityListEnumerator (this);

    // --- IEnumerable<>
    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => new EntityListEnumerator (this);
}


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
