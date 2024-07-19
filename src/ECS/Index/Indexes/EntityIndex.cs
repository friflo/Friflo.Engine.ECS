// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Collections;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal abstract class EntityIndex : ComponentIndex<Entity>
{
    internal override   int                         Count       => entityMap.Count;
    
#region fields
    /// map:  indexed / linked entity (id)  ->  entities (ids) containing a <see cref="ILinkComponent"/> referencing the indexed / linked entity.
    internal readonly   Dictionary<int, IdArray>    entityMap   = new();
    
    private             EntityIndexValues           keyCollection;
    #endregion
    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        int target = IndexedValueUtils<TComponent,Entity>.GetIndexedValue(component).Id;
    //  var target = ((IIndexedComponent<Entity>)component).GetIndexedValue();    // boxes component
        EntityIndexUtils.AddComponentValue    (id, target, this);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap<TComponent> heap)
    {
        int oldTarget = IndexedValueUtils<TComponent,Entity>.GetIndexedValue(heap.componentStash).Id;
        int target    = IndexedValueUtils<TComponent,Entity>.GetIndexedValue(component).Id;
        if (oldTarget == target) {
            return;
        }
        EntityIndexUtils.RemoveComponentValue (id, oldTarget, this);
        EntityIndexUtils.AddComponentValue    (id, target,    this);
    }
    
    internal override void Remove<TComponent>(int id, StructHeap<TComponent> heap)
    {
        int target = IndexedValueUtils<TComponent,Entity>.GetIndexedValue(heap.componentStash).Id;
        EntityIndexUtils.RemoveComponentValue (id, target, this);
    }
    
    internal void RemoveLinksWithTarget(int targetId)
    {
        entityMap.TryGetValue(targetId, out var idArray);
        // TODO check if it necessary to make a copy of linkingEntityIds - e.g. by stackalloc 
        var linkingEntityIds  = idArray.GetSpan(idHeap, store);
        foreach (int linkingEntityId in linkingEntityIds)
        {
            var entity = new Entity(store, linkingEntityId);
            EntityUtils.RemoveEntityComponent(entity, componentType);
        }
    }
    #endregion
    
#region get matches
    internal override Entities GetHasValueEntities(Entity value)
    {
        entityMap.TryGetValue(value.Id, out var ids);
        return idHeap.GetEntities(store, ids);
    }
    
    internal override IReadOnlyCollection<Entity> IndexedComponentValues => keyCollection ??= new EntityIndexValues(this);
    #endregion
}

internal sealed class EntityIndex<TIndexedComponent> : EntityIndex
    where TIndexedComponent : struct, IIndexedComponent<Entity>
{
    internal override void RemoveEntityFromIndex(int id, Archetype archetype, int compIndex)
    {
        var map             = entityMap;
        var heap            = idHeap;
        var components      = ((StructHeap<TIndexedComponent>)archetype.heapMap[componentType.StructIndex]).components;
        int linkedEntity    = components[compIndex].GetIndexedValue().Id;
        map.TryGetValue(linkedEntity, out var idArray);
        var idSpan  = idArray.GetSpan(heap, store);
        int index   = idSpan.IndexOf(id);
        idArray.RemoveAt(index, heap);
        if (idArray.Count == 0) {
            map.Remove(linkedEntity);
        } else {
            map[linkedEntity] = idArray;
        }
        store.nodes[id].isOwner &= ~indexBit;
    }
}