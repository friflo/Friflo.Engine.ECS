// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class HasEntityIndex : ComponentIndex<Entity>
{
    private readonly    Dictionary<int, IdArray>    map;
    private readonly    IdArrayHeap                 arrayHeap;
    
#region general
    internal HasEntityIndex() {
        map         = new Dictionary<int, IdArray>();
        arrayHeap   = new IdArrayHeap();
    }
    #endregion
    
    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexedComponentUtils<TComponent,Entity>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<Entity>)component).GetIndexedValue();    // boxes component
    IndexUtils.AddComponentValue(id, value.Id, map, arrayHeap);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap heap)
    {
        var oldValue = IndexedComponentUtils<TComponent,Entity>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        var value    = IndexedComponentUtils<TComponent,Entity>.GetIndexedValue(component);
        if (oldValue.Id == value.Id) {
            return;
        }
        IndexUtils.RemoveComponentValue(id, oldValue.Id, map, arrayHeap);
        IndexUtils.AddComponentValue   (id, value.   Id, map, arrayHeap);
    }
    
    internal override void Remove<TComponent>(int id, StructHeap heap)
    {
        var value = IndexedComponentUtils<TComponent,Entity>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        IndexUtils.RemoveComponentValue(id, value.Id, map, arrayHeap);
    }
    #endregion
    
#region get matches
    internal override Entities GetMatchingEntities(Entity value)
    {
        map.TryGetValue(value.Id, out var ids);
        return arrayHeap.GetEntities(store, ids);
    }
    #endregion
}