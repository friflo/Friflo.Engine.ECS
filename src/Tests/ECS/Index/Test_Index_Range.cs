using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index {
    
public static class Test_Index_Range
{
    [Test]
    public static void Test_Index_Range_Query_ValueInRange()
    {
        var store = new EntityStore();
        
        var query0 = store.Query<IndexedIntRange, Position>().ValueInRange<IndexedIntRange, int>(0,    99);
        var query1 = store.Query<IndexedIntRange, Position>().ValueInRange<IndexedIntRange, int>(100, 100);
        var query2 = store.Query<IndexedIntRange, Position>().ValueInRange<IndexedIntRange, int>(100, 200);
        var query3 = store.Query<IndexedIntRange, Position>().ValueInRange<IndexedIntRange, int>(100, 300);
        var query4 = store.Query<IndexedIntRange, Position>().ValueInRange<IndexedIntRange, int>(900, 999);
        
        AreEqual(0, query1.Entities.Count);
        
        var entity1 = store.CreateEntity(new Position());
        var entity2 = store.CreateEntity(new Position());
        var entity3 = store.CreateEntity(new Position());
        
        var values = store.GetAllIndexedComponentValues<IndexedIntRange, int>();

        entity1.AddComponent(new IndexedIntRange { value  = 100 });     AreEqual("{ 100 }",             values.Debug());
        entity2.AddComponent(new IndexedIntRange { value  = 200 });     AreEqual("{ 100, 200 }",        values.Debug());
        entity3.AddComponent(new IndexedIntRange { value  = 300 });     AreEqual("{ 100, 200, 300 }",   values.Debug());
        
        var result = store.GetEntitiesWithComponentValue<IndexedIntRange, int>(100);
        AreEqual(1, result.Count);     AreEqual("{ 1 }",    result.Ids.Debug());
        result     = store.GetEntitiesWithComponentValue<IndexedIntRange, int>(42);
        AreEqual(0, result.Count);     AreEqual("{ }",      result.Ids.Debug());
        {
            int count = 0;
            query3.ForEachEntity((ref IndexedIntRange _, ref Position _, Entity entity) => {
                AreEqual(++count, entity.Id);
            });
            AreEqual(3, count);
        }
        AreEqual(0, query0.Entities.Count);     AreEqual("{ }",         query0.Entities.Debug());
        AreEqual(1, query1.Entities.Count);     AreEqual("{ 1 }",       query1.Entities.Debug());
        AreEqual(2, query2.Entities.Count);     AreEqual("{ 1, 2 }",    query2.Entities.Debug());
        AreEqual(3, query3.Entities.Count);     AreEqual("{ 1, 2, 3 }", query3.Entities.Debug());
        AreEqual(0, query4.Entities.Count);     AreEqual("{ }",         query4.Entities.Debug());
        
        var start = Mem.GetAllocatedBytes();
        Mem.AreEqual(0, query0.Entities.Count);
        Mem.AreEqual(1, query1.Entities.Count);
        Mem.AreEqual(2, query2.Entities.Count);
        Mem.AreEqual(3, query3.Entities.Count);
        Mem.AreEqual(0, query4.Entities.Count);
        Mem.AssertNoAlloc(start);
        
    }
    
    [Test]
    public static void Test_Index_Range_Query_HasValue()
    {
        var store = new EntityStore();
        var entity1 = store.CreateEntity(new Position());
        
        entity1.AddComponent(new IndexedIntRange { value  = 100 });
        
        var query1 = store.Query<IndexedIntRange, Position>().HasValue<IndexedIntRange, int>(100);
        var query2 = store.Query<IndexedIntRange, Position>().HasValue<IndexedIntRange, int>(42);
        {
            int count = 0;
            query1.ForEachEntity((ref IndexedIntRange intRange, ref Position _, Entity entity) => {
                AreEqual(1,   entity.Id);
                AreEqual(100, intRange.value);
                ++count;
            });
            AreEqual(1, count);
        } {
            int count = 0;
            query2.ForEachEntity((ref IndexedIntRange _, ref Position _, Entity _) => {
                ++count;
            });
            AreEqual(0, count);
        }
        AreEqual(1, query1.Entities.Count);     AreEqual("{ 1 }",   query1.Entities.Debug());
        AreEqual(0, query2.Entities.Count);     AreEqual("{ }",     query2.Entities.Debug());
    }
    
    [Test]
    public static void Test_Index_Range_Update_Remove()
    {
        var store = new EntityStore();
        var entity1 = store.CreateEntity(new Position());
        var entity2 = store.CreateEntity(new Position());
        var entity3 = store.CreateEntity(new Position());
        var entity4 = store.CreateEntity(new Position());
        
        var values = store.GetAllIndexedComponentValues<IndexedIntRange, int>();
        entity1.AddComponent(new IndexedIntRange { value  = 100 });     AreEqual("{ 100 }",             values.Debug());
        entity2.AddComponent(new IndexedIntRange { value  = 200 });     AreEqual("{ 100, 200 }",        values.Debug());
        entity3.AddComponent(new IndexedIntRange { value  = 200 });     AreEqual("{ 100, 200 }",        values.Debug());
        entity4.AddComponent(new IndexedIntRange { value  = 300 });     AreEqual("{ 100, 200, 300 }",   values.Debug());
        entity4.AddComponent(new IndexedIntRange { value  = 300 });     AreEqual("{ 100, 200, 300 }",   values.Debug()); // cover add same component value again
        
        var query1 = store.Query<IndexedIntRange, Position>().ValueInRange<IndexedIntRange, int>(100, 300);
        var query2 = store.Query<IndexedIntRange, Position>().ValueInRange<IndexedIntRange, int>(200, 400);
        
        AreEqual(4, query1.Entities.Count);     AreEqual("{ 1, 2, 3, 4 }",  query1.Entities.Debug());
        AreEqual(3, query2.Entities.Count);     AreEqual("{ 2, 3, 4 }",     query2.Entities.Debug());
        
        entity1.AddComponent(new IndexedIntRange { value  = 400 });     AreEqual("{ 400, 200, 300 }", values.Debug());
        AreEqual(3, query1.Entities.Count);     AreEqual("{ 2, 3, 4 }",     query1.Entities.Debug());
        AreEqual(4, query2.Entities.Count);     AreEqual("{ 2, 3, 4, 1 }",  query2.Entities.Debug());
        
        entity2.RemoveComponent<IndexedIntRange>();                     AreEqual("{ 400, 200, 300 }", values.Debug());
        AreEqual(2, query1.Entities.Count);     AreEqual("{ 3, 4 }",        query1.Entities.Debug());
        AreEqual(3, query2.Entities.Count);     AreEqual("{ 3, 4, 1 }",     query2.Entities.Debug());
    }
    
    [Test]
    public static void Test_Index_Range_coverage() {
        _ = new ComponentIndexAttribute(null);    
    }
    
    
    // Indexing using range index currently allocates memory when using value types like int.
    // One reason is that SortedList<,>.TryGetValue() does boxing when using:
    // Array.BinarySearch<T>(T[] array, int index, int length, T value, IComparer<T>? comparer).
    // E.g. BinarySearch<> does boxing at
    // https://github.com/dotnet/corert/blob/master/src/System.Private.CoreLib/shared/System/Collections/Generic/ArraySortHelper.cs#L353
    [Test]
    public static void Test_Index_Range_Allocation()
    {
        var count       = 100;
        var store       = new EntityStore();
        var entities    = new List<Entity>();
        var values      = store.GetAllIndexedComponentValues<IndexedStringRange, string>();
        var strings     = new string[count];
        for (int n = 1; n <= count; n++) {
            entities.Add(store.CreateEntity());
        }
        for (int n = 0; n < count; n++) {
            strings[n] = n.ToString();
            entities[n].AddComponent(new IndexedStringRange { value = strings[n] });
        }
        // var start = Mem.GetAllocatedBytes();
        for (int n = 0; n < count; n++) {
            entities[n].AddComponent(new IndexedStringRange { value = strings[count - n - 1] });
        }
        // Mem.AssertNoAlloc(start);
        AreEqual(count, values.Count);
        
        var query = store.Query().ValueInRange<IndexedStringRange, string>("0", "z");
        AreEqual(count, query.Count);
    }
    
    [Test]
    public static void Test_Index_Allocation()
    {
        var count       = 100;
        var store       = new EntityStore();
        var entities    = new List<Entity>();
        var values      = store.GetAllIndexedComponentValues<IndexedIntRange, int>();
        for (int n = 1; n <= count; n++) {
            entities.Add(store.CreateEntity());
        }
        for (int n = 0; n < count; n++) {
            entities[n].AddComponent(new IndexedIntRange { value = count + n });
        }
        var start = Mem.GetAllocatedBytes();
        for (int n = 0; n < count; n++) {
            entities[n].AddComponent(new IndexedIntRange { value = n });
        }
        Mem.AssertNoAlloc(start);
        AreEqual(count, values.Count);
        
        var query = store.Query().ValueInRange<IndexedIntRange, int>(10, 19);
        AreEqual(10, query.Count);
    }
    
    [Test]
    public static void Test_Index_Range_Array_BinarySearch()
    {
        var values = new int [] { 1, 2, 3, 4,    6, 7, 8, 9, 10 };
        Array.BinarySearch(values, 0, values.Length, 5); // force one time allocations
        var start = Mem.GetAllocatedBytes();
        var index = Array.BinarySearch(values, 0, values.Length, 5);
        Mem.AssertNoAlloc(start);
        AreEqual(-5, index);
    }
}

}
