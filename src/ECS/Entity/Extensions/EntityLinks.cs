// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using Friflo.Engine.ECS.Collections;
using Friflo.Engine.ECS.Index;
using System.Collections.Generic;
using Friflo.Engine.ECS.Utils;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class EntityExtensions
{
#region incoming links
    private static readonly List<EntityLink> LinkBuffer = new ();
    
    public static EntityLinks GetAllIncomingLinks(this Entity target)
    {
        GetIncomingLinkTypes(target, out var indexTypes, out var relationTypes);
        var store       = target.store;
        var targetId    = target.Id;
        var linkBuffer  = LinkBuffer;
        linkBuffer.Clear();
        
        // --- add all incoming link components
        var indexMap = store.extension.indexMap;
        foreach (var componentType in indexTypes)
        {
            var entityIndex = (EntityIndex)indexMap[componentType.StructIndex];
            entityIndex.entityMap.TryGetValue(targetId, out var idArray);
            var idSpan = idArray.GetSpan(entityIndex.idHeap, target.store);
            foreach (var linkId in idSpan) {
                var linkEntity  = new Entity(store, linkId);
                var component   = EntityUtils.GetEntityComponent(linkEntity, componentType);
                linkBuffer.Add(new EntityLink(linkEntity, targetId, component));
            }
        }
        // --- add all incoming link relations
        var relationsMap = store.extension.relationsMap;
        foreach (var componentType in relationTypes) {
            var relations = relationsMap[componentType.StructIndex];
            relations.AddIncomingRelations(target.Id, linkBuffer);
        }
        var links = linkBuffer.ToArray();
        linkBuffer.Clear(); // clear to avoid tracing references by GC
        return new EntityLinks(links);
    }
    
    public static int CountAllIncomingLinks(this Entity target)
    {
        GetIncomingLinkTypes(target, out var indexTypes, out var relationTypes);
        var store   = target.store;
        int count   = 0;
        // --- count all incoming link components
        var indexMap = store.extension.indexMap;
        foreach (var componentType in indexTypes)
        {
            var entityIndex = (EntityIndex)indexMap[componentType.StructIndex];
            entityIndex.entityMap.TryGetValue(target.Id, out var idArray);
            count += idArray.Count;
        }
        // --- count all incoming link relations
        var relationsMap = store.extension.relationsMap;
        foreach (var componentType in relationTypes) {
            var relations   = relationsMap[componentType.StructIndex];
            count          += relations.CountIncomingLinkRelations(target.Id);
        }
        return count;
    }
    #endregion
    
    
#region outgoing links
    public static int CountAllOutgoingLinks(this Entity entity)
    {
        var schema  = EntityStoreBase.Static.EntitySchema;
        var type    = entity.GetArchetype() ?? throw EntityStoreBase.EntityNullException(entity);
        // --- count link components
        var linkTypes   = new ComponentTypes {
            bitSet = BitSet.Intersect(type.componentTypes.bitSet, schema.linkComponentTypes.bitSet)
        };
        // --- count relations components
        int count           = linkTypes.Count;
        var store           = entity.store;
        var relationsMap    = store.extension.relationsMap;
        var relationsTypes  = new ComponentTypes();
        relationsTypes.bitSet.l0 = store.nodes[entity.Id].isOwner & schema.linkRelationTypes.bitSet.l0;
        foreach (var componentType in relationsTypes) {
            var relations   = relationsMap[componentType.StructIndex];
            count          += relations.GetRelationCount(entity);
        }
        return count;
    }
    #endregion
    
#region utils
    internal static void GetIncomingLinkTypes(Entity target, out ComponentTypes indexTypes, out ComponentTypes relationTypes)
    {
        var store               = target.store;
        var isLinked            = store.nodes[target.Id].isLinked;
        indexTypes              = new ComponentTypes();
        relationTypes           = new ComponentTypes();
        var schema              = EntityStoreBase.Static.EntitySchema;
        indexTypes.bitSet.l0    = schema.indexTypes.   bitSet.l0 & isLinked; // intersect
        relationTypes.bitSet.l0 = schema.relationTypes.bitSet.l0 & isLinked; // intersect
    }
    #endregion
}