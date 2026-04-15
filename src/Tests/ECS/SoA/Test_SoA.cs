using System;
using System.Numerics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable EqualExpressionComparison
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.SoA {

public static class Test_SoA
{
    [Test]
    public static void Test_SoA_Add_Remove_Components()
    {
        var store = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        
        var pos1 = new Pos3SoA { value = new Vector3(11, 12, 13) };
        var pos2 = new Pos3SoA { value = new Vector3(21, 22, 23) };
        var pos3 = new Pos3SoA { value = new Vector3(31, 32, 33) };
        store.OnComponentAdded += changed => {
            switch (changed.EntityId) {
                case 1: AreEqual(changed.Component<Pos3SoA>().value, pos1.value); break;
                case 2: AreEqual(changed.Component<Pos3SoA>().value, pos2.value); break;
            }
        };
        store.OnComponentRemoved += changed => {
            switch (changed.EntityId) {
                case 1: AreEqual(changed.OldComponent<Pos3SoA>().value, pos1.value); break;
            }
        };
        
        entity1.AddComponent(pos1);
        var result1 = entity1.GetSoA<Pos3SoA>();
        AreEqual(result1.value, pos1.value);
        
        entity2.AddComponent(pos2);
        var result2 = entity2.GetSoA<Pos3SoA>();
        AreEqual(result2.value, pos2.value);
        
        entity3.AddComponent(pos3);
        var result3 = entity3.GetSoA<Pos3SoA>();
        AreEqual(result3.value, pos3.value);
        
        entity1.RemoveComponent<Pos3SoA>();
        
        result2 = entity2.GetSoA<Pos3SoA>();
        AreEqual(result2.value, pos2.value);
        
        result3 = entity3.GetSoA<Pos3SoA>();
        AreEqual(result3.value, pos3.value);
        
        IsTrue(entity3.TryGetComponent(out Pos3SoA tryGet));
        AreEqual(tryGet.value, pos3.value);
    }

    /// Test <see cref="StructFloatSoA{T}.ResizeComponents"/>
    [Test]
    public static void Test_SoA_Check_ResizeComponents()
    {
        var store = new EntityStore();
        store.ShrinkRatioThreshold = 0;
        for (int n = 1; n <= 2000; n++)
        {
            {
                var pos = new Pos3SoA { value = new Vector3(n,  n + 10_000, n + 20_000) };
                store.CreateEntity(pos);
            }
            if (n % 100 == 0) {
                for (int i = 1; i < n; i++)
                {
                    var entity = store.GetEntityById(i);
                    var pos = entity.GetSoA<Pos3SoA>();
                    var expect = new Vector3(i,  i + 10_000, i + 20_000);
                    AreEqual(expect, pos.value);
                }
            }
        }
        var query = store.Query<Pos3SoA>();
        for (int n = 1; n <= 2000; n++)
        {
            if (n % 2 == 0) {
                var entity = store.GetEntityById(n);
                entity.DeleteEntity();
            }
            if (n % 200 == 0) {
                foreach (var entity in query.Entities)
                {
                    var id = entity.Id;
                    var pos = entity.GetSoA<Pos3SoA>();
                    var expect = new Vector3(id,  id + 10_000, id + 20_000);
                    AreEqual(expect, pos.value);
                }
            }
        }
        var entities = query.Entities.ToEntityList();
        int counter = 0;
        foreach (var removeEntity in entities)
        {
            removeEntity.DeleteEntity();
            if (counter++ % 200 == 0) {
                foreach (var entity in query.Entities)
                {
                    var id = entity.Id;
                    var pos = entity.GetSoA<Pos3SoA>();
                    var expect = new Vector3(id,  id + 10_000, id + 20_000);
                    AreEqual(expect, pos.value);
                }
            }
        }
        AreEqual(0, query.Count);
    }
    
    [Test]
    public static void Test_SoA_Entity_exceptions()
    {
        var store = new EntityStore();
        var entitySoA = store.CreateEntity(new Pos3SoA());
        var entityAoS = store.CreateEntity(new Position());
        
        var e = Throws<InvalidOperationException>(() => {
            entityAoS.GetSoA<Position>();
        });
        AreEqual("Component 'Position' is stored as AoS. GetSoA() requires SoA storage. Add [SoA] attribute or use GetComponent() instead.", e.Message);
        
        e = Throws<InvalidOperationException>(() => {
            entitySoA.GetComponent<Pos3SoA>();
        });
        AreEqual("Component 'Pos3SoA' is stored as SoA. GetComponent() requires AoS storage. Remove attribute [SoA] or use GetSoA() instead.", e.Message);
    }
    
    [Test]
    public static void Test_SoA_serialize()
    {
        var store = new EntityStore();
        var pos = new Pos3SoA { value = new Vector3(11, 12, 13) };
        var entity = store.CreateEntity(pos);
        
        EntityConverter converter = new EntityConverter();
        var dataEntity = converter.EntityToDataEntity(entity, null, false);
        AreEqual("{\"Pos3SoA\":{\"value\":{\"X\":11,\"Y\":12,\"Z\":13}}}", dataEntity.components.ToString());
        
        var targetStore = new EntityStore();
        var targetEntity = converter.DataEntityToEntity(dataEntity, targetStore, out var error);
        AreEqual(pos.value, targetEntity.GetSoA<Pos3SoA>().value);
        IsNull(error);
    }
}

}
