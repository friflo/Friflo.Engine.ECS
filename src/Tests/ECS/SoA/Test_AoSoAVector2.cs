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

public static class Test_AoSoAVector2
{
    [Test]
    public static void Test_AoSoAVector2_SoA_Add_Remove_Components()
    {
        var store = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        
        var pos1 = new Pos2AoSoA { value = new Vector2(11, 12) };
        var pos2 = new Pos2AoSoA { value = new Vector2(21, 22) };
        var pos3 = new Pos2AoSoA { value = new Vector2(31, 32) };
        store.OnComponentAdded += changed => {
            switch (changed.EntityId) {
                case 1: AreEqual(pos1.value, changed.Component<Pos2AoSoA>().value) ; break;
                case 2: AreEqual(pos2.value, changed.Component<Pos2AoSoA>().value); break;
            }
        };
        store.OnComponentRemoved += changed => {
            switch (changed.EntityId) {
                case 1:
                    AreEqual(pos1.value, changed.OldComponent<Pos2AoSoA>().value);
                    AreEqual(pos1.value, ((Pos2AoSoA)changed.DebugOldComponent).value);
                    break;
            }
        };
        
        entity1.AddComponent(pos1);
        var result1 = entity1.GetSoA<Pos2AoSoA>();
        AreEqual(result1.value, pos1.value);
        
        entity2.AddComponent(pos2);
        var result2 = entity2.GetSoA<Pos2AoSoA>();
        AreEqual(result2.value, pos2.value);
        
        entity3.AddComponent(pos3);
        var result3 = entity3.GetSoA<Pos2AoSoA>();
        AreEqual(result3.value, pos3.value);
        
        entity1.RemoveComponent<Pos2AoSoA>();
        
        result2 = entity2.GetSoA<Pos2AoSoA>();
        AreEqual(result2.value, pos2.value);
        
        result3 = entity3.GetSoA<Pos2AoSoA>();
        AreEqual(result3.value, pos3.value);
        
        IsTrue(entity3.TryGetComponent(out Pos2AoSoA tryGet));
        AreEqual(tryGet.value, pos3.value);
    }
    
    /// Test <see cref="StructSoAVector4{T}.CopyComponentTo"/>
    [Test]
    public static void Test_AoSoAVector2_SoA_Add_Tag()
    {
        var store = new EntityStore();
        var pos1 = new Pos2AoSoA { value = new Vector2(11, 12) };
        var pos2 = new Pos2AoSoA { value = new Vector2(21, 22) };
        var pos3 = new Pos2AoSoA { value = new Vector2(31, 32) };
        var entity1 = store.CreateEntity(pos1);
        var entity2 = store.CreateEntity(pos2);
        var entity3 = store.CreateEntity(pos3);
        
        entity1.AddTag<General.MyTag1>();
        entity2.AddTag<General.MyTag2>();
        
        AreEqual(pos1.value, entity1.GetSoA<Pos2AoSoA>().value);
        AreEqual(pos2.value, entity2.GetSoA<Pos2AoSoA>().value);
        AreEqual(pos3.value, entity3.GetSoA<Pos2AoSoA>().value);
    }

    /// Test <see cref="StructSoAVector4{T}.ResizeComponents"/>
    [Test]
    public static void Test_AoSoAVector2_SoA_Check_ResizeComponents()
    {
        var store = new EntityStore();
        store.ShrinkRatioThreshold = 0;
        for (int n = 1; n <= 2000; n++)
        {
            {
                var pos = new Pos2AoSoA { value = new Vector2(n,  n + 10_000) };
                store.CreateEntity(pos);
            }
            if (n % 100 == 0) {
                for (int i = 1; i < n; i++)
                {
                    var entity = store.GetEntityById(i);
                    var pos = entity.GetSoA<Pos2AoSoA>();
                    var expect = new Vector2(i,  i + 10_000);
                    AreEqual(expect, pos.value);
                }
            }
        }
        var query = store.Query<Pos2AoSoA>();
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
                    var pos = entity.GetSoA<Pos2AoSoA>();
                    var expect = new Vector2(id,  id + 10_000);
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
                    var pos = entity.GetSoA<Pos2AoSoA>();
                    var expect = new Vector2(id,  id + 10_000);
                    AreEqual(expect, pos.value);
                }
            }
        }
        AreEqual(0, query.Count);
    }
    
    [Test]
    public static void Test_AoSoAVector2_SoA_Entity_exceptions()
    {
        var store = new EntityStore();
        var entitySoA = store.CreateEntity(new Pos2AoSoA());
        var entityAoS = store.CreateEntity(new Position());
        
        var e = Throws<InvalidOperationException>(() => {
            entityAoS.GetSoA<Position>();
        });
        AreEqual("Component 'Position' is stored as AoS. GetSoA() requires SoA storage. Add [SoA] attribute or use GetComponent() instead.", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            entitySoA.GetComponent<Pos2AoSoA>();
        });
        AreEqual("Component 'Pos2AoSoA' is stored as SoA. GetComponent() requires AoS storage. Remove attribute [SoA] or use GetSoA() instead.", e!.Message);
    }
    
    /// Test <see cref="StructSoAVector4{T}.Write"/>
    /// Test <see cref="StructSoAVector4{T}.Read"/>
    [Test]
    public static void Test_AoSoAVector2_SoA_serialize()
    {
        var store = new EntityStore();
        var pos = new Pos2AoSoA { value = new Vector2(11, 12) };
        var entity = store.CreateEntity(pos);
        
        EntityConverter converter = new EntityConverter();
        var dataEntity = converter.EntityToDataEntity(entity, null, false);
        AreEqual("{\"Pos2AoSoA\":{\"value\":{\"X\":11,\"Y\":12}}}", dataEntity.components.ToString());
        
        var targetStore = new EntityStore();
        var targetEntity = converter.DataEntityToEntity(dataEntity, targetStore, out var error);
        AreEqual(pos.value, targetEntity.GetSoA<Pos2AoSoA>().value);
        IsNull(error);
    }
    
    /// Test <see cref="StructSoAVector4{T}.GetComponentMember"/>
    [Test]
    public static void Test_AoSoAVector2_SoA_GetEntityComponentMember()
    {
        var store  = new EntityStore();
        var pos = new Pos2AoSoA { value = new Vector2(11, 12) };
        var entity = store.CreateEntity(pos);
        var componentType   = typeof(Pos2AoSoA);
        var pos3Info = MemberPath.Get(componentType, nameof(Pos2AoSoA.value));
        IsTrue(EntityUtils.GetEntityComponentMember<Vector2>(entity, pos3Info, out var vector3, out _));
        
        AreEqual(new Vector2(11, 12),               vector3);
        AreEqual("value",                           pos3Info.path);
        AreEqual("value",                           pos3Info.name);
        AreEqual(typeof(Vector2),                   pos3Info.memberType);
        AreEqual(typeof(Pos2AoSoA),                 pos3Info.declaringType);
        AreEqual(typeof(Pos2AoSoA),                 pos3Info.componentType.Type);
        AreEqual("get: (Pos2AoSoA => value)",       pos3Info.getter.Method.Name);
        AreEqual("set: (Pos2AoSoA => value)",       pos3Info.setter.Method.Name);
        NotNull ("value",                           pos3Info.memberInfo.Name);
        AreEqual("Pos2AoSoA value : Vector2",       pos3Info.ToString());
    }
    
    /// Test <see cref="StructSoAVector4{T}.SetComponentMember"/>
    [Test]
    public static void Test_AoSoAVector2_SoA_SetEntityComponentMember()
    {
        var store  = new EntityStore();
        var pos = new Pos2AoSoA { value = new Vector2(11, 12) };
        var entity = store.CreateEntity(pos);
        var componentType   = typeof(Pos2AoSoA);
        var nameInfo = MemberPath.Get(componentType, nameof(Pos2AoSoA.value));
        
        var newPos = new Vector2(101, 102);
        OnMemberChanged<Pos2AoSoA> handler = (ref Pos2AoSoA value, Entity entity1, string path, in Pos2AoSoA old) => {
            AreEqual(1,         entity1.Id);
            AreEqual("value",   path);
            AreEqual(newPos,    value.value);
            AreEqual(pos.value, old.value);
        };
        IsTrue(EntityUtils.SetEntityComponentMember(entity, nameInfo, newPos, handler, out _));
        var result = entity.GetSoA<Pos2AoSoA>();
        AreEqual(newPos, result.value);
    }
    
    /// Test <see cref="StructSoAVector4{T}.CopyComponent"/>
    [Test]
    public static void Test_AoSoAVector2_SoA_CopyEntity()
    {
        var store       = new EntityStore();
        var pos         = new Pos2AoSoA { value = new Vector2(11, 12) };
        var srcEntity   = store.CreateEntity(pos);
        
        var target  = new EntityStore();
        var targetEntity = target.CreateEntity();
        srcEntity.CopyEntity(targetEntity);
        
        AreEqual(pos.value, targetEntity.GetSoA<Pos2AoSoA>().value);
    }
    
    /// Test <see cref="StructSoAVector4{T}.SetBatchComponent"/>
    [Test]
    public static void Test_AoSoAVector2_SoA_Batch_CreateEntity()
    {
        var store = new EntityStore();
        var batch = new CreateEntityBatch(store);
        var pos = new Pos2AoSoA { value = new Vector2(11, 12) };
        batch.Add(pos);
        var entity = batch.CreateEntity();
        
        AreEqual(pos.value, entity.GetSoA<Pos2AoSoA>().value);
    }
    
    /// Test <see cref="StructSoAVector4{T}.GetComponentDebug"/>
    [Test]
    public static void Test_AoSoAVector2_SoA_GetEntityComponent()
    {
        var store = new EntityStore();
        var pos         = new Pos2AoSoA { value = new Vector2(11, 12) };
        var entity = store.CreateEntity(pos);
        var schema          = EntityStore.GetEntitySchema();
        var componentType   = schema.ComponentTypeByType[typeof(Pos2AoSoA)];
        
        var result = (Pos2AoSoA)EntityUtils.GetEntityComponent(entity, componentType);
        AreEqual(pos.value, result.value);
    }
    
    /// Test <see cref="StructSoAVector4{T}.SetComponentsDefault"/>
    [Test]
    public static void Test_AoSoAVector2_SoA_Archetype_CreateEntities()
    {
        var store = new EntityStore();
        var pos         = new Pos2AoSoA { value = new Vector2(11, 12) };
        var entity1 = store.CreateEntity(pos);
        var entity2 = store.CreateEntity(pos);
        var archetype = entity1.Archetype;
        
        entity1.DeleteEntity();
        entity2.DeleteEntity();
        
        var entities = archetype.CreateEntities(2);
        foreach (var entity in entities) {
            AreEqual(new Vector2(), entity.GetSoA<Pos2AoSoA>().value);
        }
    }
    

}

}
