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

[Ignore("Implement StructAoSVector4<>")]
public static class Test_AoSVector4
{
    [Test]
    public static void Test_AoSVector4_Add_Remove_Components()
    {
        var store = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        
        var pos1 = new Pos4 { value = new Vector4(11, 12, 13, 14) };
        var pos2 = new Pos4 { value = new Vector4(21, 22, 23, 24) };
        var pos3 = new Pos4 { value = new Vector4(31, 32, 33, 34) };
        store.OnComponentAdded += changed => {
            switch (changed.EntityId) {
                case 1: AreEqual(pos1.value, changed.Component<Pos4>().value) ; break;
                case 2: AreEqual(pos2.value, changed.Component<Pos4>().value); break;
            }
        };
        store.OnComponentRemoved += changed => {
            switch (changed.EntityId) {
                case 1:
                    AreEqual(pos1.value, changed.OldComponent<Pos4>().value);
                    AreEqual(pos1.value, ((Pos4)changed.DebugOldComponent).value);
                    break;
            }
        };
        
        entity1.AddComponent(pos1);
        var result1 = entity1.GetComponent<Pos4>();
        AreEqual(result1.value, pos1.value);
        
        entity2.AddComponent(pos2);
        var result2 = entity2.GetComponent<Pos4>();
        AreEqual(result2.value, pos2.value);
        
        entity3.AddComponent(pos3);
        var result3 = entity3.GetComponent<Pos4>();
        AreEqual(result3.value, pos3.value);
        
        entity1.RemoveComponent<Pos4>();
        
        result2 = entity2.GetComponent<Pos4>();
        AreEqual(result2.value, pos2.value);
        
        result3 = entity3.GetComponent<Pos4>();
        AreEqual(result3.value, pos3.value);
        
        IsTrue(entity3.TryGetComponent(out Pos4 tryGet));
        AreEqual(tryGet.value, pos3.value);
    }
    
    /// Test <see cref="StructSoAVector4{T}.CopyComponentTo"/>
    [Test]
    public static void Test_AoSVector4_Add_Tag()
    {
        var store = new EntityStore();
        var pos1 = new Pos4 { value = new Vector4(11, 12, 13, 14) };
        var pos2 = new Pos4 { value = new Vector4(21, 22, 23, 24) };
        var pos3 = new Pos4 { value = new Vector4(31, 32, 33, 34) };
        var entity1 = store.CreateEntity(pos1);
        var entity2 = store.CreateEntity(pos2);
        var entity3 = store.CreateEntity(pos3);
        
        entity1.AddTag<General.MyTag1>();
        entity2.AddTag<General.MyTag2>();
        
        AreEqual(pos1.value, entity1.GetSoA<Pos4>().value);
        AreEqual(pos2.value, entity2.GetSoA<Pos4>().value);
        AreEqual(pos3.value, entity3.GetSoA<Pos4>().value);
    }

    /// Test <see cref="StructSoAVector4{T}.ResizeComponents"/>
    [Test]
    public static void Test_AoSVector4_Check_ResizeComponents()
    {
        var store = new EntityStore();
        store.ShrinkRatioThreshold = 0;
        for (int n = 1; n <= 2000; n++)
        {
            {
                var pos = new Pos4 { value = new Vector4(n,  n + 10_000, n + 20_000, n + 30_000) };
                store.CreateEntity(pos);
            }
            if (n % 100 == 0) {
                for (int i = 1; i < n; i++)
                {
                    var entity = store.GetEntityById(i);
                    var pos = entity.GetSoA<Pos4>();
                    var expect = new Vector4(i,  i + 10_000, i + 20_000, i + 30_000);
                    AreEqual(expect, pos.value);
                }
            }
        }
        var query = store.Query<Pos4>();
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
                    var pos = entity.GetSoA<Pos4>();
                    var expect = new Vector4(id,  id + 10_000, id + 20_000, id + 30_000);
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
                    var pos = entity.GetSoA<Pos4>();
                    var expect = new Vector4(id,  id + 10_000, id + 20_000, id + 30_000);
                    AreEqual(expect, pos.value);
                }
            }
        }
        AreEqual(0, query.Count);
    }
    
    [Test]
    public static void Test_AoSVector4_Entity_exceptions()
    {
        var store = new EntityStore();
        var entitySoA = store.CreateEntity(new Pos4());
        var entityAoS = store.CreateEntity(new Position());
        
        var e = Throws<InvalidOperationException>(() => {
            entityAoS.GetSoA<Position>();
        });
        AreEqual("Component 'Position' is stored as AoS. GetSoA() requires SoA storage. Add [SoA] attribute or use GetComponent() instead.", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            entitySoA.GetComponent<Pos4>();
        });
        AreEqual("Component 'Pos4' is stored as SoA. GetComponent() requires AoS storage. Remove attribute [SoA] or use GetSoA() instead.", e!.Message);
    }
    
    /// Test <see cref="StructSoAVector4{T}.Write"/>
    /// Test <see cref="StructSoAVector4{T}.Read"/>
    [Test]
    public static void Test_AoSVector4_serialize()
    {
        var store = new EntityStore();
        var pos = new Pos4 { value = new Vector4(11, 12, 13, 14) };
        var entity = store.CreateEntity(pos);
        
        EntityConverter converter = new EntityConverter();
        var dataEntity = converter.EntityToDataEntity(entity, null, false);
        AreEqual("{\"Pos4\":{\"value\":{\"X\":11,\"Y\":12,\"Z\":13,\"W\":14}}}", dataEntity.components.ToString());
        
        var targetStore = new EntityStore();
        var targetEntity = converter.DataEntityToEntity(dataEntity, targetStore, out var error);
        AreEqual(pos.value, targetEntity.GetSoA<Pos4>().value);
        IsNull(error);
    }
    
    /// Test <see cref="StructSoAVector4{T}.GetComponentMember"/>
    [Test]
    public static void Test_AoSVector4_GetEntityComponentMember()
    {
        var store  = new EntityStore();
        var pos = new Pos4 { value = new Vector4(11, 12, 13, 14) };
        var entity = store.CreateEntity(pos);
        var componentType   = typeof(Pos4);
        var posInfo = MemberPath.Get(componentType, nameof(Pos4.value));
        IsTrue(EntityUtils.GetEntityComponentMember<Vector4>(entity, posInfo, out var vector3, out _));
        
        AreEqual(new Vector4(11, 12, 13, 14),   vector3);
        AreEqual("value",                       posInfo.path);
        AreEqual("value",                       posInfo.name);
        AreEqual(typeof(Vector4),               posInfo.memberType);
        AreEqual(typeof(Pos4),                  posInfo.declaringType);
        AreEqual(typeof(Pos4),                  posInfo.componentType.Type);
        AreEqual("get: (Pos4 => value)",        posInfo.getter.Method.Name);
        AreEqual("set: (Pos4 => value)",        posInfo.setter.Method.Name);
        NotNull ("value",                       posInfo.memberInfo.Name);
        AreEqual("Pos4 value : Vector4",        posInfo.ToString());
    }
    
    /// Test <see cref="StructSoAVector4{T}.SetComponentMember"/>
    [Test]
    public static void Test_AoSVector4_SetEntityComponentMember()
    {
        var store  = new EntityStore();
        var pos = new Pos4 { value = new Vector4(11, 12, 13, 14) };
        var entity = store.CreateEntity(pos);
        var componentType   = typeof(Pos4);
        var nameInfo = MemberPath.Get(componentType, nameof(Pos4.value));
        
        var newPos = new Vector4(101, 102, 103, 104);
        OnMemberChanged<Pos4> handler = (ref Pos4 value, Entity entity1, string path, in Pos4 old) => {
            AreEqual(1,         entity1.Id);
            AreEqual("value",   path);
            AreEqual(newPos,    value.value);
            AreEqual(pos.value, old.value);
        };
        IsTrue(EntityUtils.SetEntityComponentMember(entity, nameInfo, newPos, handler, out _));
        var result = entity.GetSoA<Pos4>();
        AreEqual(newPos, result.value);
    }
    
    /// Test <see cref="StructSoAVector4{T}.CopyComponent"/>
    [Test]
    public static void Test_AoSVector4_CopyEntity()
    {
        var store       = new EntityStore();
        var pos         = new Pos4 { value = new Vector4(11, 12, 13, 14) };
        var srcEntity   = store.CreateEntity(pos);
        
        var target  = new EntityStore();
        var targetEntity = target.CreateEntity();
        srcEntity.CopyEntity(targetEntity);
        
        AreEqual(pos.value, targetEntity.GetSoA<Pos4>().value);
    }
    
    /// Test <see cref="StructSoAVector4{T}.SetBatchComponent"/>
    [Test]
    public static void Test_AoSVector4_Batch_CreateEntity()
    {
        var store = new EntityStore();
        var batch = new CreateEntityBatch(store);
        var pos = new Pos4 { value = new Vector4(11, 12, 13, 14) };
        batch.Add(pos);
        var entity = batch.CreateEntity();
        
        AreEqual(pos.value, entity.GetSoA<Pos4>().value);
    }
    
    /// Test <see cref="StructSoAVector4{T}.GetComponentDebug"/>
    [Test]
    public static void Test_AoSVector4_GetEntityComponent()
    {
        var store = new EntityStore();
        var pos         = new Pos4 { value = new Vector4(11, 12, 13, 14) };
        var entity = store.CreateEntity(pos);
        var schema          = EntityStore.GetEntitySchema();
        var componentType   = schema.ComponentTypeByType[typeof(Pos4)];
        
        var result = (Pos4)EntityUtils.GetEntityComponent(entity, componentType);
        AreEqual(pos.value, result.value);
    }
    
    /// Test <see cref="StructSoAVector4{T}.SetComponentsDefault"/>
    [Test]
    public static void Test_AoSVector4_Archetype_CreateEntities()
    {
        var store = new EntityStore();
        var pos         = new Pos4 { value = new Vector4(11, 12, 13, 14) };
        var entity1 = store.CreateEntity(pos);
        var entity2 = store.CreateEntity(pos);
        var archetype = entity1.Archetype;
        
        entity1.DeleteEntity();
        entity2.DeleteEntity();
        
        var entities = archetype.CreateEntities(2);
        foreach (var entity in entities) {
            AreEqual(new Vector4(), entity.GetSoA<Pos4>().value);
        }
    }
}

}
