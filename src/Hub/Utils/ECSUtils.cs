// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Friflo.Fliox.Engine.ECS.Collections;
using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox;


// ReSharper disable HeuristicUnreachableCode
// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS.Utils;

/// <remarks> Note: This file will be moved to project: <see cref="Friflo.Fliox.Engine.ECS"/> </remarks>
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
    public static string JsonArrayToDataEntities(JsonValue jsonArray, List<DataEntity> dataEntities)
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
    
#region Duplicate Entity's
    public static int[] DuplicateEntities(List<Entity> entities)
    {
        var indexes = new List<int>(entities.Count);
        var store   = entities[0].Store;
        foreach (var entity in entities) {
            var parent  = entity.Parent;
            if (parent.IsNull) {
                continue;
            }
            var clone = store.CloneEntity(entity);
            var index = parent.AddChild(clone);
            indexes.Add(index);
            
            DuplicateChildren(entity, clone, store);
        }
        return indexes.ToArray();
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
    /// <remarks> The order of items in <paramref name="dataEntities"/> is not relevant. </remarks>
    public static AddDataEntitiesResult AddDataEntitiesToEntity(Entity targetEntity, List<DataEntity> dataEntities)
    {
        var entityCount     = dataEntities.Count;
        var addedEntities   = new HashSet<long>         (entityCount);               
        var childEntities   = new HashSet<long>         (entityCount);
        var oldToNewPid     = new Dictionary<long, long>(entityCount);
        var newToOldPid     = new Dictionary<long, long>(entityCount);
        var store           = targetEntity.Store;
        
        // --- create a new Entity for every DataEntity in the store
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
        var missingPids = new HashSet<long>();            
        foreach (var dataEntity in dataEntities)
        {
            var children = dataEntity.children;
            dataEntity.children = null;
            converter.DataEntityToEntity(dataEntity, store, out _);
            
            ReplaceChildrenPids(children, oldToNewPid, newToOldPid, store, missingPids);
            dataEntity.children = children;
        }
        // --- add child entities to their parent entity
        var addErrors = new HashSet<long>();  
        foreach (var dataEntity in dataEntities)
        {
            var entity      = store.GetEntityByPid(dataEntity.pid);
            var children    = dataEntity.children;
            if (children == null) {
                continue;
            }
            foreach (var childPid in children) {
                childEntities.Add(childPid);
                var child = store.GetEntityByPid(childPid);
                var index = entity.AddChild(child);
                if (index >= 0) {
                    addedEntities.Add(childPid);
                    continue;
                }
                addErrors.Add(newToOldPid[childPid]);
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
            addedEntities.Add(pid);
            indexes.Add(index);
        }
        return new AddDataEntitiesResult {
            indexes         = indexes,
            addedEntities   = addedEntities,
            missingPids     = missingPids,
            addErrors       = addErrors
        };
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
    /// <summary> Create a JSON array from given <paramref name="entities"/> </summary>
    public static JsonEntities EntitiesToJsonArray(IEnumerable<Entity> entities)
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
        var json = new JsonValue(stream.GetBuffer(), 0, (int)stream.Length); 
        return new JsonEntities { count = treeList.Count, entities = json };
    }
    
    private static void AddChildren(Entity entity, List<Entity> list, HashSet<Entity> set)
    {
        foreach (var child in entity.ChildEntities) {
            if (!set.Add(child)) {
                continue;
            }
            list.Add(child);
            AddChildren(child, list, set);
        }
    }
    #endregion
    
#region Remove ExplorerItem's
    public static void RemoveExplorerItems(ExplorerItem[] items, ExplorerItem rootItem)
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
    #endregion
    
#region Move ExplorerItem's
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
            int index       = parent.GetChildIndex(entity.Id);
            int newIndex    = index - shift;
            if (newIndex < pos) {
                indexes[pos] = index;
            } else {
                indexes[pos] = newIndex;
                Log(() => $"parent id: {parent.Id} - Move child: ChildIds[{newIndex}] = {entity.Id}");
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
            int index       = parent.GetChildIndex(entity.Id);
            int newIndex    = index + shift;
            if (newIndex >= childCount - pos++) {
                indexes[n] = index;
                continue;
            }
            indexes[n] = newIndex;
            Log(() => $"parent id: {parent.Id} - Move child: ChildIds[{newIndex}] = {entity.Id}");
            parent.InsertChild(newIndex, entity);
        }
        return indexes;
    }
    #endregion
}


public class AddDataEntitiesResult
{
    public  List<int>       indexes;
    /// <summary> contains new pid's </summary>
    public  HashSet<long>   addedEntities;
    /// <summary> contains old pid's </summary>
    public  HashSet<long>   missingPids;
    /// <summary> contains old pid's </summary>
    public  HashSet<long>   addErrors;
}

public class JsonEntities
{
    public  int             count;
    public  JsonValue       entities;
}