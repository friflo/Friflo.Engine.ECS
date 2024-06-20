using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;
using NUnit.Framework;
using Tests.ECS;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Internal.ECS {
public static class Test_Index_Range
{
    [CodeCoverageTest]
    [ComponentIndex(typeof(ValueInRangeIndex<>))]
    private struct IndexedIntRange : IIndexedComponent<int> {
        public      int     GetIndexedValue() => value;
        internal    int     value;
    
        public override string ToString() => value.ToString();
    }
    
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
        
        entity1.AddComponent(new IndexedIntRange { value  = 100 });
        entity2.AddComponent(new IndexedIntRange { value  = 200 });
        entity3.AddComponent(new IndexedIntRange { value  = 300 });
        
        var result = store.GetEntitiesWithComponentValue<IndexedIntRange, int>(100);
        AreEqual(1, result.Count);     AreEqual(new int[] { 1 },    result.Ids.ToArray());
        result     = store.GetEntitiesWithComponentValue<IndexedIntRange, int>(42);
        AreEqual(0, result.Count);     AreEqual(new int[] { },      result.Ids.ToArray());
        {
            int count = 0;
            query3.ForEachEntity((ref IndexedIntRange _, ref Position _, Entity entity) => {
                AreEqual(++count, entity.Id);
            });
            AreEqual(3, count);
        }
        AreEqual(0, query0.Entities.Count);     AreEqual(new int[] {         }, query0.Entities.ToIds());
        AreEqual(1, query1.Entities.Count);     AreEqual(new int[] { 1       }, query1.Entities.ToIds());
        AreEqual(2, query2.Entities.Count);     AreEqual(new int[] { 1, 2    }, query2.Entities.ToIds());
        AreEqual(3, query3.Entities.Count);     AreEqual(new int[] { 1, 2, 3 }, query3.Entities.ToIds());
        AreEqual(0, query4.Entities.Count);     AreEqual(new int[] {         }, query4.Entities.ToIds());
        
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
        AreEqual(1, query1.Entities.Count);     AreEqual(new int[] { 1       }, query1.Entities.ToIds());
        AreEqual(0, query2.Entities.Count);     AreEqual(new int[] {         }, query2.Entities.ToIds());
    }
    
    [Test]
    public static void Test_Index_Range_Update_Remove()
    {
        var store = new EntityStore();
        var entity1 = store.CreateEntity(new Position());
        var entity2 = store.CreateEntity(new Position());
        var entity3 = store.CreateEntity(new Position());
        var entity4 = store.CreateEntity(new Position());
        
        entity1.AddComponent(new IndexedIntRange { value  = 100 });
        entity2.AddComponent(new IndexedIntRange { value  = 200 });
        entity3.AddComponent(new IndexedIntRange { value  = 200 });
        entity4.AddComponent(new IndexedIntRange { value  = 300 });
        entity4.AddComponent(new IndexedIntRange { value  = 300 }); // cover add same component value again
        
        var query1 = store.Query<IndexedIntRange, Position>().ValueInRange<IndexedIntRange, int>(100, 300);
        var query2 = store.Query<IndexedIntRange, Position>().ValueInRange<IndexedIntRange, int>(200, 400);
        
        AreEqual(4, query1.Entities.Count);     AreEqual(new int[] { 1, 2, 3, 4 }, query1.Entities.ToIds());
        AreEqual(3, query2.Entities.Count);     AreEqual(new int[] {    2, 3, 4 }, query2.Entities.ToIds());
        
        entity1.AddComponent(new IndexedIntRange { value  = 400 });
        AreEqual(3, query1.Entities.Count);     AreEqual(new int[] {    2, 3, 4 }, query1.Entities.ToIds());
        AreEqual(4, query2.Entities.Count);     AreEqual(new int[] { 2, 3, 4, 1 }, query2.Entities.ToIds());
        
        entity2.RemoveComponent<IndexedIntRange>();
        AreEqual(2, query1.Entities.Count);     AreEqual(new int[] {       3, 4 }, query1.Entities.ToIds());
        AreEqual(3, query2.Entities.Count);     AreEqual(new int[] {    3, 4, 1 }, query2.Entities.ToIds());
    }
    
    [Test]
    public static void Test_Index_Range_coverage() {
        _ = new ComponentIndexAttribute(null);    
    }
    
    [Test]
    public static void Test_Index_Range_already_added()
    {
        var world   = new EntityStore();
        var entity  = world.CreateEntity(1);
        
        entity.AddComponent(new IndexedIntRange { value =  456 });
        
        var index = (ValueInRangeIndex<int>)world.extension.componentIndexes[StructInfo<IndexedIntRange>.Index];
        index.Add(1, new IndexedIntRange { value = 456 });
        AreEqual(1, index.Count);
    }
    
    [Test]
    public static void Test_Index_Range_already_removed()
    {
        var map = new SortedList<string, IdArray>();
        var arrayHeap = new IdArrayHeap();
        SortedListUtils.RemoveComponentValue(1, "missing", map, arrayHeap);
        AreEqual(0, map.Count);
    }
}

}
