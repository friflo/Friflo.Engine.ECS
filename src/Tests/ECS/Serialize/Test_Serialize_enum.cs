using System.Collections.Generic;
using System.IO;
using System.Text;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using static NUnit.Framework.Assert;


// ReSharper disable InconsistentNaming
namespace Tests.ECS.Serialize {
    
enum MyEnum { two = 2 }

struct EnumComp : IComponent
{
    public MyEnum myEnum;
}

public static class Test_Serialize_enum
{
    // https://github.com/friflo/Friflo.Engine.ECS/issues/35   
    [Test]
    public static void Test_Serialize_enum_component()
    {
        var store = new EntityStore();
        Entity entity = store.CreateEntity(1);
        entity.AddComponent(new EnumComp { myEnum = MyEnum.two });
        
        var serializer = new EntitySerializer();
        
        using MemoryStream writeStream = new MemoryStream();
        var entities = new List<Entity> { entity };
        serializer.WriteEntities(entities, writeStream);
        
        writeStream.Position = 0;
        using StreamReader reader = new(writeStream);
        string json = reader.ReadToEnd();
        var expect =
@"[{
    ""id"": 1,
    ""components"": {
        ""EnumComp"": {""myEnum"":""two""}
    }
}]";
        AreEqual(expect, json);
        
        var newStore = new EntityStore();
        var readStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        serializer.ReadIntoStore(newStore, readStream);
        var newEntity = newStore.GetEntityById(1);
        AreEqual(MyEnum.two, newEntity.GetComponent<EnumComp>().myEnum);
    }
}

}