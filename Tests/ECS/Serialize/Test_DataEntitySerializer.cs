using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Serialize;

public static class Test_DataEntitySerializer
{
    [Test]
    public static void Test_WriteDataEntity()
    {
        var serializer = new DataEntitySerializer();
        
        var dataEntity = new DataEntity {
            pid         = 10,
            children    = new List<long> { 11 },
            tags        = new List<string> { "test-tag", nameof(TestTag3) },
            components  = new JsonValue("{ \"pos\": { \"x\" : 1 , \"y\" : 2 , \"z\" : 3 } , \"script1\":{\"val1\":10}}")
        };
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
        var json = serializer.WriteDataEntity(dataEntity);
        AreEqual(expect, json);
    }
}