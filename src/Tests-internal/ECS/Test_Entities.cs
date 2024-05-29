using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS {

public static class Test_Entities
{
    [Test]
    public static void Test_Entities_DebugView()
    {
        var store       = new EntityStore();
        var type        = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        var entities    = type.CreateEntities(10);
        AreSame (store, entities.EntityStore);
        AreEqual(10, entities.Count);
        
        var view    = new EntitiesDebugView(entities);
        var array   = view.Entities;
        AreEqual(10, array.Length);
        for (int n = 0; n < 10; n++) {
            var entity = array[n];
            AreSame (store, entity.Store);
            AreEqual(n + 1, entity.Id);   
        }
    }
}

}
