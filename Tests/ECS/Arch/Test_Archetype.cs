using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_Archetype
{
    [Test]
    public static void Test_Archetype_CreateEntity_id_exception()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var type1   = store.GetArchetype(ComponentTypes.Get<Position>());
        
        var e = Throws<ArgumentException>(() => {
            type1.CreateEntity(0);    
        });
        AreEqual("invalid entity id <= 0. was: 0 (Parameter 'id')", e!.Message);
        
        type1.CreateEntity(5);
        e = Throws<ArgumentException>(() => {
            type1.CreateEntity(5);    
        });
        AreEqual("id already in use in EntityStore. id: 5 (Parameter 'id')", e!.Message);
    }
}

}

