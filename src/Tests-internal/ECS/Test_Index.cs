using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;
using NUnit.Framework;
using Tests.Examples;
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

internal struct LinkComponent : ILinkComponent {
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
        
        var nameValues  = store.GetIndexedComponentValues<IndexedName, string>();
        var intValues   = store.GetIndexedComponentValues<IndexedInt, int>();
        
        entity0.AddComponent(new IndexedName    { name   = "find-me" });    AreEqual(1, nameValues.Count);
        entity1.AddComponent(new IndexedInt     { value  = 123       });    AreEqual(1, intValues.Count);
        entity2.AddComponent(new IndexedName    { name   = "find-me" });    AreEqual(1, nameValues.Count);
        entity2.AddComponent(new IndexedInt     { value  = 123       });    AreEqual(1, intValues.Count);
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
        AreEqual(2, query1.Entities.Count);     AreEqual("{ 11, 13 }",      query1.Entities.ToStr());
        AreEqual(2, query2.Entities.Count);     AreEqual("{ 12, 13 }",      query2.Entities.ToStr());
        AreEqual(1, query3.Entities.Count);     AreEqual("{ 13 }",          query3.Entities.ToStr());
        AreEqual(3, query4.Entities.Count);     AreEqual("{ 11, 13, 12 }",  query4.Entities.ToStr());
        
        entity2.RemoveComponent<IndexedName>();                             AreEqual(1, nameValues.Count);
        AreEqual(1, query1.Entities.Count);     AreEqual("{ 11 }",          query1.Entities.ToStr());
        AreEqual(2, query2.Entities.Count);     AreEqual("{ 12, 13 }",      query2.Entities.ToStr());
        AreEqual(0, query3.Entities.Count);     AreEqual("{ }",             query3.Entities.ToStr());
        AreEqual(3, query4.Entities.Count);     AreEqual("{ 11, 12, 13 }",  query4.Entities.ToStr());
        
        entity2.RemoveComponent<IndexedInt>();                              AreEqual(1, intValues.Count);
        AreEqual(1, query1.Entities.Count);     AreEqual("{ 11 }",          query1.Entities.ToStr());
        AreEqual(1, query2.Entities.Count);     AreEqual("{ 12 }",          query2.Entities.ToStr());
        AreEqual(0, query3.Entities.Count);     AreEqual("{ }",             query3.Entities.ToStr());
        AreEqual(2, query4.Entities.Count);     AreEqual("{ 11, 12 }",      query4.Entities.ToStr());
        
        entity1.RemoveComponent<IndexedInt>();                              AreEqual(0, intValues.Count);
        AreEqual(1, query1.Entities.Count);     AreEqual("{ 11 }",          query1.Entities.ToStr());
        AreEqual(0, query2.Entities.Count);     AreEqual("{ }",             query2.Entities.ToStr());
        AreEqual(0, query3.Entities.Count);     AreEqual("{ }",             query3.Entities.ToStr());
        AreEqual(1, query4.Entities.Count);     AreEqual("{ 11 }",          query4.Entities.ToStr());
        
        entity0.RemoveComponent<IndexedName>();                             AreEqual(0, nameValues.Count);
        AreEqual(0, query1.Entities.Count);     AreEqual("{ }",             query1.Entities.ToStr());
        AreEqual(0, query2.Entities.Count);     AreEqual("{ }",             query2.Entities.ToStr());
        AreEqual(0, query3.Entities.Count);     AreEqual("{ }",             query3.Entities.ToStr());
        AreEqual(0, query4.Entities.Count);     AreEqual("{ }",             query4.Entities.ToStr());
    }
    
    [Test]
    public static void Test_Index_ValueInRange_ValueStructIndex()
    {
        var store = new EntityStore();
        var values = store.GetIndexedComponentValues<IndexedInt, int>();
        for (int n = 1; n <= 10; n++) {
            var entity = store.CreateEntity(n);
            entity.AddComponent(new IndexedInt { value = n });
            AreEqual(n, values.Count);
        }
        var query  = store.Query().ValueInRange<IndexedInt, int>(3, 8);
        AreEqual(6, query.Count);
        AreEqual("{ 3, 4, 5, 6, 7, 8 }", query.Entities.ToStr());
    }
    
    [Test]
    public static void Test_Index_ValueInRange_ValueClassIndex()
    {
        var store = new EntityStore();
        var values = store.GetIndexedComponentValues<IndexedName, string>();
        for (int n = 1; n <= 10; n++) {
            var entity = store.CreateEntity(n);
            entity.AddComponent(new IndexedName { name = n.ToString() });
            AreEqual(n, values.Count);
        }
        var query  = store.Query().ValueInRange<IndexedName, string>("3", "8");
        AreEqual(6, query.Count);
        AreEqual("{ 3, 4, 5, 6, 7, 8 }", query.Entities.ToStr());
    }
    
    [Test]
    public static void Test_Index_ValueInRange_EntityIndex()
    {
        var store       = new EntityStore();
        var entityIndex = new EntityIndex { store = store };
        var e = Throws<NotSupportedException>(() => {
            entityIndex.AddValueInRangeEntities(default, default, null);    
        });
        AreEqual("ValueInRange() not supported by EntityIndex", e!.Message);
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
        var nameValues  = store.GetIndexedComponentValues<IndexedName, string>();
        var intValues   = store.GetIndexedComponentValues<IndexedInt, int>();
        
        entity1.AddComponent(new IndexedName   { name   = "find-me1" });    AreEqual(1, nameValues.Count);
        entity2.AddComponent(new IndexedInt    { value  = 123        });    AreEqual(1, intValues.Count);
        entity3.AddComponent(new IndexedName   { name   = "find-me1" });    AreEqual(1, nameValues.Count);
        entity3.AddComponent(new IndexedInt    { value  = 123        });    AreEqual(1, nameValues.Count);

        AreEqual("find-me1",    nameValues.First());
        AreEqual(123,           intValues.First());
        
        var result = store.GetEntitiesWithComponentValue<IndexedName, string>("find-me1");
        AreEqual(2, result.Count);     AreEqual("{ 1, 3 }",    result.Ids.ToStr());
        result     = store.GetEntitiesWithComponentValue<IndexedInt, int>(123);
        AreEqual(2, result.Count);     AreEqual("{ 2, 3 }",    result.Ids.ToStr());
        result     = store.GetEntitiesWithComponentValue<IndexedInt, int>(42);
        AreEqual(0, result.Count);     AreEqual("{ }",          result.Ids.ToStr());

        var query1  = store.Query<Position,    IndexedName>().  HasValue<IndexedName,   string>("find-me1");
        var query2  = store.Query<IndexedName, IndexedInt>().   HasValue<IndexedName,   string>("find-me1");
        var query3  = store.Query().                            HasValue<IndexedName,   string>("find-me1");
        
        AreEqual(2, query1.Entities.Count);     AreEqual("{ 1, 3 }",        query1.Entities.ToStr());
        AreEqual(1, query2.Entities.Count);     AreEqual("{ 3 }",           query2.Entities.ToStr());
        AreEqual(2, query3.Entities.Count);     AreEqual("{ 1, 3 }",        query3.Entities.ToStr());
        
        // Add same value of indexed component again
        entity1.AddComponent(new IndexedName   { name   = "find-me1" });    AreEqual(1, nameValues.Count);
        AreEqual(2, query1.Entities.Count);     AreEqual("{ 1, 3 }",        query1.Entities.ToStr());
        AreEqual(1, query2.Entities.Count);     AreEqual("{ 3 }",           query2.Entities.ToStr());
        AreEqual(2, query3.Entities.Count);     AreEqual("{ 1, 3 }",        query3.Entities.ToStr());
        
        // Update value of indexed component
        entity1.AddComponent(new IndexedName   { name   = "find-me2" });    AreEqual(2, nameValues.Count);
        AreEqual(1, query1.Entities.Count);     AreEqual("{ 3 }",           query1.Entities.ToStr());
        AreEqual(1, query2.Entities.Count);     AreEqual("{ 3 }",           query2.Entities.ToStr());
        AreEqual(1, query3.Entities.Count);     AreEqual("{ 3 }",           query3.Entities.ToStr());

        // --- change queries
        query1  = store.Query<Position,    IndexedName>().  HasValue<IndexedName,   string>("find-me2");
        query2  = store.Query<IndexedName, IndexedInt>().   HasValue<IndexedName,   string>("find-me2");
        query3  = store.Query().                            HasValue<IndexedName,   string>("find-me2");
        
        AreEqual(1, query1.Entities.Count);     AreEqual("{ 1 }",           query1.Entities.ToStr());
        AreEqual(0, query2.Entities.Count);     AreEqual("{ }",             query2.Entities.ToStr());
        AreEqual(1, query3.Entities.Count);     AreEqual("{ 1 }",           query3.Entities.ToStr());
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
        
        var values = store.GetLinkedEntities<LinkComponent>();
        
        entity1.AddComponent(new LinkComponent { entity = target4 });   AreEqual(1, values.Count);
        entity2.AddComponent(new LinkComponent { entity = target5 });   AreEqual(2, values.Count);
        entity3.AddComponent(new LinkComponent { entity = target5 });   AreEqual(2, values.Count);

        int count = 0;
        foreach (var entity in values) {
            switch (count++) {
                case 0: AreEqual(4, entity.Id); break;
                case 1: AreEqual(5, entity.Id); break;
            } 
        }
        AreEqual(2, count);
        
        var query1  = store.Query().HasValue<LinkComponent,   Entity>(target4);
        var query2  = store.Query().HasValue<LinkComponent,   Entity>(target4).
                                    HasValue<LinkComponent,   Entity>(target5);
        
        AreEqual("{ 1 }",           query1.Entities.ToStr());
        AreEqual("{ 1, 2, 3 }",     query2.Entities.ToStr());
        
        var references4 = target4.GetLinkingEntities<LinkComponent>();
        AreEqual("{ 1 }",           references4.Ids.ToStr());
        
        var references5 = target5.GetLinkingEntities<LinkComponent>();
        AreEqual("{ 2, 3 }",        references5.Ids.ToStr());
        
        entity2.AddComponent(new LinkComponent { entity = target6 });   AreEqual(3, values.Count);
        references5 = target5.GetLinkingEntities<LinkComponent>();
        AreEqual("{ 3 }",           references5.Ids.ToStr());
        
        entity2.AddComponent(new LinkComponent { entity = target6 });   AreEqual(3, values.Count);
        references5 = target5.GetLinkingEntities<LinkComponent>();
        AreEqual("{ 3 }",           references5.Ids.ToStr());
        
        entity3.RemoveComponent<LinkComponent>();                       AreEqual(2, values.Count);
        references5 = target5.GetLinkingEntities<LinkComponent>();
        AreEqual("{ }",             references5.Ids.ToStr());
    }
    
    [Test]
    public static void Test_Index_support_null()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        entity1.AddComponent(new IndexedName { name = null });
        entity2.AddComponent(new IndexedName { name = null });
        
        var start = Mem.GetAllocatedBytes();
        var result = store.GetEntitiesWithComponentValue<IndexedName, string>(null);
        Mem.AssertNoAlloc(start);
        
        AreEqual(2, result.Count);
        AreEqual(1, result[0].Id);
        AreEqual(2, result[1].Id);
        
        entity2.RemoveComponent<IndexedName>();
        result = store.GetEntitiesWithComponentValue<IndexedName, string>(null);
        AreEqual(1, result.Count);
    }
    
    [Test]
    public static void Test_Index_ValueStructIndex()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity();
        entity1.AddComponent(new IndexedInt { value = 123 });
        entity1.AddComponent(new IndexedInt { value = 123 }); // add same component value again
        var result = store.GetEntitiesWithComponentValue<IndexedInt, int>(123);
        AreEqual(1, result.Count);
        
        entity1.AddComponent(new IndexedInt { value = 456 });
        result = store.GetEntitiesWithComponentValue<IndexedInt, int>(456);
        AreEqual(1, result.Count);
        result = store.GetEntitiesWithComponentValue<IndexedInt, int>(123);
        AreEqual(0, result.Count);
    }
    
    [Test]
    public static void Test_Index_already_added()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        
        entity1.AddComponent(new IndexedName { name = "added" });
        entity2.AddComponent(new IndexedName { name = null });
        
        var index = (ValueClassIndex<string>)StoreIndex.GetIndex(store, StructInfo<IndexedName>.Index);
        index.Add(1, new IndexedName { name = "added" });
        index.Add(2, new IndexedName { name = null    });

        AreEqual(2, index.Count);
    }
    
    [Test]
    public static void Test_Index_already_removed()
    {
        var index = new ValueClassIndex<string>();
        index.RemoveComponentValue(1, "missing");   // add key with default IdArray
        AreEqual(0, index.Count);
        
        index.RemoveComponentValue(2, null);
        AreEqual(0, index.Count);
    }
    
    [Test]
    public static void Test_Index_Perf()
    {
        int count       = 100;
        // 1_000_000  #PC    Test_Index_Allocation - count: 1000000 duration: 176 ms
        var store       = new EntityStore();
        var entities    = new List<Entity>();
        var values      = store.GetIndexedComponentValues<IndexedInt, int>();
        for (int n = 1; n <= count; n++) {
            entities.Add(store.CreateEntity());
        }
        for (int n = 0; n < count; n++) {
            entities[n].AddComponent(new IndexedInt { value = n });
        }
        for (int n = 0; n < count; n++) {
            entities[n].AddComponent(new IndexedInt { value = n + count });
        }
        AreEqual(count, values.Count);
        var sw = new Stopwatch();
        sw.Start();
        var start = Mem.GetAllocatedBytes();
        for (int n = 0; n < count; n++) {
            entities[n].AddComponent(new IndexedInt { value = n });
        }
        Mem.AssertNoAlloc(start);
        Console.WriteLine($"Test_Index_Perf - count: {count} duration: {sw.ElapsedMilliseconds} ms");
        AreEqual(count, values.Count);
    }
    
    [Test]
    public static void Test_Index_Perf_Reference()
    {
        int count       = 100;
        // 1_000_000  #PC    Test_Index_Perf_Reference - count: 1000000 duration: 18 ms
        var store       = new EntityStore();
        var entities    = new List<Entity>();
        for (int n = 1; n <= count; n++) {
            entities.Add(store.CreateEntity());
        }
        for (int n = 0; n < count; n++) {
            entities[n].AddComponent(new General.MyComponent { value = n });
        }
        var sw = new Stopwatch();
        sw.Start();
        var start = Mem.GetAllocatedBytes();
        for (int n = 0; n < count; n++) {
            entities[n].AddComponent(new  General.MyComponent { value = n });
        }
        Mem.AssertNoAlloc(start);
        Console.WriteLine($"Test_Index_Perf_Reference - count: {count} duration: {sw.ElapsedMilliseconds} ms");
    }
    
    [Test]
    public static void Test_Index_StoreIndex_ToString()
    {
        var store       = new EntityStore();
        var entity1     = store.CreateEntity();
        var entity2     = store.CreateEntity();
        entity1.AddComponent(new IndexedName { name = "test" });
        
        var indexMap    = store.extension.indexMap;
        AreEqual(null,          indexMap[StructInfo<Position>.Index]);

        AreEqual("IndexedName - ValueClassIndex`1 count: 1", indexMap[StructInfo<IndexedName>.Index].ToString());
        
        entity1.AddComponent(new LinkComponent { entity = entity2 });
        AreEqual("LinkComponent - EntityIndex count: 1", indexMap[StructInfo<LinkComponent>.Index].ToString());
        
        entity1.AddComponent(new IndexedInt { value = 42 });
        AreEqual("IndexedInt - ValueStructIndex`1 count: 1", indexMap[StructInfo<IndexedInt>.Index].ToString());
    }
    
    [Test]
    public static void Test_Index_EntityIndexValue()
    {
        var index       = new EntityIndex();
        var values      = new EntityIndexValues(index) as IEnumerable;

        Throws<NotImplementedException>(() => {
            // ReSharper disable once NotDisposedResource
            values.GetEnumerator();
        });
        IEnumerator e = new EntityIndexValuesEnumerator(index);
        Throws<NotImplementedException>(() => {
            _ = e.Current;
        });
        Throws<NotImplementedException>(() => {
            e.Reset();
        });
    }
    


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
