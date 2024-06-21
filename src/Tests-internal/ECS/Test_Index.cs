using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Internal.ECS {

internal struct AttackComponent : IIndexedComponent<Entity> {
    public      Entity  GetIndexedValue() => target;
    internal    Entity  target;
}


internal struct IndexedName : IIndexedComponent<string> {
    public      string  GetIndexedValue() => name;
    internal    string  name;

    public override string ToString() => name;
}

internal struct IndexedInt : IIndexedComponent<int> {
    public      int     GetIndexedValue() => value;
    internal    int     value;
    
    public override string ToString() => value.ToString();
}

internal struct IndexedEntity : IIndexedComponent<Entity> {
    public      Entity  GetIndexedValue() => entity;
    internal    Entity  entity;
    
    public override string ToString() => entity.ToString();
}


[SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
[SuppressMessage("Performance", "CA1825:Avoid zero-length array allocations")]
public static class Test_Index
{
    [Test]
    public static void Test_Index_Component_Add_Remove()
    {
        var store = new EntityStore();
        var targets = new EntityList(store);
        for (int n = 0; n < 10; n++) {
            targets.Add(store.CreateEntity());
        }
        var entities = new List<Entity>();
        for (int n = 0; n < 10; n++) {
            var entity = store.CreateEntity(new Position(n, 0, 0), new AttackComponent{target = targets[n]});
            entities.Add(entity);
        }
        Entity target = targets[0];
        var entity0 = entities[0];
        var entity1 = entities[1];
        var entity2 = entities[2];
        
        entity0.AddComponent(new IndexedName    { name   = "find-me" });
        entity1.AddComponent(new IndexedInt     { value  = 123       });
        entity2.AddComponent(new IndexedName    { name   = "find-me" });
        entity2.AddComponent(new IndexedInt     { value  = 123       });
        entity2.AddComponent(new AttackComponent{ target = target    });
        
        var query1  = store.Query<Position,    IndexedName>().  HasValue<IndexedName,   string>("find-me");
        var query2  = store.Query<Position,    IndexedInt>().   HasValue<IndexedInt,    int>   (123);
        var query3  = store.Query<IndexedName, IndexedInt>().   HasValue<IndexedName,   string>("find-me").
                                                                HasValue<IndexedInt,    int>   (123);
        var query4  = store.Query().                            HasValue<IndexedName,   string>("find-me").
                                                                HasValue<IndexedInt,    int>   (123);
        var query5  = store.Query<Position, AttackComponent>(). HasValue<AttackComponent, Entity>(target);
        
        {
            int count = 0;
            query1.ForEachEntity((ref Position _, ref IndexedName indexedName, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(11, entity.Id); break;
                    case 1: AreEqual(13, entity.Id); break;
                }
                AreEqual("find-me", indexedName.name);
            });
            AreEqual(2, count);
        } { 
            int count = 0;
            query2.ForEachEntity((ref Position _, ref IndexedInt indexedInt, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(12, entity.Id); break;
                    case 1: AreEqual(13, entity.Id); break;
                }
                AreEqual(123, indexedInt.value);
            });
            AreEqual(2, count);
        } { 
            var count = 0;
            query3.ForEachEntity((ref IndexedName _, ref IndexedInt _, Entity entity) => {
                AreEqual(13, entity.Id);
                count++;
            });
            AreEqual(1, count);
        } {
            var count = 0;
            query5.ForEachEntity((ref Position _, ref AttackComponent attack, Entity entity) => {
                count++;
                AreEqual(13,     entity.Id);
                AreEqual(target, attack.target);
            });
            AreEqual(1, count);
        }
        AreEqual(2, query1.Entities.Count);     AreEqual(new int[] { 11, 13 },      query1.Entities.ToIds());
        AreEqual(2, query2.Entities.Count);     AreEqual(new int[] { 12, 13 },      query2.Entities.ToIds());
        AreEqual(1, query3.Entities.Count);     AreEqual(new int[] { 13 },          query3.Entities.ToIds());
        AreEqual(3, query4.Entities.Count);     AreEqual(new int[] { 11, 13, 12 },  query4.Entities.ToIds());
        
        entity2.RemoveComponent<IndexedName>();
        AreEqual(1, query1.Entities.Count);     AreEqual(new int[] { 11 },          query1.Entities.ToIds());
        AreEqual(2, query2.Entities.Count);     AreEqual(new int[] { 12, 13 },      query2.Entities.ToIds());
        AreEqual(0, query3.Entities.Count);     AreEqual(new int[] { },             query3.Entities.ToIds());
        AreEqual(3, query4.Entities.Count);     AreEqual(new int[] { 11, 12, 13 },  query4.Entities.ToIds());
        
        entity2.RemoveComponent<IndexedInt>();
        AreEqual(1, query1.Entities.Count);     AreEqual(new int[] { 11 },          query1.Entities.ToIds());
        AreEqual(1, query2.Entities.Count);     AreEqual(new int[] { 12 },          query2.Entities.ToIds());
        AreEqual(0, query3.Entities.Count);     AreEqual(new int[] { },             query3.Entities.ToIds());
        AreEqual(2, query4.Entities.Count);     AreEqual(new int[] { 11, 12 },      query4.Entities.ToIds());
        
        entity1.RemoveComponent<IndexedInt>();
        AreEqual(1, query1.Entities.Count);     AreEqual(new int[] { 11 },          query1.Entities.ToIds());
        AreEqual(0, query2.Entities.Count);     AreEqual(new int[] { },             query2.Entities.ToIds());
        AreEqual(0, query3.Entities.Count);     AreEqual(new int[] { },             query3.Entities.ToIds());
        AreEqual(1, query4.Entities.Count);     AreEqual(new int[] { 11 },          query4.Entities.ToIds());
        
        entity0.RemoveComponent<IndexedName>();
        AreEqual(0, query1.Entities.Count);     AreEqual(new int[] { },             query1.Entities.ToIds());
        AreEqual(0, query2.Entities.Count);     AreEqual(new int[] { },             query2.Entities.ToIds());
        AreEqual(0, query3.Entities.Count);     AreEqual(new int[] { },             query3.Entities.ToIds());
        AreEqual(0, query4.Entities.Count);     AreEqual(new int[] { },             query4.Entities.ToIds());
    }
    
    /// <summary>
    /// Cover <see cref="ValueStructIndex{TValue}.Add{TComponent}"/>
    /// </summary>
    [Test]
    public static void Test_Index_Component_Update()
    {
        var store = new EntityStore();

        var entities = new List<Entity>();
        for (int n = 0; n < 10; n++) {
            var entity = store.CreateEntity(new Position(n, 0, 0));
            entities.Add(entity);
        }
        var entity1 = entities[0];
        var entity2 = entities[1];
        var entity3 = entities[2];
        
        entity1.AddComponent(new IndexedName   { name   = "find-me1" });
        entity2.AddComponent(new IndexedInt    { value  = 123       });
        entity3.AddComponent(new IndexedName   { name   = "find-me1" });
        entity3.AddComponent(new IndexedInt    { value  = 123       });
        
        var result = store.GetEntitiesWithComponentValue<IndexedName, string>("find-me1");
        AreEqual(2, result.Count);     AreEqual(new int[] { 1, 3 },    result.Ids.ToArray());
        result     = store.GetEntitiesWithComponentValue<IndexedInt, int>(123);
        AreEqual(2, result.Count);     AreEqual(new int[] { 2, 3 },    result.Ids.ToArray());
        result     = store.GetEntitiesWithComponentValue<IndexedInt, int>(42);
        AreEqual(0, result.Count);     AreEqual(new int[] {      },    result.Ids.ToArray());

        var query1  = store.Query<Position,    IndexedName>().  HasValue<IndexedName,   string>("find-me1");
        var query2  = store.Query<IndexedName, IndexedInt>().   HasValue<IndexedName,   string>("find-me1");
        var query3  = store.Query().                            HasValue<IndexedName,   string>("find-me1");
        
        AreEqual(2, query1.Entities.Count);     AreEqual(new int[] { 1, 3 },        query1.Entities.ToIds());
        AreEqual(1, query2.Entities.Count);     AreEqual(new int[] { 3    },        query2.Entities.ToIds());
        AreEqual(2, query3.Entities.Count);     AreEqual(new int[] { 1, 3 },        query3.Entities.ToIds());
        
        // Add same value of indexed component again
        entity1.AddComponent(new IndexedName   { name   = "find-me1" });
        AreEqual(2, query1.Entities.Count);     AreEqual(new int[] { 1, 3 },    query1.Entities.ToIds());
        AreEqual(1, query2.Entities.Count);     AreEqual(new int[] { 3    },    query2.Entities.ToIds());
        AreEqual(2, query3.Entities.Count);     AreEqual(new int[] { 1, 3 },    query3.Entities.ToIds());
        
        // Update value of indexed component
        entity1.AddComponent(new IndexedName   { name   = "find-me2" });
        AreEqual(1, query1.Entities.Count);     AreEqual(new int[] { 3 },       query1.Entities.ToIds());
        AreEqual(1, query2.Entities.Count);     AreEqual(new int[] { 3 },       query2.Entities.ToIds());
        AreEqual(1, query3.Entities.Count);     AreEqual(new int[] { 3 },       query3.Entities.ToIds());

        // --- change queries
        query1  = store.Query<Position,    IndexedName>().  HasValue<IndexedName,   string>("find-me2");
        query2  = store.Query<IndexedName, IndexedInt>().   HasValue<IndexedName,   string>("find-me2");
        query3  = store.Query().                            HasValue<IndexedName,   string>("find-me2");
        
        AreEqual(1, query1.Entities.Count);     AreEqual(new int[] { 1 },       query1.Entities.ToIds());
        AreEqual(0, query2.Entities.Count);     AreEqual(new int[] {   },       query2.Entities.ToIds());
        AreEqual(1, query3.Entities.Count);     AreEqual(new int[] { 1 },       query3.Entities.ToIds());
    }
    
    [Test]
    public static void Test_Index_indexed_Entity()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        
        var target4 = store.CreateEntity(4);
        var target5 = store.CreateEntity(5);
        var target6 = store.CreateEntity(6);
        
        entity1.AddComponent(new IndexedEntity { entity = target4 });
        entity2.AddComponent(new IndexedEntity { entity = target5 });
        entity3.AddComponent(new IndexedEntity { entity = target5 });
        
        var query1  = store.Query().HasValue<IndexedEntity,   Entity>(target4);
        var query2  = store.Query().HasValue<IndexedEntity,   Entity>(target4).
                                    HasValue<IndexedEntity,   Entity>(target5);
        
        AreEqual(new int [] { 1       },    query1.Entities.ToIds());
        AreEqual(new int [] { 1, 2, 3 },    query2.Entities.ToIds());
        
        var references4 = target4.GetForeignEntities<IndexedEntity>();
        AreEqual(new int [] { 1       },    references4.Ids.ToArray());
        
        var references5 = target5.GetForeignEntities<IndexedEntity>();
        AreEqual(new int [] { 2, 3    },   references5.Ids.ToArray());
        
        entity2.AddComponent(new IndexedEntity { entity = target6 });
        references5 = target5.GetForeignEntities<IndexedEntity>();
        AreEqual(new int [] { 3       },   references5.Ids.ToArray());
        
        entity2.AddComponent(new IndexedEntity { entity = target6 });
        references5 = target5.GetForeignEntities<IndexedEntity>();
        AreEqual(new int [] { 3       },   references5.Ids.ToArray());
        
        entity3.RemoveComponent<IndexedEntity>();
        references5 = target5.GetForeignEntities<IndexedEntity>();
        AreEqual(new int [] {         },   references5.Ids.ToArray());
    }
    
    [Test]
    public static void Test_Index_support_null()
    {
        var world   = new EntityStore();
        var entity1 = world.CreateEntity();
        var entity2 = world.CreateEntity();
        entity1.AddComponent(new IndexedName { name = null });
        entity2.AddComponent(new IndexedName { name = null });
        
        var start = Mem.GetAllocatedBytes();
        var result = world.GetEntitiesWithComponentValue<IndexedName, string>(null);
        Mem.AssertNoAlloc(start);
        
        AreEqual(2, result.Count);
        AreEqual(1, result[0].Id);
        AreEqual(2, result[1].Id);
        
        entity2.RemoveComponent<IndexedName>();
        result = world.GetEntitiesWithComponentValue<IndexedName, string>(null);
        AreEqual(1, result.Count);
    }
    
    [Test]
    public static void Test_Index_ValueStructIndex()
    {
        var world   = new EntityStore();
        var entity1 = world.CreateEntity();
        entity1.AddComponent(new IndexedInt { value = 123 });
        entity1.AddComponent(new IndexedInt { value = 123 }); // add same component value again
        var result = world.GetEntitiesWithComponentValue<IndexedInt, int>(123);
        AreEqual(1, result.Count);
        
        entity1.AddComponent(new IndexedInt { value = 456 });
        result = world.GetEntitiesWithComponentValue<IndexedInt, int>(456);
        AreEqual(1, result.Count);
        result = world.GetEntitiesWithComponentValue<IndexedInt, int>(123);
        AreEqual(0, result.Count);
    }
    
    [Test]
    public static void Test_Index_exceptions()
    {
        var store = new EntityStore();
        var query = store.Query().ValueInRange<IndexedInt, int>(1,2);
        var e = Throws<NotSupportedException>(() => {
            _ = query.Count;
        });
        AreEqual("ValueInRange() not supported by ValueStructIndex`1", e!.Message);
    }
    
    [Test]
    public static void Test_Index_already_added()
    {
        var world   = new EntityStore();
        var entity1 = world.CreateEntity(1);
        var entity2 = world.CreateEntity(2);
        
        entity1.AddComponent(new IndexedName { name = "added" });
        entity2.AddComponent(new IndexedName { name = null });
        
        var index = (ValueClassIndex<string>)StoreIndex.GetIndex(world, StructInfo<IndexedName>.Index);
        index.Add(1, new IndexedName { name = "added" });
        index.Add(2, new IndexedName { name = null    });

        AreEqual(2, index.Count);
    }
    
    [Test]
    public static void Test_Index_already_removed()
    {
        var index = new ValueClassIndex<string>();
        index.RemoveComponentValue(1, "missing");   // add key with default IdArray
        AreEqual(1, index.Count);
        
        index.RemoveComponentValue(2, null);
        AreEqual(1, index.Count);
    }
    
    [Test]
    public static void Test_Index_StoreIndex_ToString()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var indexes = store.extension.indexes;
        AreEqual(null,          indexes[StructInfo<Position>.Index].ToString());
        
        AreEqual("IndexedName", indexes[StructInfo<IndexedName>.Index].ToString());
        entity1.AddComponent(new IndexedName { name = "test" });
        AreEqual("IndexedName - ValueClassIndex`1 count: 1", indexes[StructInfo<IndexedName>.Index].ToString());
        
        entity1.AddComponent(new IndexedEntity { entity = entity2 });
        AreEqual("IndexedEntity - EntityIndex count: 1", indexes[StructInfo<IndexedEntity>.Index].ToString());
        
        entity1.AddComponent(new IndexedInt { value = 42 });
        AreEqual("IndexedInt - ValueStructIndex`1 count: 1", indexes[StructInfo<IndexedInt>.Index].ToString());
    }
    
    internal static int[] ToIds(this QueryEntities entities) => entities.ToEntityList().Ids.ToArray();
    
    [Test]
    public static void Test_AvoidBoxing()
    {
        var indexedName = new IndexedInt { value = 123 };
        
        var getter = CreateGetValue<IndexedInt,int>();
        var value = getter(indexedName);
        AreEqual(123, value);
        
        var start = GC.GetAllocatedBytesForCurrentThread();
        getter(indexedName);
        var diff = GC.GetAllocatedBytesForCurrentThread() - start;
        Console.WriteLine($"diff: {diff}");
    }
    
    // https://stackoverflow.com/questions/61185678/c-sharp-casting-t-where-t-struct-to-an-interface-without-boxing
    private static GetIndexedValue<T,V> CreateGetValue<T,V>() where T : struct, IComponent
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        var method          = typeof(Test_Index).GetMethod(nameof(GetIndexedComponentValue), flags);
        var genericMethod   = method!.MakeGenericMethod(typeof(T), typeof(V));
        
        var genericDelegate = Delegate.CreateDelegate(typeof(GetIndexedValue<T,V>), genericMethod);
        return (GetIndexedValue<T,V>)genericDelegate;
    }
    
    internal static V GetIndexedComponentValue<T,V>(in T component) where T : struct, IIndexedComponent<V> {
        return component.GetIndexedValue();
    }
    
    private delegate V GetIndexedValue<T, out V>(in T component) where T : struct, IComponent;
}

}
