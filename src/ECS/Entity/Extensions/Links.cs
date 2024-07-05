// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using Friflo.Engine.ECS.Collections;
using Friflo.Engine.ECS.Index;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class EntityExtensions
{
    private static readonly List<IncomingLink> LinkBuffer = new ();
    
    public static IncomingLinks GetAllIncomingLinks(this Entity entity)
    {
        var store               = entity.store;
        var id                  = entity.Id;
        var isLinked            = store.nodes[id].isLinked;
        var indexTypes          = new ComponentTypes();
        var relationTypes       = new ComponentTypes();
        var schema              = EntityStoreBase.Static.EntitySchema;

        indexTypes.bitSet.l0    = schema.indexTypes.   bitSet.l0 & isLinked; // intersect
        relationTypes.bitSet.l0 = schema.relationTypes.bitSet.l0 & isLinked; // intersect
        
        LinkBuffer.Clear();
        
        // --- remove link components from entities having the passed entity id as target
        var indexMap = store.extension.indexMap;
        foreach (var componentType in indexTypes)
        {
            var entityIndex = (EntityIndex)indexMap[componentType.StructIndex];
            entityIndex.entityMap.TryGetValue(id, out var idArray);
            var idSpan = idArray.GetIdSpan(entityIndex.idHeap);
            foreach (var linkId in idSpan) {
                var linkEntity  = new Entity(store, linkId);
                var component   = EntityUtils.GetEntityComponent(linkEntity, componentType);
                LinkBuffer.Add(new IncomingLink(linkEntity, component));
            }
        }
        // --- remove link relations from entities having the passed entity id as target
        var relationsMap = store.extension.relationsMap;
        foreach (var componentType in relationTypes) {
            // var relations = relationsMap[componentType.StructIndex];
            // relations.RemoveLinksWithTarget(id);
        }
        var links = LinkBuffer.ToArray();
        return new IncomingLinks(entity, links);
    }

    
}