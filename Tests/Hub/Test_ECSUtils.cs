// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable InconsistentNaming
namespace Tests.Hub;


public static class Test_ECSUtils
{
#region JsonArrayToDataEntities()
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
    
    
#region DuplicateEntities()
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
        root.AddChild(child3);
        AreEqual(2, root.ChildCount);
        
        var entities    = new List<Entity>{ child2, child3 };
        
        var indexes     = ECSUtils.DuplicateEntities(entities);
        
        AreEqual(4,             root.ChildCount);
        
        var clone2 =            root.ChildEntities[indexes[0]];
        AreEqual("child-2",     clone2.Name.value);
        
        var clone3 =            root.ChildEntities[indexes[1]];
        AreEqual("child-3",     clone3.Name.value);
    }
    #endregion
}