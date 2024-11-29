using System.IO;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using Tests.ECS.Relations;
using Tests.ECS.Serialize;
using static NUnit.Framework.Assert;


// ReSharper disable InconsistentNaming
namespace Internal.ECS {

public static class Test_SerializeLinkRelations
{
   
// referenced entity 1002 is loaded after loading its LinkRelation
// The fields of store.nodes[1002] .isLinked & .isOwner are assigned
private const string JsonLinkRelation =
@"[{
    ""id"": 1,
    ""components"": {
        ""multi-attack"": [{""speed"":0,""target"":1002}]
    }
},{
    ""id"": 1002
}]";

private const string JsonMissingLinkRelation =
    @"[{
    ""id"": 1,
    ""components"": {
        ""multi-attack"": [{""speed"":0,""target"":1003}]
    }
}]";
    
    
#region read relations
  
    [Test]
    public static void Test_SerializeLinkRelations_read_LinkRelation()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(JsonLinkRelation);
        
        var result = serializer.ReadIntoStore(store, stream);
        IsNull(result.error);
        
        var entity1 = store.GetEntityById(1);
        var entity2 = store.GetEntityById(1002);
        var relations1 = entity1.GetRelations<AttackRelation>();
        AreEqual(1,     relations1.Length);
        AreEqual(1002 , relations1[0].target.Id);
        
        var incomingLinks = entity2.GetIncomingLinks<AttackRelation>();
        var attackBit = 1 << StructInfo<AttackRelation>.Index;
        AreEqual(attackBit, store.nodes[1].isOwner);
        AreEqual(attackBit, store.nodes[1002].isLinked);
        
        AreEqual(1, incomingLinks.Count);
        AreEqual(1, incomingLinks[0].Entity.Id);

        entity1.DeleteEntity();
        AreEqual(0, store.nodes[1].isOwner);
        // AreEqual(0, store.nodes[1002].isLinked);
         
        incomingLinks = entity2.GetIncomingLinks<AttackRelation>();
        AreEqual(0, incomingLinks.Count);
    }
    
    [Test]
    public static void Test_SerializeLinkRelations_read_MissingLinkRelation()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(JsonMissingLinkRelation);
        
        var result = serializer.ReadIntoStore(store, stream);
        IsNull(result.error);
        
        var entity1 = store.GetEntityById(1);
        var relations1 = entity1.GetRelations<AttackRelation>();
        AreEqual(1,     relations1.Length);
        AreEqual(1003 , relations1[0].target.Id);
        
        var entity2 = store.GetEntityById(1003);
        IsTrue(entity2.IsNull);
    }
    #endregion

    
#region write relations
    [Test]
    public static void Test_SerializeLinkRelations_write_LinkRelation()
    {
        var store = new EntityStore();
        Entity entity1   = store.CreateEntity(1);
        Entity entity2   = store.CreateEntity(1002);
        
        entity1.AddRelation(new AttackRelation { target = entity2 });
        
        var serializer = new EntitySerializer();
        using MemoryStream writeStream = new MemoryStream();
        serializer.WriteEntities(new []{ entity1, entity2 }, writeStream);
        
        var str = Test_Serializer.MemoryStreamAsString(writeStream);
        AreEqual(JsonLinkRelation, str);
    }
    #endregion
}

}