using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using Tests.ECS.Index;
using Tests.ECS.Serialize;
using static NUnit.Framework.Assert;


// ReSharper disable InconsistentNaming
namespace Internal.ECS {

public static class Test_SerializeLinkComponent
{
   
// referenced entity 1002 is loaded after loading its LinkRelation
// The fields of store.nodes[1002] .isLinked & .isOwner are assigned
private const string Json =
@"[{
    ""id"": 1,
    ""components"": {
        ""LinkComponent"": {""entity"":1002}
    }
},{
    ""id"": 1002
}]";


    [Test]
    public static void Test_SerializeEntity_read_present()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        var stream      = Test_Serializer.StringAsStream(Json);
        
        var result = serializer.ReadIntoStore(store, stream); // referenced EntityReference.entity 1002 is already deserialized
        IsNull(result.error);
        
        AreEqual(2, store.Count);
        
        var entity1 = store.GetEntityById(1);
        AreEqual(1002, entity1.GetComponent<LinkComponent>().entity.Id);
        
        var attackBit = 1 << StructInfo<LinkComponent>.Index;
        AreEqual(attackBit, store.nodes[1].isOwner);
        AreEqual(attackBit, store.nodes[1002].isLinked);
        
        Entity entityRef = store.GetEntityById(1002);
        IsFalse(entityRef.IsNull);
        
        var incomingLinks = entityRef.GetIncomingLinks<LinkComponent>();
        AreEqual(1, incomingLinks.Count);
        AreEqual(1, incomingLinks[0].Entity.Id);
        
        entityRef.DeleteEntity();
        
        IsFalse(entity1.HasComponent<LinkComponent>());
    }

}

}