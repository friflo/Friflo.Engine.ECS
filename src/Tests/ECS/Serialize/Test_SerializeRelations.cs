using System.IO;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using Tests.ECS.Relations;
using static NUnit.Framework.Assert;


// ReSharper disable MethodHasAsyncOverload
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Serialize {

public static class Test_SerializeRelations
{
    /// referenced entity is loaded before entity reference
    private const string Json =
@"[{
    ""id"": 1,
    ""components"": {
        ""IntRelation"": [{""value"":101},{""value"":102}]
    }
}]";
    

    
#region write Entity
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