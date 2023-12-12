// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Collections;
using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable InconsistentNaming
namespace Tests.Hub;


public static class Test_ECSUtils
{
#region JSON array -> DataEntity's
    /// <summary> Cover <see cref="ECSUtils.JsonArrayToDataEntities"/> </summary>
    [Test]
    public static void Test_ECSUtils_JsonArrayToDataEntities()
    {
        var json = new JsonValue(
        """
        [{
            "id": 2,
            "tags": ["Tag1"],
            "components": {
                "name": { "value": "test" }
            },
            "children": [3]
        }]
        """);
        var dataEntities = new List<DataEntity>();
        IsNull(ECSUtils.JsonArrayToDataEntities(json, dataEntities));
        
        AreEqual(1, dataEntities.Count);
        var data0 = dataEntities[0];
        AreEqual(2,                 data0.pid);
        AreEqual(new [] { "Tag1" }, data0.tags);
        AreEqual(new [] { 3 },      data0.children);
        var components = "{\n        \"name\": { \"value\": \"test\" }\n    }";
        AreEqual(components,        data0.components.ToString());
    }
    
    /// <summary> Cover <see cref="ECSUtils.JsonArrayToDataEntities"/> errors </summary>
    [Test]
    public static void Test_ECSUtils_JsonArrayToDataEntities_errors()
    {
        var dataEntities = new List<DataEntity>();
        {
            var error = ECSUtils.JsonArrayToDataEntities(new JsonValue(), dataEntities);
            AreEqual("expect array. was: ValueNull at position: 4", error);
        } {
            var error = ECSUtils.JsonArrayToDataEntities(new JsonValue(""), dataEntities);
            AreEqual("unexpected EOF on root path: '(root)' at position: 0", error);
        } {
            var error = ECSUtils.JsonArrayToDataEntities(new JsonValue("{}"), dataEntities);
            AreEqual("expect array. was: ObjectStart at position: 1", error);
        }
    }
    #endregion
    
#region Duplicate Entity's
    /// <summary> Cover <see cref="ECSUtils.DuplicateEntities"/> and <see cref="ECSUtils.DuplicateChildren"/> </summary>
    [Test]
    public static void Test_ECSUtils_DuplicateEntities()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity(1);
        var child2  = store.CreateEntity(2);
        var child3  = store.CreateEntity(3);
        child2.AddComponent(new EntityName("child-2"));
        child3.AddComponent(new EntityName("child-3"));
        
        store.SetStoreRoot(root);
        root.AddChild(child2);
        child2.AddChild(child3);
        AreEqual(1,             root.ChildCount);
        AreEqual(1,             child2.ChildCount);
        
        // --- Duplicate two child entities
        var entities    = new List<Entity>{ child2, child3 };
        var indexes     = ECSUtils.DuplicateEntities(entities);
        
        AreEqual(2,             indexes.Length);
        AreEqual(2,             root.ChildCount);
        AreEqual(2,             child2.ChildCount);
        
        var clone2 =            root.ChildEntities  [indexes[0]];
        AreEqual("child-2",     clone2.Name.value);
        
        var clone3 =            child2.ChildEntities[indexes[1]];
        AreEqual("child-3",     clone3.Name.value);
        
        // --- Duplicate root
        entities        = new List<Entity>{ root };
        indexes         = ECSUtils.DuplicateEntities(entities);
        
        AreEqual(2,             root.ChildCount);
        AreEqual(1,             indexes.Length);
        AreEqual(-1,            indexes[0]);
    }
    #endregion
    
#region Entity's -> JSON array
    /// <summary> Cover <see cref="ECSUtils.EntitiesToJsonArray"/> and <see cref="ECSUtils.AddChildren"/></summary>
    [Test]
    public static void Test_ECSUtils_EntitiesToJsonArray()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity(1);
        var child2  = store.CreateEntity(2);
        var child3  = store.CreateEntity(3);
        root.AddChild(child2);
        root.AddChild(child3);
        root.  AddComponent(new EntityName("root"));
        child2.AddComponent(new EntityName("child-2"));
        child3.AddComponent(new EntityName("child-3"));
        
        var entities        = new [] { root, child2 };
        var jsonEntities    = ECSUtils.EntitiesToJsonArray(entities);
        
        AreEqual(3, jsonEntities.count);
        var json =
"""
[{
    "id": 1,
    "children": [
        2,
        3
    ],
    "components": {
        "name": {"value":"root"}
    }
},{
    "id": 2,
    "components": {
        "name": {"value":"child-2"}
    }
},{
    "id": 3,
    "components": {
        "name": {"value":"child-3"}
    }
}]
""";
        AreEqual(json, jsonEntities.entities.ToString());
    }
    #endregion
    
#region Add DataEntity's to Entity
    /// <summary> Cover <see cref="ECSUtils.AddDataEntitiesToEntity"/> and <see cref="ECSUtils.ReplaceChildrenPids"/></summary>
    [Test]
    public static void Test_ECSUtils_AddDataEntitiesToEntity()
    {
        var store           = new EntityStore(PidType.UsePidAsId);
        var root            = store.CreateEntity(1);
        var child1          = store.CreateEntity(2);
        root.AddChild(child1);
        
        var dataEntity10    = new DataEntity { pid = 10, children = new List<long> { 11 }};
        var dataEntity11    = new DataEntity { pid = 11 };
        var dataEntities    = new [] { dataEntity10, dataEntity11 };
        
        var result = ECSUtils.AddDataEntitiesToEntity(root, dataEntities);
        
        AreEqual(1,         result.indexes.Count);
        AreEqual(2,         result.addedEntities.Count);
        AreEqual(0,         result.errors.Count);
    }
    #endregion

#region Remove ExplorerItem's
    /// <summary> Cover <see cref="ECSUtils.RemoveExplorerItems"/> </summary>
    [Test]
    public static void Test_ECSUtils_RemoveExplorerItems()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity(1);
        store.SetStoreRoot(root);
        var child2  = store.CreateEntity(2);
        var child3  = store.CreateEntity(3);
        root.AddChild(child2);
        root.AddChild(child3);
        root.  AddComponent(new EntityName("root"));
        child2.AddComponent(new EntityName("child-2"));
        child3.AddComponent(new EntityName("child-3"));
        
        var tree        = new ExplorerItemTree(root, "test-tree");
        var rootItem    = tree.GetItemById(1);
        var item2       = tree.GetItemById(2);
        var items = new [] { item2 };
        AreEqual(2,         root.ChildCount);
        AreEqual("child-2", root.ChildEntities[0].Name.value);
     
        // remove child2
        ECSUtils.RemoveExplorerItems(items, rootItem);
        
        AreEqual(1,         root.ChildCount);
        AreEqual("child-3", root.ChildEntities[0].Name.value);
    }
    #endregion
    
#region Move ExplorerItem's
    /// <summary> Cover <see cref="ECSUtils.MoveExplorerItemsUp"/> </summary>
    [Test]
    public static void Test_ECSUtils_MoveExplorerItemsUp()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity(1);
        store.SetStoreRoot(root);
        var child2  = store.CreateEntity(2);
        var child3  = store.CreateEntity(3);
        root.AddChild(child2);
        root.AddChild(child3);
        root.AddComponent(new EntityName("root"));
        
        var tree        = new ExplorerItemTree(root, "test-tree");
        var item3       = tree.GetItemById(3);
        var items       = new [] { item3 };
        AreEqual(new [] { 2, 3 },   root.ChildIds.ToArray());
     
        // move item3 up
        ECSUtils.MoveExplorerItemsUp(items, 1);
        
        AreEqual(new [] { 3, 2 },   root.ChildIds.ToArray());
    }
    
    /// <summary> Cover <see cref="ECSUtils.MoveExplorerItemsDown"/> </summary>
    [Test]
    public static void Test_ECSUtils_MoveExplorerItemsDown()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity(1);
        store.SetStoreRoot(root);
        var child2  = store.CreateEntity(2);
        var child3  = store.CreateEntity(3);
        root.AddChild(child2);
        root.AddChild(child3);
        root.AddComponent(new EntityName("root"));
        
        var tree        = new ExplorerItemTree(root, "test-tree");
        var item2       = tree.GetItemById(2);
        var items       = new [] { item2 };
        AreEqual(new [] { 2, 3 },   root.ChildIds.ToArray());
     
        // move item2 down
        ECSUtils.MoveExplorerItemsDown(items, 1);
        
        AreEqual(new [] { 3, 2 },   root.ChildIds.ToArray());
    }
    #endregion
}