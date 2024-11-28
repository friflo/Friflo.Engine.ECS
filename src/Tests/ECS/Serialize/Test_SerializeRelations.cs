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
        ""IntRelation"": [{""value"":101}]
    }
},{
    ""id"": 2,
    ""components"": {
        ""IntRelation"": [{""value"":201},{""value"":202}]
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
    #endregion

    
#region write relations
    [Test]
    public static void Test_SerializeRelations_write()
    {
        var store = new EntityStore();
        Entity entity1   = store.CreateEntity(1);
        entity1.AddRelation(new IntRelation { value = 101 });
        
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