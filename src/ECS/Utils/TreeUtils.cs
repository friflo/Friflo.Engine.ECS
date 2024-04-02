// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Friflo.Engine.ECS.Collections;
using Friflo.Engine.ECS.Serialize;
using Friflo.Json.Fliox;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable ReturnTypeCanBeEnumerable.Global
namespace Friflo.Engine.ECS.Utils;

public static class TreeUtils
{
#region Convert a JSON array to DataEntity's
    /// <summary> Convert a JSON array to <see cref="DataEntity"/>'s </summary>
    public static string JsonArrayToDataEntities(JsonValue jsonArray, List<DataEntity> dataEntities)
    {
        var serializer      = new EntitySerializer();
        var stream          = new MemoryStream(jsonArray.Count);
        stream.Write(jsonArray.AsByteArray(), jsonArray.start, jsonArray.Count);
        stream.Position     = 0;
        var result          = serializer.ReadEntities(dataEntities, stream);
        // Console.WriteLine($"Paste - entities: {result.entityCount}, error: {result.error}");
        if (result.error != null) {
            Console.Error.WriteLine($"Paste error: {result.error}");
            return result.error;
        }
        return null;
    }
    #endregion
    
#region Duplicate Entity's
    /// <returns> the indexes of the duplicated entities within the parent of the original entities</returns>
    public static int[] DuplicateEntities(List<Entity> entities)
    {
        var indexes = new int [entities.Count];
        var store   = entities[0].Store;
        int pos     = 0;
        foreach (var entity in entities) {
            var parent  = entity.Parent;
            if (parent.IsNull) {
                indexes[pos++] = -1;
                continue;
            }
            var clone       = store.CloneEntity(entity);
            var index       = parent.AddChild(clone);
            indexes[pos++]  = index;
            DuplicateChildren(entity, clone, store);
        }
        return indexes;
    }
    
    private static void DuplicateChildren(Entity entity, Entity clone, EntityStore store)
    {
        foreach (var childId in entity.ChildIds) {
            var child       = store.GetEntityById(childId);
            var childClone  = store.CloneEntity(child);
            clone.AddChild(childClone);
            
            DuplicateChildren(child, childClone, store);
        }
    }
    
    #endregion
    
#region Paste DataEntity's
    /// <summary>
    /// Add the given <paramref name="dataEntities"/> to the specified <paramref name="targetEntity"/>.<br/>
    /// The <see cref="DataEntity.pid"/> and the <see cref="DataEntity.children"/> of the given <paramref name="dataEntities"/> 
    /// are replaced with the pids of the newly created <see cref="Entity"/>'s. 
    /// </summary>
    /// <remarks>
    /// The order of items in <paramref name="dataEntities"/> is not relevant.
    /// </remarks>
    public static AddDataEntitiesResult AddDataEntitiesToEntity(Entity targetEntity, IReadOnlyList<DataEntity> dataEntities)
    {
        var entityCount     = dataEntities.Count;
        var childEntities   = new HashSet<long>         (entityCount);
        var oldToNewPid     = new Dictionary<long, long>(entityCount);
        var newToOldPid     = new Dictionary<long, long>(entityCount);
        var store           = targetEntity.Store;
        
        // --- create a new Entity for every DataEntity in dataEntities
        foreach (var dataEntity in dataEntities)
        {
            var entity                  = store.CreateEntity();
            var newPid                  = entity.Pid;
            oldToNewPid[dataEntity.pid] = newPid;
            newToOldPid[newPid]         = dataEntity.pid;
            dataEntity.pid              = newPid;
        }
        var converter = EntityConverter.Default;
        
        // --- convert each DataEntity into an Entity's created above
        //     replace children pid's with their new pid
        var errors      = new List<string>();
        var missingPids = new HashSet<long>();            
        foreach (var dataEntity in dataEntities)
        {
            var children = dataEntity.children;
            dataEntity.children = null;
            converter.DataEntityToEntity(dataEntity, store, out string error);
            if (error != null) {
                var oldPid = newToOldPid[dataEntity.pid];
                errors.Add($"entity: {oldPid} {error}");
            }
            missingPids.Clear();
            ReplaceChildrenPids(children, oldToNewPid, newToOldPid, store, missingPids);
            
            if (missingPids.Count > 0) {
                var oldPid  = newToOldPid[dataEntity.pid];
                var ids     = string.Join(",", missingPids);
                errors.Add($"entity: {oldPid} 'children' - missing entities: [{ids}]");
            }
            dataEntity.children = children;
        }
        // --- add child entities to their parent entity
        foreach (var dataEntity in dataEntities)
        {
            var entity      = store.GetEntityByPid(dataEntity.pid);
            var children    = dataEntity.children;
            if (children == null) {
                continue;
            }
            foreach (var childPid in children) {
                var child = store.GetEntityByPid(childPid);
                if (entity.Id != child.Id) {
                    childEntities.Add(childPid);
                    entity.AddChild(child);
                    continue;
                }
                var oldPid = newToOldPid[childPid];
                errors.Add($"entity: {oldPid} 'children' - entity contains itself as a child.");
            }
        }
        // --- add all root entities to target
        var indexes = new List<int>();
        foreach (var dataEntity in dataEntities)
        {
            var pid = dataEntity.pid;
            if (childEntities.Contains(pid)) {
                continue;
            }
            var entity = store.GetEntityByPid(pid);
            var index = targetEntity.AddChild(entity);
            indexes.Add(index);
        }
        return new AddDataEntitiesResult { indexes = indexes, errors = errors };
    }
    
    private static void ReplaceChildrenPids(
        List<long>              children,
        Dictionary<long, long>  oldToNewPid,
        Dictionary<long, long>  newToOldPid,
        EntityStore             store,
        HashSet<long>           missingPids)
    {
        if (children == null) {
            return;
        }
        // replace each child pid with its new pid
        for (int n = 0; n < children.Count; n++)
        {
            var oldPid  = children[n];
            if (oldToNewPid.TryGetValue(oldPid, out long newPid)) {
                children[n] = newPid;
                continue;
            }
            missingPids.Add(oldPid);
            var missingChild    = store.CreateEntity();
            missingChild.AddComponent(new EntityName($"missing entity - pid: {oldPid}"));
            var missingChildPid             = missingChild.Pid;
            children[n]                     = missingChildPid;
            oldToNewPid[oldPid]             = missingChildPid;
            newToOldPid[missingChildPid]    = oldPid;
        }
    }
    #endregion
    
#region Copy Entity's
    /// <summary>
    /// Create a JSON array from given <paramref name="entities"/> including their <see cref="Entity.ChildEntities"/>.
    /// </summary>
    public static JsonEntities EntitiesToJsonArray(IEnumerable<Entity> entities)
    {
        var stream      = new MemoryStream();
        var serializer  = new EntitySerializer();
        var treeList    = new List<Entity>();
        var treeSet     = new HashSet<Entity>(EntityUtils.EqualityComparer);

        foreach (var entity in entities)
        {
            if (!treeSet.Add(entity)) {
                continue;
            }
            treeList.Add(entity);
            AddChildren(entity, treeList, treeSet);
        }
        serializer.WriteEntities(treeList, stream);
        var json = new JsonValue(stream.GetBuffer(), 0, (int)stream.Length); 
        return new JsonEntities { count = treeList.Count, entities = json };
    }
    
    private static void AddChildren(Entity entity, List<Entity> list, HashSet<Entity> set)
    {
        foreach (var child in entity.ChildEntities) {
            if (!set.Add(child)) {
                // case: child already added => skip 
                continue;
            }
            list.Add(child);
            AddChildren(child, list, set);
        }
    }
    #endregion
    
#region Remove ExplorerItem's
    private static readonly bool Log = false;
    
    [ExcludeFromCodeCoverage]
    private static void LogRemove(Entity parent, Entity entity) {
        if (!Log) return;
        var msg = $"parent id: {parent.Id} - Remove child id: {entity.Id}";
        Console.WriteLine(msg);
    }
    
    public static void RemoveExplorerItems(ExplorerItem[] items)
    {
        foreach (var item in items) {
            var entity = item.Entity; 
            if (entity.TreeMembership != TreeMembership.treeNode) {
                // case: entity is not a tree member => cannot remove from tree
                continue;
            }
            var parent = entity.Parent;
            if (parent.IsNull) {
                // case: entity is root item => cannot remove root item
                continue;
            }
            LogRemove(parent, entity);
            parent.RemoveChild(entity);
        }
    }
    #endregion
    
#region Move ExplorerItem's
    [ExcludeFromCodeCoverage]
    private static void LogMove(Entity parent, int newIndex, Entity entity) {
        if (!Log) return;
        var msg = $"parent id: {parent.Id} - Move child: Child[{newIndex}] = {entity.Id}";
        Console.WriteLine(msg);
    }

    public static int[] MoveExplorerItemsUp(ExplorerItem[] items, int shift)
    {
        var parent  = items[0].Entity.Parent;
        if (parent.IsNull) {
            return null;
        }
        var indexes = new int[items.Length];
        var pos     = 0;
        foreach (var item in items)
        {
            var entity      = item.Entity;
            int index       = parent.GetChildIndex(entity);
            int newIndex    = index - shift;
            if (newIndex < pos) {
                indexes[pos] = index;
            } else {
                indexes[pos] = newIndex;
                LogMove(parent, newIndex, entity);
                parent.InsertChild(newIndex, entity);
            }
            pos++;
        }
        return indexes;
    }
    
    public static int[] MoveExplorerItemsDown(ExplorerItem[] items, int shift)
    {
        var parent      = items[0].Entity.Parent;
        if (parent.IsNull) {
            return null;
        }
        var indexes     = new int[items.Length];
        var childCount  = parent.ChildCount;
        var pos         = 0;
        for (int n = items.Length - 1; n >= 0; n--)
        {
            var entity      = items[n].Entity;
            int index       = parent.GetChildIndex(entity);
            int newIndex    = index + shift;
            if (newIndex >= childCount - pos++) {
                indexes[n] = index;
                continue;
            }
            indexes[n] = newIndex;
            LogMove(parent, newIndex, entity);
            parent.InsertChild(newIndex, entity);
        }
        return indexes;
    }
    #endregion
}


public sealed class AddDataEntitiesResult
{
    public  List<int>       indexes;
    /// <summary> contains errors detected when executing <see cref="TreeUtils.AddDataEntitiesToEntity"/> </summary>
    public  List<string>    errors;
}

public sealed class JsonEntities
{
    public  int             count;
    public  JsonValue       entities;
}