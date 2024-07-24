// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Collections;
using Friflo.Engine.ECS.Serialize;
using Friflo.Engine.ECS.Utils;
using Friflo.Json.Fliox;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Utils {


public static class Test_TreeUtils
{
#region JSON array -> DataEntity's
    /// <summary> Cover <see cref="TreeUtils.JsonArrayToDataEntities"/> </summary>
    [Test]
    public static void Test_TreeUtils_JsonArrayToDataEntities()
    {
        var json = new JsonValue(
@"
[{
    ""id"": 2,
    ""tags"": [""Tag1""],
    ""components"": {
        ""name"": { ""value"": ""test"" }
    },
    ""children"": [3]
}]
");
        var dataEntities = new List<DataEntity>();
        IsNull(TreeUtils.JsonArrayToDataEntities(json, dataEntities));
        
        AreEqual(1, dataEntities.Count);
        var data0 = dataEntities[0];
        AreEqual(2,                 data0.pid);
        AreEqual(new [] { "Tag1" }, data0.tags);
        AreEqual(new [] { 3 },      data0.children);
        var components = "{\n        \"name\": { \"value\": \"test\" }\n    }";
        AreEqual(components,        data0.components.ToString());
    }
    
    /// <summary> Cover <see cref="TreeUtils.JsonArrayToDataEntities"/> errors </summary>
    [Test]
    public static void Test_TreeUtils_JsonArrayToDataEntities_errors()
    {
        var dataEntities = new List<DataEntity>();
        {
            var error = TreeUtils.JsonArrayToDataEntities(new JsonValue(), dataEntities);
            AreEqual("expect array. was: ValueNull at position: 4", error);
        } {
            var error = TreeUtils.JsonArrayToDataEntities(new JsonValue(""), dataEntities);
            AreEqual("unexpected EOF on root path: '(root)' at position: 0", error);
        } {
            var error = TreeUtils.JsonArrayToDataEntities(new JsonValue("{}"), dataEntities);
            AreEqual("expect array. was: ObjectStart at position: 1", error);
        }
    }
    #endregion
    
#region Duplicate Entity's
    /// <summary> Cover <see cref="TreeUtils.DuplicateEntities"/> and <see cref="TreeUtils.DuplicateChildren"/> </summary>
    [Test]
    public static void Test_TreeUtils_DuplicateEntities()
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
        var indexes     = TreeUtils.DuplicateEntities(entities);
        
        AreEqual(2,             indexes.Length);
        AreEqual(2,             root.ChildCount);
        AreEqual(2,             child2.ChildCount);
        
        var clone2 =            root.ChildEntities  [indexes[0]];
        AreEqual(1,             clone2.ChildEntities.Count);
        AreEqual("child-3",     clone2.ChildEntities[0].Name.value);
        AreEqual("child-2",     clone2.Name.value);
        
        var clone3 =            child2.ChildEntities[indexes[1]];
        AreEqual("child-3",     clone3.Name.value);
        
        // --- Duplicate root
        entities        = new List<Entity>{ root };
        indexes         = TreeUtils.DuplicateEntities(entities);
        
        AreEqual(2,             root.ChildCount);
        AreEqual(1,             indexes.Length);
        AreEqual(-1,            indexes[0]);
    }
    #endregion
    
#region Entity's -> JSON array
    /// <summary> Cover <see cref="TreeUtils.EntitiesToJsonArray"/> and <see cref="TreeUtils.AddChildren"/></summary>
    [Test]
    public static void Test_TreeUtils_EntitiesToJsonArray()
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
        var jsonEntities    = TreeUtils.EntitiesToJsonArray(entities);
        
        AreEqual(3, jsonEntities.count);
        var json =
@"[{
    ""id"": 1,
    ""children"": [
        2,
        3
    ],
    ""components"": {
        ""name"": {""value"":""root""}
    }
},{
    ""id"": 2,
    ""components"": {
        ""name"": {""value"":""child-2""}
    }
},{
    ""id"": 3,
    ""components"": {
        ""name"": {""value"":""child-3""}
    }
}]";
        AreEqual(json, jsonEntities.entities.ToString());
    }
    
    /// <summary> Cover <see cref="TreeUtils.EntitiesToJsonArray"/> and <see cref="TreeUtils.AddChildren"/></summary>
    [Test]
    public static void Test_TreeUtils_EntitiesToJsonArray_deduplicate_result()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity(1);
        var child2  = store.CreateEntity(2);
        root.AddChild(child2);
        root.  AddComponent(new EntityName("root"));
        child2.AddComponent(new EntityName("child-2"));
        // JSON of child2 is returned only once - deduplicated - even
        // - it is added explicit to requested entities and
        // - it is a child of its requested parent (root)
        var entities        = new [] { child2, root };
        var jsonEntities    = TreeUtils.EntitiesToJsonArray(entities);
        
        AreEqual(2, jsonEntities.count);
        var json =
@"[{
    ""id"": 2,
    ""components"": {
        ""name"": {""value"":""child-2""}
    }
},{
    ""id"": 1,
    ""children"": [
        2
    ],
    ""components"": {
        ""name"": {""value"":""root""}
    }
}]";
        AreEqual(json, jsonEntities.entities.ToString());
    }
    
    #endregion
    
#region Add DataEntity's to Entity
    /// <summary> Cover <see cref="TreeUtils.AddDataEntitiesToEntity"/> and <see cref="TreeUtils.ReplaceChildrenPids"/></summary>
    [Test]
    public static void Test_TreeUtils_AddDataEntitiesToEntity()
    {
        var store           = new EntityStore(PidType.UsePidAsId);
        var root            = store.CreateEntity(1);
        var child1          = store.CreateEntity(2);
        root.AddChild(child1);
        
        {
            var dataEntity10    = new DataEntity { pid = 10, children = new List<long> { 11 }};
            var dataEntity11    = new DataEntity { pid = 11 };
            var dataEntities    = new [] { dataEntity10, dataEntity11 };
            var result = TreeUtils.AddDataEntitiesToEntity(root, dataEntities);
            
            AreEqual(1,         result.indexes.Count);
            AreEqual(0,         result.errors.Count);
        }
        
        /* {
            var dataEntity10    = new DataEntity { pid = 10, children = new List<long> { 11, 11, 11 }};
            var dataEntity11    = new DataEntity { pid = 11 };
            var dataEntities    = new [] { dataEntity10, dataEntity11 };
            var result = ECSUtils.AddDataEntitiesToEntity(root, dataEntities);
            AreEqual(1,         result.indexes.Count);
            AreEqual(2,         result.addedEntities.Count);
            AreEqual(0,         result.errors.Count);
        } */
    }
    
    [Test]
    public static void Test_TreeUtils_AddDataEntitiesToEntity_errors()
    {
        var store           = new EntityStore(PidType.UsePidAsId);
        var root            = store.CreateEntity(1);
        var child1          = store.CreateEntity(2);
        root.AddChild(child1);
        
        // --- add entity with invalid component
        {
            var json            = new JsonValue("{\n                \"name\": { \"value\": 1 }\n            }");
            var dataEntity10    = new DataEntity { pid = 10, components = json };
            var dataEntities    = new [] { dataEntity10 };
            
            var result = TreeUtils.AddDataEntitiesToEntity(child1, dataEntities);
            
            AreEqual(1,         result.indexes.Count);
            AreEqual(1,         result.errors.Count);
            var error0 = "entity: 10 'components[name]' - Cannot assign number to string. got: 1 path: 'value' at position: 12";
            AreEqual(error0,    result.errors[0]);
        }
        
        // --- add entity with missing 'children' entity
        {
            var dataEntity10    = new DataEntity { pid = 10, children = new List<long> { 99 } };
            var dataEntities    = new [] { dataEntity10 };
            
            var result = TreeUtils.AddDataEntitiesToEntity(child1, dataEntities);
            
            AreEqual(1,         result.indexes.Count);
            AreEqual(1,         result.errors.Count);
            var error0 = "entity: 10 'children' - missing entities: [99]";
            AreEqual(error0,    result.errors[0]);
        }
        
        // --- add entity which reference itself as a child
        {
            var dataEntity10    = new DataEntity { pid = 10, children = new List<long> { 10 } };
            var dataEntities    = new [] { dataEntity10 };
            
            var result = TreeUtils.AddDataEntitiesToEntity(child1, dataEntities);
            
            AreEqual(1,         result.indexes.Count);
            AreEqual(1,         result.errors.Count);
            var error0 = "entity: 10 'children' - entity contains itself as a child.";
            AreEqual(error0,    result.errors[0]);
        }
    }
    #endregion

#region Remove ExplorerItem's
    /// <summary> Cover <see cref="TreeUtils.RemoveExplorerItems"/> </summary>
    [Test]
    public static void Test_TreeUtils_RemoveExplorerItems()
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
        var item2       = tree.GetItemById(2);
        var items = new [] { item2 };
        AreEqual(2,         root.ChildCount);
        AreEqual("child-2", root.ChildEntities[0].Name.value);
     
        // remove child2
        TreeUtils.RemoveExplorerItems(items);
        AreEqual(1,         root.ChildCount);
        AreEqual("child-3", root.ChildEntities[0].Name.value);
        
        // remove - already removed - child2 again
        TreeUtils.RemoveExplorerItems(items);
        AreEqual(1,         root.ChildCount);
        AreEqual("child-3", root.ChildEntities[0].Name.value);
    }
    
    /// <summary> Cover <see cref="TreeUtils.RemoveExplorerItems"/> </summary>
    [Test]
    public static void Test_TreeUtils_Remove_RootItem()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity(1);
        store.SetStoreRoot(root);
        
        var tree        = new ExplorerItemTree(root, "test-tree");
        var items = new [] { tree.RootItem };
        AreEqual(1,         store.Count);
     
        // try remove root item
        TreeUtils.RemoveExplorerItems(items);
        AreEqual(1,         store.Count);
    }
    #endregion
    
#region Move ExplorerItem's
    /// <summary> Cover <see cref="TreeUtils.MoveExplorerItemsUp"/> </summary>
    [Test]
    public static void Test_TreeUtils_MoveExplorerItemsUp()
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
        AreEqual("{ 2, 3 }",   root.ChildIds.Debug());
     
        // move item3 up
        TreeUtils.MoveExplorerItemsUp(items, 1);
        AreEqual("{ 3, 2 }",   root.ChildIds.Debug());
        
        // move item3 up - already on top => index stay unchanged
        TreeUtils.MoveExplorerItemsUp(items, 1);
        AreEqual("{ 3, 2 }",   root.ChildIds.Debug());
    }
    
    /// <summary> Cover <see cref="TreeUtils.MoveExplorerItemsDown"/> </summary>
    [Test]
    public static void Test_TreeUtils_MoveExplorerItemsDown()
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
        AreEqual("{ 2, 3 }",   root.ChildIds.Debug());
     
        // move item2 down
        TreeUtils.MoveExplorerItemsDown(items, 1);
        AreEqual("{ 3, 2 }",   root.ChildIds.Debug());
        
        // move item2 down - already on bottom => index stay unchanged
        TreeUtils.MoveExplorerItemsDown(items, 1);
        AreEqual("{ 3, 2 }",   root.ChildIds.Debug());
    }
    
    [Test]
    public static void Test_TreeUtils_MoveExplorerItems_root()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity(1);
        store.SetStoreRoot(root);
        
        var tree        = new ExplorerItemTree(root, "test-tree");
        
        var items   = new [] { tree.RootItem };
        IsNull(TreeUtils.MoveExplorerItemsUp  (items, 1));
        IsNull(TreeUtils.MoveExplorerItemsDown(items, 1));
    }
    #endregion
}

}