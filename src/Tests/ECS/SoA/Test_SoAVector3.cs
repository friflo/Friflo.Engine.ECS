using System;
using System.Numerics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using Tests.ECS;
using Tests.Examples;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable ConvertToLocalFunction
// ReSharper disable EqualExpressionComparison
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.SoA {

public static class Test_SoAVector3
{
    [Test]
    public static void Test_SoAVector3_SoA_Add_Remove_Components()
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
                case 1: AreEqual(pos1.value, changed.Component<Pos3SoA>().value) ; break;
                case 2: AreEqual(pos2.value, changed.Component<Pos3SoA>().value); break;
            }
        };
        store.OnComponentRemoved += changed => {
            switch (changed.EntityId) {
                case 1:
                    AreEqual(pos1.value, changed.OldComponent<Pos3SoA>().value);
                    AreEqual(pos1.value, ((Pos3SoA)changed.DebugOldComponent).value);
                    break;
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
    
    /// Test <see cref="StructSoAVector3{T}.CopyComponentTo"/>
    [Test]
    public static void Test_SoAVector3_SoA_Add_Tag()
    {
        var store = new EntityStore();
        var pos1 = new Pos3SoA { value = new Vector3(11, 12, 13) };
        var pos2 = new Pos3SoA { value = new Vector3(21, 22, 23) };
        var pos3 = new Pos3SoA { value = new Vector3(31, 32, 33) };
        var entity1 = store.CreateEntity(pos1);
        var entity2 = store.CreateEntity(pos2);
        var entity3 = store.CreateEntity(pos3);
        
        entity1.AddTag<General.MyTag1>();
        entity2.AddTag<General.MyTag2>();
        
        AreEqual(pos1.value, entity1.GetSoA<Pos3SoA>().value);
        AreEqual(pos2.value, entity2.GetSoA<Pos3SoA>().value);
        AreEqual(pos3.value, entity3.GetSoA<Pos3SoA>().value);
    }

    /// Test <see cref="StructSoAVector3{T}.ResizeComponents"/>
    [Test]
    public static void Test_SoAVector3_SoA_Check_ResizeComponents()
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
    public static void Test_SoAVector3_SoA_Entity_exceptions()
    {
        var store = new EntityStore();
        var entitySoA = store.CreateEntity(new Pos3SoA());
        var entityAoS = store.CreateEntity(new Position());
        
        var e = Throws<InvalidOperationException>(() => {
            entityAoS.GetSoA<Position>();
        });
        AreEqual("Component 'Position' is stored as AoS. GetSoA() requires SoA storage. Add [SoA] attribute or use GetComponent() instead.", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            entitySoA.GetComponent<Pos3SoA>();
        });
        AreEqual("Component 'Pos3SoA' is stored as SoA. GetComponent() requires AoS storage. Remove attribute [SoA] or use GetSoA() instead.", e!.Message);
    }
    
    /// Test <see cref="StructSoAVector3{T}.Write"/>
    /// Test <see cref="StructSoAVector3{T}.Read"/>
    [Test]
    public static void Test_SoAVector3_SoA_serialize()
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
    
    /// Test <see cref="StructSoAVector3{T}.GetComponentMember"/>
    [Test]
    public static void Test_SoAVector3_SoA_GetEntityComponentMember()
    {
        var store  = new EntityStore();
        var pos = new Pos3SoA { value = new Vector3(11, 12, 13) };
        var entity = store.CreateEntity(pos);
        var componentType   = typeof(Pos3SoA);
        var pos3Info = MemberPath.Get(componentType, nameof(Pos3SoA.value));
        IsTrue(EntityUtils.GetEntityComponentMember<Vector3>(entity, pos3Info, out var vector3, out _));
        
        AreEqual(new Vector3(11, 12, 13),           vector3);
        AreEqual("value",                           pos3Info.path);
        AreEqual("value",                           pos3Info.name);
        AreEqual(typeof(Vector3),                   pos3Info.memberType);
        AreEqual(typeof(Pos3SoA),                   pos3Info.declaringType);
        AreEqual(typeof(Pos3SoA),                   pos3Info.componentType.Type);
        AreEqual("get: (Pos3SoA => value)",         pos3Info.getter.Method.Name);
        AreEqual("set: (Pos3SoA => value)",         pos3Info.setter.Method.Name);
        NotNull ("value",                           pos3Info.memberInfo.Name);
        AreEqual("Pos3SoA value : Vector3",         pos3Info.ToString());
    }
    
    /// Test <see cref="StructSoAVector3{T}.SetComponentMember"/>
    [Test]
    public static void Test_SoAVector3_SoA_SetEntityComponentMember()
    {
        var store  = new EntityStore();
        var pos = new Pos3SoA { value = new Vector3(11, 12, 13) };
        var entity = store.CreateEntity(pos);
        var componentType   = typeof(Pos3SoA);
        var nameInfo = MemberPath.Get(componentType, nameof(Pos3SoA.value));
        
        var newPos = new Vector3(101, 102, 103);
        OnMemberChanged<Pos3SoA> handler = (ref Pos3SoA value, Entity entity1, string path, in Pos3SoA old) => {
            AreEqual(1,         entity1.Id);
            AreEqual("value",   path);
            AreEqual(newPos,    value.value);
            AreEqual(pos.value, old.value);
        };
        IsTrue(EntityUtils.SetEntityComponentMember(entity, nameInfo, newPos, handler, out _));
        var result = entity.GetSoA<Pos3SoA>();
        AreEqual(newPos, result.value);
    }
    
    /// Test <see cref="StructSoAVector3{T}.CopyComponent"/>
    [Test]
    public static void Test_SoAVector3_SoA_CopyEntity()
    {
        var store       = new EntityStore();
        var pos         = new Pos3SoA { value = new Vector3(11, 12, 13) };
        var srcEntity   = store.CreateEntity(pos);
        
        var target  = new EntityStore();
        var targetEntity = target.CreateEntity();
        srcEntity.CopyEntity(targetEntity);
        
        AreEqual(pos.value, targetEntity.GetSoA<Pos3SoA>().value);
    }
    
    /// Test <see cref="StructSoAVector3{T}.SetBatchComponent"/>
    [Test]
    public static void Test_SoAVector3_SoA_Batch_CreateEntity()
    {
        var store = new EntityStore();
        var batch = new CreateEntityBatch(store);
        var pos = new Pos3SoA { value = new Vector3(11, 12, 13) };
        batch.Add(pos);
        var entity = batch.CreateEntity();
        
        AreEqual(pos.value, entity.GetSoA<Pos3SoA>().value);
    }
    
    /// Test <see cref="StructSoAVector3{T}.GetComponentDebug"/>
    [Test]
    public static void Test_SoAVector3_SoA_GetEntityComponent()
    {
        var store = new EntityStore();
        var pos         = new Pos3SoA { value = new Vector3(11, 12, 13) };
        var entity = store.CreateEntity(pos);
        var schema          = EntityStore.GetEntitySchema();
        var componentType   = schema.ComponentTypeByType[typeof(Pos3SoA)];
        
        var result = (Pos3SoA)EntityUtils.GetEntityComponent(entity, componentType);
        AreEqual(pos.value, result.value);
    }
    
    /// Test <see cref="StructSoAVector3{T}.SetComponentsDefault"/>
    [Test]
    public static void Test_SoAVector3_SoA_Archetype_CreateEntities()
    {
        var store = new EntityStore();
        var pos         = new Pos3SoA { value = new Vector3(11, 12, 13) };
        var entity1 = store.CreateEntity(pos);
        var entity2 = store.CreateEntity(pos);
        var archetype = entity1.Archetype;
        
        entity1.DeleteEntity();
        entity2.DeleteEntity();
        
        var entities = archetype.CreateEntities(2);
        foreach (var entity in entities) {
            AreEqual(new Vector3(), entity.GetSoA<Pos3SoA>().value);
        }
    }
    
    /// Test <see cref="Chunk{T}.GetLanesSoA"/>
    [Test]
    public static void Test_SoAVector3_Query_Lanes()
    {
        var store = new EntityStore();
        for (int n = 0; n < 1000; n++) {
            store.CreateEntity(new Pos3SoA { value = new Vector3(n * 10, n * 20, n * 30) });
        }
        var query = store.Query<Pos3SoA>();
        int count = 0;
        foreach (var (positions, entities) in query.Chunks) {
            var lanes = positions.GetLanesSoA();
            var stride = positions.GetStrideSoA();
            AreEqual(3096, lanes.Length);
            AreEqual(1032, stride);
            for (int i = 0; i < entities.Length; i++) {
                var value = positions.GetSoA(i);
                var expect = new Vector3(i * 10, i * 20, i * 30);
                That(value.value, Is.EqualTo(expect));
                
                expect += new Vector3(100, 200, 300);
                value.value = expect;
                positions.SetSoA(i, value);
                value = positions.GetSoA(i);
                That(value.value, Is.EqualTo(expect));
            }
            count++;
            var e = Throws<InvalidOperationException>(() => { _ = positions.Span; });
            AreEqual("Expect call for regular component data.", e!.Message);
            
            e = Throws<InvalidOperationException>(() => { _ = positions[0]; });
            AreEqual("Expect call for regular component data.", e!.Message);
            
            e = Throws<InvalidOperationException>(() => { _ = positions.ArchetypeComponents; });
            AreEqual("Expect call for regular component data.", e!.Message);
            
        }
        AreEqual(1, count);
    }
    
    [Test]
    public static void Test_SoAVector3_Chunk_exceptions()
    {
        var store = new EntityStore();
        store.CreateEntity(new Position());

        var query = store.Query<Position>();
        int count = 0;
        foreach (var (positions, entities) in query.Chunks) {
            var e = Throws<InvalidOperationException>(() => { positions.GetLanesSoA(); });
            AreEqual("Expect call for SoA component data.", e!.Message);
            
            e = Throws<InvalidOperationException>(()    => { positions.GetStrideSoA(); });
            AreEqual("Expect call for SoA component data.", e!.Message);
            
            e = Throws<InvalidOperationException>(()    => { positions.GetSoA(0); });
            AreEqual("Expect call for SoA component data.", e!.Message);
            
            e = Throws<InvalidOperationException>(()    => { positions.SetSoA(0, default); });
            AreEqual("Expect call for SoA component data.", e!.Message);
            
            count++;
        }
        AreEqual(1, count);
    }
}

}
