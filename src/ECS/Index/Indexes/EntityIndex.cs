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
    /// map: indexed / linked entity (id)  ->  entities (ids) containing <see cref="ILinkComponent"/> referencing the indexed / linked entity.
    internal readonly   Dictionary<int, IdArray>    entityMap   = new();
    
    private             EntityIndexValues           keyCollection;
    #endregion
    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexedValueUtils<TComponent,Entity>.GetIndexedValue(component).Id;
    //  var value = ((IIndexedComponent<Entity>)component).GetIndexedValue();    // boxes component
        DictionaryUtils.AddComponentValue    (id, value, entityMap, this);
        store.nodes[value].isLinked |= indexBit;
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap<TComponent> heap)
    {
        var oldValue = IndexedValueUtils<TComponent,Entity>.GetIndexedValue(heap.componentStash).Id;
        var value    = IndexedValueUtils<TComponent,Entity>.GetIndexedValue(component).Id;
        if (oldValue == value) {
            return;
        }
        var map = entityMap;
        DictionaryUtils.RemoveComponentValue (id, oldValue, map, this);
        DictionaryUtils.AddComponentValue    (id, value,    map, this);
        var nodes = store.nodes;
        nodes[oldValue].isLinked &= ~indexBit;
        nodes[value]   .isLinked |=  indexBit;
    }
    
    internal override void Remove<TComponent>(int id, StructHeap<TComponent> heap)
    {
        var value = IndexedValueUtils<TComponent,Entity>.GetIndexedValue(heap.componentStash).Id;
        DictionaryUtils.RemoveComponentValue (id, value, entityMap, this);
        store.nodes[value].isLinked &= ~indexBit;
    }
    
    internal void RemoveLinkComponents(int id)
    {
        entityMap.TryGetValue(id, out var idArray);
        var linkingEntityIds  = idArray.GetIdSpan(idHeap);
        foreach (var linkingEntityId in linkingEntityIds)
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
    internal override void RemoveEntityIndex(int id, Archetype archetype, int compIndex)
    {
        var map             = entityMap;
        var heap            = idHeap;
        var components      = ((StructHeap<TIndexedComponent>)archetype.heapMap[componentType.StructIndex]).components;
        var linkedEntity    = components[compIndex].GetIndexedValue().Id;
        map.TryGetValue(linkedEntity, out var idArray);
        var idSpan  = idArray.GetIdSpan(heap);
        var index   = idSpan.IndexOf(id);
        idArray.RemoveAt(index, heap);
        if (idArray.Count == 0) {
            map.Remove(linkedEntity);
        } else {
            map[linkedEntity] = idArray;
        }
        store.nodes[id].references &= ~indexBit;
    }
}