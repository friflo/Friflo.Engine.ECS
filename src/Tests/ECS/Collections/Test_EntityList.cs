using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Collections {

public static class Test_EntityList
{
    [Test]
    public static void Test_EntityList_SetStore()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var list    = new EntityList();
        IsNull(list.EntityStore);
        var entity  = store.CreateEntity();
        list.SetStore(store);
        AreSame(store, list.EntityStore);
        list.Add(entity);
        AreEqual(1, list.Count);
    }
    
    [Test]
    public static void Test_EntityList_Add()
    {
        var store   = new EntityStore();
        AreEqual(128, store.Capacity);
        // ReSharper disable once UseObjectOrCollectionInitializer
        var list = new EntityList(store);
        list.Add(0);
        IsTrue(list[0].IsNull);
        var e = Throws<ArgumentException>(() => {
            list.Add(128);
        });
        AreEqual("id: 128. expect in [0, current max id: 127]", e!.Message);
        e = Throws<ArgumentException>(() => {
            list.Add(-1);
        });
        AreEqual("id: -1. expect in [0, current max id: 127]", e!.Message);
    }
    
    [Test]
    public static void Test_EntityList_Capacity()
    {
        var store   = new EntityStore();
        var list = new EntityList(store) { Capacity = 10 };
        AreEqual(10, list.Capacity);
        var start = Mem.GetAllocatedBytes();
        for (int n = 1; n <= 10; n ++) {
            list.Add(1);
        }
        Mem.AssertNoAlloc(start);
        
        // set capacity less than current capacity
        list.Capacity = 5;
        AreEqual(10, list.Capacity);
    }
    
    [Test]
    public static void Test_EntityList_AddTreeEntities()
    {
        var count       = 10;   // 1_000_000 ~ #PC: 7715 ms
        var entityCount = 100;
        
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity();
        var arch2   = store.GetArchetype(ComponentTypes.Get<Position, Rotation>());
        var arch3   = store.GetArchetype(ComponentTypes.Get<Position, Rotation>(), Tags.Get<Disabled>());
        
        for (int n = 1; n < entityCount; n++) {
            root.AddChild(arch2.CreateEntity());
        }
        var list = new EntityList(store);
        
        var sw = new Stopwatch();
        sw.Start();
        long start = 0; 
        var tags = Tags.Get<Disabled>();
        for (int n = 0; n < count; n++) {
            list.Clear();
            list.AddTree(root);
            list.ApplyRemoveTags(tags);
            list.ApplyAddTags(tags);
            if (n == 0) start = Mem.GetAllocatedBytes();
        }
        Mem.AssertNoAlloc(start);
        Console.WriteLine($"AddTreeEntities - duration: {sw.ElapsedMilliseconds} ms");
        AreEqual(entityCount, list.Count);
        AreEqual(entityCount, list.Ids.Length);
        
        var query = store.Query();
        AreEqual(0,                 query.Count);
        
        var disabled = store.Query().WithDisabled();
        AreEqual(entityCount,       disabled.Count);
        
        AreEqual(entityCount,       store.Count);
        AreEqual(0,                 arch2.Count);
        AreEqual(entityCount - 1,   arch3.Count);
        IsFalse (root.Enabled);
    }
    
    [Test]
    public static void Test_EntityList_ApplyBatch()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var entity = store.CreateEntity(1);
        
        var list = new EntityList(store);
        list.Add(entity.Id);
        
        var batch = new EntityBatch();
        batch.Disable();
        batch.Add(new Position());
        list.ApplyBatch(batch);
        AreEqual("id: 1  [Position, #Disabled]", entity.ToString());
        
        batch.Enable();
        batch.Remove<Position>();
        list.ApplyBatch(batch);
        AreEqual("id: 1  []", entity.ToString());
    }
    
    [Test]
    public static void Test_EntityList_Enumerator()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var list    = new EntityList(store);
        list.Add(store.CreateEntity(1).Id);
        list.Add(store.CreateEntity(2).Id);
        
        AreEqual("Count: 2",    list.ToString());
        AreEqual(2,             list.Count);
        AreEqual(1,             list[0].Id);
        AreEqual(2,             list[1].Id);
        Throws<IndexOutOfRangeException>(() => {
            _ = list[2];
        });
        {
            int count = 0;
            foreach (var entity in list) {
                AreEqual(++count, entity.Id);
            }
            AreEqual(2, count);
        }
        {
            int count = 0;
            IEnumerable<Entity> enumerable = list;
            foreach (var entity in enumerable) {
                AreEqual(++count, entity.Id);
            }
            AreEqual(2, count);
        }
        {
            int count = 0;
            IEnumerable enumerable = list;
            var enumerator = enumerable.GetEnumerator();
            using var unknown = enumerator as IDisposable;
            enumerator.Reset();
            while (enumerator.MoveNext()) {
                var entity = (Entity)enumerator.Current!;
                AreEqual(++count, entity.Id);
            }
            AreEqual(2, count);
        }
    }
    
    [Test]
    public static void Test_EntityList_IList()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var list    = new EntityList(store);
        
        IsFalse(list.IsReadOnly);
        
        for (int n = 0; n < 100; n++) {
            var entity = store.CreateEntity();
            list.Add(entity);
        }
        var target = new Entity[100];
        list.CopyTo(target, 0);
        AreEqual(list, target);
        
        list[1] = list[0];
        AreEqual(list[1], list[0]);
    }
    
    [Test]
    public static void Test_EntityList_IList_mutate()
    {
        var store   = new EntityStore();
        var list    = new EntityList(store);

        for (int n = 0; n < 5; n++) {
            var entity = store.CreateEntity(n + 1);
            list.Add(entity);
        }
        // --- RemoveAt()
        list.RemoveAt(1);
        AreEqual("{ 1, 3, 4, 5 }", list.Debug());
        
        Throws<IndexOutOfRangeException>(() => {
            list.RemoveAt(4);
        });
        // --- Insert
        var entity6 = store.CreateEntity(6);
        list.Insert(3, entity6);
        AreEqual("{ 1, 3, 4, 6, 5 }", list.Debug());
        
        Throws<IndexOutOfRangeException>(() => {
            list.RemoveAt(5);
        });
        // --- IndexOf() / Contains()
        AreEqual(3, list.IndexOf(entity6));
        IsTrue  (   list.Contains(entity6));
        
        var entity7 = store.CreateEntity(7);
        AreEqual(-1, list.IndexOf(entity7));
        IsFalse (    list.Contains(entity7));
        
        // --- Remove()
        IsTrue(list.Remove(entity6));
        AreEqual("{ 1, 3, 4, 5 }", list.Debug());
        
        IsFalse(list.Remove(entity7));
    }
    
    [Test]
    public static void Test_EntityList_exception()
    {
        var store1  = new EntityStore(PidType.RandomPids);
        var store2  = new EntityStore(PidType.RandomPids);
        var entity1 = store1.CreateEntity();
        var entity2 = store2.CreateEntity();
        var list    = new EntityList(store2);
        
        var e = Throws<ArgumentException>(() => {
            list.AddTree(entity1);
        });
        AreEqual("entity is owned by a different store (Parameter 'entity')", e!.Message);
        
        e = Throws<ArgumentException>(() => {
            list.Add(entity1);
        });
        AreEqual("entity is owned by a different store (Parameter 'entity')", e!.Message);
        
        list.Add(entity2);
        e = Throws<ArgumentException>(() => {
            list.SetStore(store1);
        });
        AreEqual("EntityList must be empty when calling SetStore()", e!.Message);
    }
    
#pragma warning disable CS0618 // Type or member is obsolete

    [Test]
    public static void Test_EntityList_Sort_field()
    {
        var store       = new EntityStore();
        store.CreateEntity();
        for (int n = 0; n < 10; n++) {
            store.CreateEntity(new MyComponent1 { a = n });    
        }
        
        var query   = store.Query();
        var list    = query.ToEntityList();
        var fields  = new ComponentField<int>[10]; 
        fields = list.SortByComponentField<MyComponent1, int>(nameof(MyComponent1.a), SortOrder.Descending, fields);
        
        AreEqual(11,    list [0].Id);
        AreEqual(2,     list [9].Id);
        AreEqual(1,     list[10].Id);
        
        AreEqual("id: 11, value: 9",    fields[0].ToString());
        AreEqual("id: 2, value: 0",     fields[9].ToString());
        AreEqual("id: 1, value: null",  fields[10].ToString());
        
        fields = list.SortByComponentField<MyComponent1, int>(nameof(MyComponent1.a), SortOrder.Ascending, fields);
        
        AreEqual(1,     list [0].Id);
        AreEqual(2,     list [1].Id);
        AreEqual(11,    list[10].Id);
        
        AreEqual("id: 1, value: null",  fields[0].ToString());
        AreEqual("id: 2, value: 0",     fields[1].ToString());
        AreEqual("id: 11, value: 9",    fields[10].ToString());
        
        var start = Mem.GetAllocatedBytes();
        for (int n = 0; n < 10; n++) {
            fields = list.SortByComponentField<MyComponent1, int>(nameof(MyComponent1.a), SortOrder.Ascending, fields);
        }
        Mem.AssertNoAlloc(start);
        AreEqual(1,     list [0].Id);
        AreEqual(2,     list [1].Id);
        AreEqual(11,    list[10].Id);
    }
    
    [Test]
    public static void Test_EntityList_Sort_property()
    {
        var store       = new EntityStore();
        store.CreateEntity();
        for (int n = 0; n < 10; n++) {
            store.CreateEntity(new MyPropertyComponent { value = n });    
        }
        
        var query   = store.Query();
        var list    = query.ToEntityList();
        var fields  = new ComponentField<int>[10]; 
        fields = list.SortByComponentField<MyPropertyComponent, int>("value", SortOrder.Descending, fields);
        
        AreEqual(11,    list [0].Id);
        AreEqual(2,     list [9].Id);
        AreEqual(1,     list[10].Id);
        
        AreEqual("id: 11, value: 9",    fields[0].ToString());
        AreEqual("id: 2, value: 0",     fields[9].ToString());
        AreEqual("id: 1, value: null",  fields[10].ToString());
        
        AreEqual(11,    fields[0].entityId);
        AreEqual(9,     fields[0].field);
        AreEqual(1,     fields[0].hasField);
        
        var start = Mem.GetAllocatedBytes();
        for (int n = 0; n < 10; n++) {
            fields = list.SortByComponentField<MyPropertyComponent, int>("value", SortOrder.Ascending, fields);
        }
        Mem.AssertNoAlloc(start);
    }
    
    [Test]
    public static void Test_EntityList_Sort_enum()
    {
        var store       = new EntityStore();
        store.CreateEntity();
        for (int n = 0; n < 10; n++) {
            store.CreateEntity(new MyEnumComponent { value = (SortEnum)n });    
        }
        
        var query   = store.Query();
        var list    = query.ToEntityList();
        var fields  = new ComponentField<SortEnum>[10]; 
        fields = list.SortByComponentField<MyEnumComponent, SortEnum>("value", SortOrder.Descending, fields);
        
        AreEqual(11,    list [0].Id);
        AreEqual(2,     list [9].Id);
        AreEqual(1,     list[10].Id);
        
        AreEqual("id: 11, value: Value9",   fields[0].ToString());
        AreEqual("id: 2, value: Value0",    fields[9].ToString());
        AreEqual("id: 1, value: null",      fields[10].ToString());
        
        AreEqual(11,                fields[0].entityId);
        AreEqual(SortEnum.Value9,   fields[0].field);
        AreEqual(1,                 fields[0].hasField);
        
        var start = Mem.GetAllocatedBytes();
        for (int n = 0; n < 10; n++) {
            fields = list.SortByComponentField<MyEnumComponent, SortEnum>("value", SortOrder.Ascending, fields);
        }
        Mem.AssertNoAlloc(start);
    }
    
    [Test]
    public static void Test_EntityList_Sort_Entity()
    {
        var store       = new EntityStore();
        store.CreateEntity();
        var entities = new List<Entity>();
        
        for (int n = 0; n < 20; n++) {
            entities.Add(store.CreateEntity());
        }
        for (int n = 0; n < 10; n++) {
            entities[n].AddComponent(new EntityReference { entity = entities[10 - n]});
        }
        entities[10].AddComponent(new EntityReference()); // add component with default entity
        entities[11].AddComponent(new EntityReference()); // add component with default entity
        
        var query   = store.Query();
        var list    = query.ToEntityList();
        var fields  = new ComponentField<Entity>[10]; 
        fields = list.SortByComponentField<EntityReference, Entity>("entity", SortOrder.Descending, fields);
        
        AreEqual("{ 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 12, 14, 15, 16, 17, 18, 19, 20, 21, 1 }", list.Debug());
        
        AreEqual("id: 2, value: id: 12  [EntityReference]", fields[0].ToString());
        AreEqual("id: 11, value: id: 3  [EntityReference]", fields[9].ToString());
        AreEqual("id: 13, value: null",                     fields[10].ToString());
        
        var start = Mem.GetAllocatedBytes();
        for (int n = 0; n < 10; n++) {
            fields = list.SortByComponentField<EntityReference, Entity>("entity", SortOrder.Ascending, fields);
        }
        Mem.AssertNoAlloc(start);
        AreEqual("{ 1, 20, 19, 18, 17, 16, 15, 14, 21, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 }", list.Debug());
    }
    
    [Test]
    public static void Test_EntityList_Sort_Perf()
    {
        int count   = 10; // 1_000_000;
        int repeat  = 1000;
        // Test_EntityList_Sort_Perf - count: 1000000, repeat: 1000, stopWatch: 2149 ms
        var store   = new EntityStore();
        store.CreateEntity();
        for (int n = 0; n < count; n++) {
            store.CreateEntity(new MyComponent1 { a = n });
        }
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var query   = store.Query();
        var list    = query.ToEntityList();
        var fields  = Array.Empty<ComponentField<int>>();
        for (int n = 0; n < repeat; n++) {
            fields = list.SortByComponentField<MyComponent1, int>(nameof(MyComponent1.a), SortOrder.Ascending, fields);
        }
        Console.WriteLine($"Test_EntityList_Sort_Perf - count: {count}, repeat: {repeat}, stopWatch: {stopWatch.ElapsedMilliseconds} ms");
    }
        
}

}
