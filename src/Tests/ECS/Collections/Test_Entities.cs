using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Collections {

public static class Test_Entities
{
    [Test]
    public static void Test_Entities_SetStore()
    {
        var store       = new EntityStore();
        var type        = store.GetArchetype(new ComponentTypes());
        var entities    = type.CreateEntities(10);
        AreSame (store, entities.EntityStore);
        AreEqual(10, entities.Count);
        {
            int count = 0;
            foreach (var entity in entities) {
                AreSame (store, entity.Store);
                AreSame (type,  entity.Archetype);
                AreEqual(count + 1, entities[count].Id);
                count++;
                AreEqual(count, entity.Id);
            }
        }
        {
            IEnumerable enumerable = entities;
            IEnumerator enumerator = enumerable.GetEnumerator();
            using var enumerator1 = enumerator as IDisposable;
            int count = 0;
            while (enumerator.MoveNext()) {
                count++;
                var entity = (Entity)enumerator.Current!;
                AreEqual(count, entity.Id);
            }
            AreEqual(10, count);
                
            count = 0;
            enumerator.Reset();
            while (enumerator.MoveNext()) {
                count++;
                var entity = (Entity)enumerator.Current!;
                AreEqual(count, entity.Id);
            }
            AreEqual(10, count);
        }
        {
            IEnumerable<Entity> enumerable = entities;
            using var enumerator = enumerable.GetEnumerator();
            int count = 0;
            while (enumerator.MoveNext()) {
                count++;
                AreEqual(count, enumerator.Current.Id);
            }
            AreEqual(10, count);
        }
    }
}

}
