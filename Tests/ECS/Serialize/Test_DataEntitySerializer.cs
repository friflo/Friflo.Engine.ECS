using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Serialize;

public static class Test_DataEntitySerializer
{
    [Test]
    public static void Test_WriteDataEntity()
    {
        var serializer = new DataEntitySerializer();
        
        var dataEntity = new DataEntity { pid = 10 };
        
        // --- write entity containing only an id
        var json = serializer.WriteDataEntity(dataEntity, out _);
        AreEqual("{\n    \"id\": 10\n}", json);
        
        // --- write entity containing children, tags and components
        dataEntity.children    = new List<long> { 11 };
        dataEntity.tags        = new List<string> { "test-tag", nameof(TestTag3) };
        dataEntity.components  = new JsonValue("{ \"pos\": { \"x\" : 1 , \"y\" : 2 , \"z\" : 3 } , \"script1\":{\"val1\":10}}");
        
        var expect =
"""
{
    "id": 10,
    "children": [
        11
    ],
    "components": {
        "pos": {"x":1,"y":2,"z":3},
        "script1": {"val1":10}
    },
    "tags": [
        "test-tag",
        "TestTag3"
    ]
}
""";
        json = serializer.WriteDataEntity(dataEntity, out _);
        AreEqual(expect, json);
    }
    
    [Test]
    public static void Test_WriteDataEntity_errors()
    {
        var serializer = new DataEntitySerializer();
        
        var dataEntity = new DataEntity { pid = 1, components  = new JsonValue("1") };
        string error;
        
        serializer.WriteDataEntity(dataEntity, out error);
        AreEqual("expect 'components' == object or null. was: ValueNumber", error);
        
        dataEntity.components = new JsonValue("");
        serializer.WriteDataEntity(dataEntity, out error);
        AreEqual("components error: unexpected EOF on root path: '(root)' at position: 0", error);
        
        
        
        
    }
}