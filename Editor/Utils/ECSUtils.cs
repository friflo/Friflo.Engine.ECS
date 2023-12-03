// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Collections;
using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable ReturnTypeCanBeEnumerable.Global
namespace Friflo.Fliox.Editor.Utils;

public static class ECSUtils
{
    private static void Log(Func<string> message) {
        return;
#pragma warning disable CS0162 // Unreachable code detected
        var msg = message();
        Console.WriteLine(msg);
#pragma warning restore CS0162 // Unreachable code detected
    }
    
    /// <summary> Convert a JSON array to <see cref="DataEntity"/>'s </summary>
    internal static string JsonArrayToDataEntities(JsonValue jsonArray, List<DataEntity> dataEntities)
    {
        // --- 
        var serializer      = new EntitySerializer();
        var stream          = new MemoryStream(jsonArray.Count);
        stream.Write(jsonArray.AsReadOnlySpan());
        stream.Position     = 0;
        var result          = serializer.ReadEntities(dataEntities, stream);
        // Console.WriteLine($"Paste - entities: {result.entityCount}, error: {result.error}");
        if (result.error != null) {
            Console.Error.WriteLine($"Paste error: {result.error}");
            return result.error;
        }
        return null;
    }
    
#region duplicate Entity's
    internal static void DuplicateEntities(List<Entity> entities)
    {
        var store   = entities[0].Store;
        foreach (var entity in entities) {
            var parent  = entity.Parent;
            if (parent == null) {
                continue;
            }
            var clone = store.CloneEntity(entity);
            parent.AddChild(clone);
            
            DuplicateChildren(entity, clone, store);
        }
    }
    
    private static void DuplicateChildren(Entity entity, Entity clone, EntityStore store)
    {
        foreach (var childId in entity.ChildIds) {
            var child       = store.GetNodeById(childId).Entity;
            var childClone  = store.CloneEntity(child);
            clone.AddChild(childClone);
            
            DuplicateChildren(child, childClone, store);
        }
    }
    
    #endregion
    
#region paste DataEntity's
    /// <remarks> The order of items in <paramref name="dataEntities"/> is not relevant. </remarks>
    internal static void AddDataEntitiesToEntity(Entity targetEntity, List<DataEntity> dataEntities)
    {
        var childEntities   = new HashSet<long>(dataEntities.Count);
        var pidMap          = new Dictionary<long, long>();
        var store           = targetEntity.Store;
        
        // --- create a new Entity for every DataEntity in the store
        foreach (var dataEntity in dataEntities)
        {
            var entity              = store.CreateEntity();
            var newPid              = store.GetNodeById(entity.Id).Pid;
            pidMap[dataEntity.pid]  = newPid;
            dataEntity.pid          = newPid;
        }
        var converter = EntityConverter.Default;
        
        // --- convert each DataEntity into an Entity's created above
        //     replace children pid's with their new pid
        foreach (var dataEntity in dataEntities)
        {
            var children = dataEntity.children;
            dataEntity.children = null;
            converter.DataEntityToEntity(dataEntity, store, out _);
            
            ReplaceChildrenPids(children, pidMap, store);
            dataEntity.children = children;
        }
        // --- add child entities to their parent entity
        foreach (var dataEntity in dataEntities)
        {
            var entity      = store.GetNodeByPid(dataEntity.pid).Entity;
            var children    = dataEntity.children;
            if (children == null) {
                continue;
            }
            foreach (var childPid in children) {
                childEntities.Add(childPid);
                var child = store.GetNodeByPid(childPid).Entity;
                entity.AddChild(child);
            }
        }
        // --- add all root entities to target
        foreach (var dataEntity in dataEntities)
        {
            var pid = dataEntity.pid;
            if (childEntities.Contains(pid)) {
                continue;
            }
            var entity = store.GetNodeByPid(pid).Entity;
            targetEntity.AddChild(entity);
        }
    }
    
    private static void ReplaceChildrenPids(List<long> children, Dictionary<long, long> pidMap, EntityStore store)
    {
        if (children == null) {
            return;
        }
        // replace each child pid with its new pid
        for (int n = 0; n < children.Count; n++)
        {
            var oldPid  = children[n];
            if (pidMap.TryGetValue(oldPid, out long newPid)) {
                children[n] = newPid;
                continue;
            }
            var missingChild    = store.CreateEntity();
            missingChild.AddComponent(new EntityName($"missing entity - pid: {oldPid}"));
            var missingChildPid = store.GetNodeById(missingChild.Id).Pid;
            children[n]         = missingChildPid;
            pidMap[oldPid]      = missingChildPid;
        }
    }
    #endregion
    
#region Copy Entity's
    /// <summary> Create a JSON array from given <paramref name="entities"/> </summary>
    internal static JsonValue EntitiesToJsonArray(IEnumerable<Entity> entities)
    {
        var stream      = new MemoryStream();
        var serializer  = new EntitySerializer();
        var treeList    = new List<Entity>();
        var treeSet     = new HashSet<Entity>();

        foreach (var entity in entities)
        {
            if (!treeSet.Add(entity)) {
                continue;
            }
            treeList.Add(entity);
            AddChildren(entity, treeList, treeSet);
        }
        serializer.WriteEntities(treeList, stream);
    
        return new JsonValue(stream.GetBuffer(), 0, (int)stream.Length);
    }
    
    private static void AddChildren(Entity entity, List<Entity> list, HashSet<Entity> set)
    {
        foreach (var childNode in entity.ChildNodes) {
            var child = childNode.Entity;
            if (!set.Add(child)) {
                continue;
            }
            list.Add(child);
            AddChildren(child, list, set);
        }
    }
    #endregion
    
#region ExplorerItem's
    internal static void RemoveExplorerItems(ExplorerItem[] items, ExplorerItem rootItem)
    {
        foreach (var item in items) {
            var entity = item.Entity; 
            if (entity.TreeMembership != TreeMembership.treeNode) {
                continue;
            }
            if (rootItem == item) {
                continue;
            }
            var parent = entity.Parent;
            Log(() => $"parent id: {parent.Id} - Remove child id: {entity.Id}");
            parent.RemoveChild(entity);
        }
    }
    
    internal static int[] MoveExplorerItemsUp(ExplorerItem[] items, int shift)
    {
        var indexes = new List<int>(items.Length);
        var parent  = items[0].Entity.Parent;
        var pos     = 0;
        foreach (var item in items)
        {
            var entity      = item.Entity;
            int index       = parent.GetChildIndex(entity.Id);
            int newIndex    = index - shift;
            if (newIndex < pos++) {
                continue;
            }
            indexes.Add(newIndex);
            Log(() => $"parent id: {parent.Id} - Move child: ChildIds[{newIndex}] = {entity.Id}");
            parent.InsertChild(newIndex, entity);
        }
        return indexes.ToArray();
    }
    
    internal static int[] MoveExplorerItemsDown(ExplorerItem[] items, int shift)
    {
        var indexes     = new List<int>(items.Length);
        var parent      = items[0].Entity.Parent;
        var childCount  = parent.ChildCount;
        var pos     = 0;
        for (int n = items.Length - 1; n >= 0; n--)
        {
            var entity      = items[n].Entity;
            int index       = parent.GetChildIndex(entity.Id);
            int newIndex    = index + shift;
            if (newIndex >= childCount - pos++) {
                continue;
            }
            indexes.Add(newIndex);
            Log(() => $"parent id: {parent.Id} - Move child: ChildIds[{newIndex}] = {entity.Id}");
            parent.InsertChild(newIndex, entity);
        }
        return indexes.ToArray();
    }
    #endregion
}
