using System.IO;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using static NUnit.Framework.Assert;


// ReSharper disable MethodHasAsyncOverload
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Serialize {

public static class Test_SerializeEntity
{
    /// referenced entity is loaded before entity reference
    private const string JsonEntityRefPresent =
@"[{
    ""id"": 1002,
    ""components"": {
        ""name"": {""value"":""test""}
    }
},{
    ""id"": 1,
    ""components"": {
        ""EntityReference"": {""entity"":1002}
    }
}]";
    
    /// referenced entity is loaded after entity reference
    private const string JsonEntityRefNotPresent =
@"[{
    ""id"": 1,
    ""components"": {
        ""EntityReference"": {""entity"":1002}
    }
},{
    ""id"": 1002,
    ""components"": {
        ""name"": {""value"":""test""}
    }
}]";
    
    private const string JsonEntityRefMissing =
@"[{
    ""id"": 1,
    ""components"": {
        ""EntityReference"": {""entity"":1002}
    }
}]";
    
    private const string JsonEntityRefNull =
@"[{
    ""id"": 1,
    ""components"": {
        ""EntityReference"": {""entity"":null}
    }
}]";
    
    private const string JsonInvalidId =
        @"[{
    ""id"": 1,
    ""components"": {
        ""EntityReference"": {""entity"":""xxx""}
    }
}]";
    
    
    #region read Entity
    [Test]
    public static void Test_SerializeEntity_read_present()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(JsonEntityRefPresent);
        
        var result = serializer.ReadIntoStore(store, stream); // referenced EntityReference.entity 1002 is already deserialized
        IsNull(result.error);
        
        AreEqual(2, store.Count);
        
        var entity1 = store.GetEntityById(1);
        AreEqual(1002, entity1.GetComponent<EntityReference>().entity.Id);
        
        Entity entityRef = store.GetEntityById(1002);
        IsFalse(entityRef.IsNull);
    }
    
    [Test]
    public static void Test_SerializeEntity_read_missing()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(JsonEntityRefMissing);
        
        var result = serializer.ReadIntoStore(store, stream);
        IsNull(result.error);
        
        AreEqual(1, store.Count);
        
        var entity1 = store.GetEntityById(1);
        var entityRef = entity1.GetComponent<EntityReference>().entity;
        AreEqual(1002,entityRef.Id);
        IsTrue(       entityRef.IsNull);
    }
    
    [Test]
    public static void Test_SerializeEntity_read_not_present()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(JsonEntityRefNotPresent);
        
        var result = serializer.ReadIntoStore(store, stream);
        IsNull(result.error);
        
        AreEqual(2, store.Count);
        
        var entity1 = store.GetEntityById(1);
        var entityRef = entity1.GetComponent<EntityReference>().entity;
        AreEqual(1002,entityRef.Id);
        IsFalse(      entityRef.IsNull);
    }
    
    [Test]
    public static void Test_SerializeEntity_read_null()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(JsonEntityRefNull);
        
        var result = serializer.ReadIntoStore(store, stream);
        IsNull(result.error);
        
        AreEqual(1, store.Count);
        
        var entity1 = store.GetEntityById(1);
        var entityRef = entity1.GetComponent<EntityReference>().entity;
        AreEqual(0, entityRef.Id);
        
        IsTrue(entityRef.IsNull);
    }
    
    [Test]
    public static void Test_SerializeEntity_read_invalid_id()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(JsonInvalidId);
        
        var result = serializer.ReadIntoStore(store, stream);
        AreEqual("'components[EntityReference]' - Cannot assign string to Entity. got: 'xxx' path: 'entity' at position: 15 path: '[0]' at position: 87", result.error);
    }
    #endregion
    
#region write Entity
    [Test]
    public static void Test_SerializeEntity_write()
    {
        var store = new EntityStore();
        Entity entity1   = store.CreateEntity(1);
        Entity entityRef = store.CreateEntity(1002);
        entity1.AddComponent(new EntityReference { entity = entityRef });
        
        var serializer = new EntitySerializer();
        using MemoryStream writeStream = new MemoryStream();
        serializer.WriteEntities(new []{entity1}, writeStream);
        
        var str = Test_Serializer.MemoryStreamAsString(writeStream);
        AreEqual(JsonEntityRefMissing, str);
    }
    
    [Test]
    public static void Test_SerializeEntity_write_null()
    {
        var store = new EntityStore();
        Entity entity1   = store.CreateEntity(1);
        Entity entityRef = store.CreateEntity(1002);
        entity1.AddComponent(new EntityReference { entity = entityRef });
        entityRef.DeleteEntity();
        
        var serializer = new EntitySerializer();
        using MemoryStream writeStream = new MemoryStream();
        serializer.WriteEntities(new []{entity1}, writeStream);
        
        var str = Test_Serializer.MemoryStreamAsString(writeStream);
        AreEqual(JsonEntityRefNull, str);
    }
    #endregion
}

}