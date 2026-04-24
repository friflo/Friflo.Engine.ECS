using System;
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

public static class Test_SoAFloat
{
    [Test]
    public static void Test_SoAFloat_SoA_Add_Remove_Components()
    {
        var store = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        
        var pos1 = new FloatComponent { value = 11 };
        var pos2 = new FloatComponent { value = 22 };
        var pos3 = new FloatComponent { value = 31 };
        store.OnComponentAdded += changed => {
            switch (changed.EntityId) {
                case 1: AreEqual(pos1.value, changed.Component<FloatComponent>().value) ; break;
                case 2: AreEqual(pos2.value, changed.Component<FloatComponent>().value); break;
            }
        };
        store.OnComponentRemoved += changed => {
            switch (changed.EntityId) {
                case 1:
                    AreEqual(pos1.value, changed.OldComponent<FloatComponent>().value);
                    AreEqual(pos1.value, ((FloatComponent)changed.DebugOldComponent).value);
                    break;
            }
        };
        
        entity1.AddComponent(pos1);
        var result1 = entity1.GetComponent<FloatComponent>();
        AreEqual(result1.value, pos1.value);
        
        entity2.AddComponent(pos2);
        var result2 = entity2.GetComponent<FloatComponent>();
        AreEqual(result2.value, pos2.value);
        
        entity3.AddComponent(pos3);
        var result3 = entity3.GetSoA<FloatComponent>();
        AreEqual(result3.value, pos3.value);
        
        entity1.RemoveComponent<FloatComponent>();
        
        result2 = entity2.GetSoA<FloatComponent>();
        AreEqual(result2.value, pos2.value);
        
        result3 = entity3.GetSoA<FloatComponent>();
        AreEqual(result3.value, pos3.value);
        
        IsTrue(entity3.TryGetComponent(out FloatComponent tryGet));
        AreEqual(tryGet.value, pos3.value);
    }
    
    /// Test <see cref="StructSoAVector2{T}.CopyComponentTo"/>
    [Test]
    public static void Test_SoAFloat_SoA_Add_Tag()
    {
        var store = new EntityStore();
        var pos1 = new FloatComponent { value = 11 };
        var pos2 = new FloatComponent { value = 21 };
        var pos3 = new FloatComponent { value = 31 };
        var entity1 = store.CreateEntity(pos1);
        var entity2 = store.CreateEntity(pos2);
        var entity3 = store.CreateEntity(pos3);
        
        entity1.AddTag<General.MyTag1>();
        entity2.AddTag<General.MyTag2>();
        
        AreEqual(pos1.value, entity1.GetSoA<FloatComponent>().value);
        AreEqual(pos2.value, entity2.GetSoA<FloatComponent>().value);
        AreEqual(pos3.value, entity3.GetSoA<FloatComponent>().value);
    }

    /// Test <see cref="StructSoAVector2{T}.ResizeComponents"/>
    [Test]
    public static void Test_SoAFloat_SoA_Check_ResizeComponents()
    {
        var store = new EntityStore();
        store.ShrinkRatioThreshold = 0;
        for (int n = 1; n <= 2000; n++)
        {
            {
                var pos = new FloatComponent { value = n };
                store.CreateEntity(pos);
            }
            if (n % 100 == 0) {
                for (int i = 1; i < n; i++)
                {
                    var entity = store.GetEntityById(i);
                    var pos = entity.GetSoA<FloatComponent>();
                    var expect = i;
                    AreEqual(expect, pos.value);
                }
            }
        }
        var query = store.Query<FloatComponent>();
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
                    var pos = entity.GetSoA<FloatComponent>();
                    var expect = id;
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
                    var pos = entity.GetSoA<FloatComponent>();
                    var expect = id;
                    AreEqual(expect, pos.value);
                }
            }
        }
        AreEqual(0, query.Count);
    }
    
    [Test]
    public static void Test_SoAFloat_SoA_Entity_exceptions()
    {
        var store = new EntityStore();
        var entitySoA = store.CreateEntity(new FloatComponent());
        var entityAoS = store.CreateEntity(new Position());
        
        var e = Throws<InvalidOperationException>(() => {
            entityAoS.GetSoA<Position>();
        });
        AreEqual("Component 'Position' is stored as AoS. GetSoA() requires SoA storage. Add [SoA] attribute or use GetComponent() instead.", e!.Message);
    }
    
    /// Test <see cref="StructSoAVector2{T}.Write"/>
    /// Test <see cref="StructSoAVector2{T}.Read"/>
    [Test]
    public static void Test_SoAFloat_SoA_serialize()
    {
        var store = new EntityStore();
        var pos = new FloatComponent { value = 11 };
        var entity = store.CreateEntity(pos);
        
        EntityConverter converter = new EntityConverter();
        var dataEntity = converter.EntityToDataEntity(entity, null, false);
        AreEqual("{\"FloatComponent\":{\"value\":11}}", dataEntity.components.ToString());
        
        var targetStore = new EntityStore();
        var targetEntity = converter.DataEntityToEntity(dataEntity, targetStore, out var error);
        AreEqual(pos.value, targetEntity.GetSoA<FloatComponent>().value);
        IsNull(error);
    }
    
    /// Test <see cref="StructSoAVector2{T}.GetComponentMember"/>
    [Test]
    public static void Test_SoAFloat_SoA_GetEntityComponentMember()
    {
        var store  = new EntityStore();
        var pos = new FloatComponent { value = 11 };
        var entity = store.CreateEntity(pos);
        var componentType   = typeof(FloatComponent);
        var pos3Info = MemberPath.Get(componentType, nameof(FloatComponent.value));
        IsTrue(EntityUtils.GetEntityComponentMember<float>(entity, pos3Info, out var vector3, out _));
        
        AreEqual(11,                                vector3);
        AreEqual("value",                           pos3Info.path);
        AreEqual("value",                           pos3Info.name);
        AreEqual(typeof(float),                     pos3Info.memberType);
        AreEqual(typeof(FloatComponent),            pos3Info.declaringType);
        AreEqual(typeof(FloatComponent),            pos3Info.componentType.Type);
        AreEqual("get: (FloatComponent => value)",  pos3Info.getter.Method.Name);
        AreEqual("set: (FloatComponent => value)",  pos3Info.setter.Method.Name);
        NotNull ("value",                           pos3Info.memberInfo.Name);
        AreEqual("FloatComponent value : Single",   pos3Info.ToString());
    }
    
    /// Test <see cref="StructSoAVector2{T}.SetComponentMember"/>
    [Test]
    public static void Test_SoAFloat_SoA_SetEntityComponentMember()
    {
        var store  = new EntityStore();
        var pos = new FloatComponent { value = 11 };
        var entity = store.CreateEntity(pos);
        var componentType   = typeof(FloatComponent);
        var nameInfo = MemberPath.Get(componentType, nameof(FloatComponent.value));
        
        float newPos = 101;
        OnMemberChanged<FloatComponent> handler = (ref FloatComponent value, Entity entity1, string path, in FloatComponent old) => {
            AreEqual(1,         entity1.Id);
            AreEqual("value",   path);
            AreEqual(newPos,    value.value);
            AreEqual(pos.value, old.value);
        };
        IsTrue(EntityUtils.SetEntityComponentMember(entity, nameInfo, newPos, handler, out _));
        var result = entity.GetSoA<FloatComponent>();
        AreEqual(newPos, result.value);
    }
    
    /// Test <see cref="StructSoAVector2{T}.CopyComponent"/>
    [Test]
    public static void Test_SoAFloat_SoA_CopyEntity()
    {
        var store       = new EntityStore();
        var pos         = new FloatComponent { value = 11 };
        var srcEntity   = store.CreateEntity(pos);
        
        var target  = new EntityStore();
        var targetEntity = target.CreateEntity();
        srcEntity.CopyEntity(targetEntity);
        
        AreEqual(pos.value, targetEntity.GetSoA<FloatComponent>().value);
    }
    
    /// Test <see cref="StructSoAVector2{T}.SetBatchComponent"/>
    [Test]
    public static void Test_SoAFloat_SoA_Batch_CreateEntity()
    {
        var store = new EntityStore();
        var batch = new CreateEntityBatch(store);
        var pos = new FloatComponent { value = 11 };
        batch.Add(pos);
        var entity = batch.CreateEntity();
        
        AreEqual(pos.value, entity.GetSoA<FloatComponent>().value);
    }
    
    /// Test <see cref="StructSoAVector2{T}.GetComponentDebug"/>
    [Test]
    public static void Test_SoAFloat_SoA_GetEntityComponent()
    {
        var store = new EntityStore();
        var pos         = new FloatComponent { value = 11 };
        var entity = store.CreateEntity(pos);
        var schema          = EntityStore.GetEntitySchema();
        var componentType   = schema.ComponentTypeByType[typeof(FloatComponent)];
        
        var result = (FloatComponent)EntityUtils.GetEntityComponent(entity, componentType);
        AreEqual(pos.value, result.value);
    }
    
    /// Test <see cref="StructSoAVector2{T}.SetComponentsDefault"/>
    [Test]
    public static void Test_SoAFloat_SoA_Archetype_CreateEntities()
    {
        var store = new EntityStore();
        var pos         = new FloatComponent { value = 11 };
        var entity1 = store.CreateEntity(pos);
        var entity2 = store.CreateEntity(pos);
        var archetype = entity1.Archetype;
        
        entity1.DeleteEntity();
        entity2.DeleteEntity();
        
        var entities = archetype.CreateEntities(2);
        foreach (var entity in entities) {
            AreEqual(new float(), entity.GetSoA<FloatComponent>().value);
        }
    }
    
    /// Test <see cref="Chunk{T}.GetLanesSoA"/>
    [Test]
    public static void Test_SoAFloat_Query_Lanes()
    {
        var store = new EntityStore();
        for (int n = 0; n < 1000; n++) {
            store.CreateEntity(new FloatComponent { value = n * 10 });
        }
        var query = store.Query<FloatComponent>();
        int count = 0;
        foreach (var (positions, entities) in query.Chunks) {
            var positionSpan = positions.Span;
            for (int i = 0; i < entities.Length; i++) {
                var value = positions[i];
                var expect = new FloatComponent { value = i * 10 };
                That(value.value, Is.EqualTo(expect.value));
                That(positionSpan[i].value, Is.EqualTo(expect.value));
                
                expect.value += 100;
                value.value = expect.value;
                positions[i] = value;
                value = positions[i];
                That(value.value, Is.EqualTo(expect.value));
            }
            count++;
            
            var e = Throws<InvalidOperationException>(() => { _ = positions.GetLanesSoA(); });
            AreEqual("Expect call for SoA component data.", e!.Message);
            
        }
        AreEqual(1, count);
    }
}

}
