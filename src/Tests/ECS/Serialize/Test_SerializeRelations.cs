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
        ""IntRelation"": [{""value"":101},{""value"":102}]
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
        var relations = entity1.GetRelations<IntRelation>();
        AreEqual(2, relations.Length);
    }
    #endregion

    
#region write relations
    [Test]
    public static void Test_SerializeRelations_write()
    {
        var store = new EntityStore();
        Entity entity1   = store.CreateEntity(1);
        entity1.AddRelation(new IntRelation { value = 101});
        entity1.AddRelation(new IntRelation { value = 102});
        
        var serializer = new EntitySerializer();
        using MemoryStream writeStream = new MemoryStream();
        serializer.WriteEntities(new []{entity1}, writeStream);
        
        var str = Test_Serializer.MemoryStreamAsString(writeStream);
        AreEqual(Json, str);
    }
    #endregion
}

}