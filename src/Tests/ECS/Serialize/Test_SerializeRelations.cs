using System.IO;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using Tests.ECS.Relations;
using static NUnit.Framework.Assert;


// ReSharper disable InconsistentNaming
namespace Tests.ECS.Serialize {

public static class Test_SerializeRelations
{
    private const string Json =
@"[{
    ""id"": 1,
    ""components"": {
        ""name"": {""value"":""test""},
        ""IntRelation"": [{""value"":101}]
    }
},{
    ""id"": 2,
    ""components"": {
        ""IntRelation"": [{""value"":201},{""value"":202}]
    }
}]";
    
    private const string JsonEmptyRelations =
        @"[{
    ""id"": 1,
    ""components"": {
        ""IntRelation"": []
    }
}]";
    
    private const string JsonInvalidEntityId =
        @"[{
    ""id"": 1,
    ""components"": {
        ""IntRelation"": [{""value"":""abc""}]
    }
}]";
    
    private const string JsonError =
@"[{
    ""id"": 1,
    ""components"": {
        ""IntRelation"": [1]
    }
}]";
    
    private const string JsonInvalidRelationsArray =
        @"[{
    ""id"": 1,
    ""components"": {
        ""IntRelation"": [x]
    }
}]";
    
    
#region read relations
    [Test]
    public static void Test_SerializeRelations_read()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(Json);
        
        var result = serializer.ReadIntoStore(store, stream);
        IsNull(result.error);
        
        var entity1 = store.GetEntityById(1);
        var relations1 = entity1.GetRelations<IntRelation>();
        AreEqual(1,     relations1.Length);
        AreEqual(101,   relations1[0].value);
        
        var entity2 = store.GetEntityById(2);
        var relations2 = entity2.GetRelations<IntRelation>();
        AreEqual(2,     relations2.Length);
        AreEqual(201,   relations2[0].value);
        AreEqual(202,   relations2[1].value);
    }
    
    [Test]
    public static void Test_SerializeRelations_read_empty_relations()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(JsonEmptyRelations);
        
        var result = serializer.ReadIntoStore(store, stream);
        IsNull(result.error);
        
        var entity1 = store.GetEntityById(1);
        var relations1 = entity1.GetRelations<IntRelation>();
        AreEqual(0,     relations1.Length);
    }
    
    [Test]
    public static void Test_SerializeRelations_read_invalid_entity_id()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(JsonInvalidEntityId);
        
        var result = serializer.ReadIntoStore(store, stream);
        AreEqual("'components[IntRelation]' - Cannot assign string to int. got: 'abc' path: 'value' at position: 14 path: '[0]' at position: 84", result.error);
    }
    
    [Test]
    public static void Test_SerializeRelations_read_error()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(JsonError);
        
        var result = serializer.ReadIntoStore(store, stream);
        AreEqual("'components' member expect array of objects. was ValueNumber. id: 1, component: 'IntRelation' path: '[0]' at position: 70", result.error);
    }
    
    [Test]
    public static void Test_SerializeRelations_read_invalid_JSON()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(JsonInvalidRelationsArray);
        
        var result = serializer.ReadIntoStore(store, stream);
        AreEqual("unexpected character while reading value. Found: x path: '[0].components.IntRelation[0]' at position: 61", result.error);
    }
    #endregion

    
#region write relations
    [Test]
    public static void Test_SerializeRelations_write()
    {
        var store = new EntityStore();
        Entity entity1   = store.CreateEntity(1);
        entity1.AddRelation(new IntRelation { value = 101 });
        entity1.AddComponent(new EntityName("test"));
        
        Entity entity2   = store.CreateEntity(2);
        entity2.AddRelation(new IntRelation { value = 201 });
        entity2.AddRelation(new IntRelation { value = 202 });
        
        var serializer = new EntitySerializer();
        using MemoryStream writeStream = new MemoryStream();
        serializer.WriteEntities(new []{ entity1, entity2 }, writeStream);
        
        var str = Test_Serializer.MemoryStreamAsString(writeStream);
        AreEqual(Json, str);
    }
    #endregion
}

}