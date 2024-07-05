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
        var target              = entity.Id;
        var isLinked            = store.nodes[target].isLinked;
        var indexTypes          = new ComponentTypes();
        var relationTypes       = new ComponentTypes();
        var schema              = EntityStoreBase.Static.EntitySchema;

        indexTypes.bitSet.l0    = schema.indexTypes.   bitSet.l0 & isLinked; // intersect
        relationTypes.bitSet.l0 = schema.relationTypes.bitSet.l0 & isLinked; // intersect
        
        LinkBuffer.Clear();
        
        // --- add all link components
        var indexMap = store.extension.indexMap;
        foreach (var componentType in indexTypes)
        {
            var entityIndex = (EntityIndex)indexMap[componentType.StructIndex];
            entityIndex.entityMap.TryGetValue(target, out var idArray);
            var idSpan = idArray.GetIdSpan(entityIndex.idHeap);
            foreach (var linkId in idSpan) {
                var linkEntity  = new Entity(store, linkId);
                var component   = EntityUtils.GetEntityComponent(linkEntity, componentType);
                LinkBuffer.Add(new IncomingLink(linkEntity, target, component));
            }
        }
        // --- add all link relations
        var relationsMap = store.extension.relationsMap;
        foreach (var componentType in relationTypes) {
            var relations = relationsMap[componentType.StructIndex];
            relations.AddLinkRelations(entity.Id, LinkBuffer);
        }
        var links = LinkBuffer.ToArray();
        return new IncomingLinks(entity, links);
    }

    
}