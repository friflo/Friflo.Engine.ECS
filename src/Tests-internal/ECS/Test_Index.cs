using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;
using NUnit.Framework;
using Tests.ECS.Index;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Internal.ECS {


[SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
[SuppressMessage("Performance", "CA1825:Avoid zero-length array allocations")]
public static class Test_Index
{
    [Test]
    public static void Test_Index_ValueInRange_EntityIndex()
    {
        var store       = new EntityStore();
        var entityIndex = new EntityIndex<AttackComponent> { store = store };
        var e = Throws<NotSupportedException>(() => {
            entityIndex.AddValueInRangeEntities(default, default, null);    
        });
        AreEqual("ValueInRange() not supported by EntityIndex`1", e!.Message);
    }
    
    [Test]
    public static void Test_Index_already_added()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        
        entity1.AddComponent(new IndexedName { name = "added" });
        entity2.AddComponent(new IndexedName { name = null });
        
        var index = (ValueClassIndex<IndexedName,string>)StoreIndex.GetIndex(store, StructInfo<IndexedName>.Index);
        index.Add(1, new IndexedName { name = "added" });
        index.Add(2, new IndexedName { name = null    });

        AreEqual(2, index.Count);
    }
    
    [Test]
    public static void Test_Index_already_removed()
    {
        var index = new ValueClassIndex<IndexedName,string>();
        index.RemoveComponentValue(1, "missing");   // add key with default IdArray
        AreEqual(0, index.Count);
        
        index.RemoveComponentValue(2, null);
        AreEqual(0, index.Count);
    }
    
    [Test]
    public static void Test_Index_StoreIndex_ToString()
    {
        var store       = new EntityStore();
        var entity1     = store.CreateEntity();
        var entity2     = store.CreateEntity();
        entity1.AddComponent(new IndexedName { name = "test" });
        
        var indexMap    = store.extension.indexMap;

        AreEqual("IndexedName - ValueClassIndex`2 count: 1", indexMap[StructInfo<IndexedName>.Index].ToString());
        
        entity1.AddComponent(new LinkComponent { entity = entity2 });
        AreEqual("LinkComponent - EntityIndex`1 count: 1",   indexMap[StructInfo<LinkComponent>.Index].ToString());
        
        entity1.AddComponent(new IndexedInt { value = 42 });
        AreEqual("IndexedInt - ValueStructIndex`2 count: 1", indexMap[StructInfo<IndexedInt>.Index].ToString());
    }
    
    [Test]
    public static void Test_Index_cover_AddComponentValue()
    {
        var store = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        entity1.AddComponent(new AttackComponent { target = entity2 });
        
        var index = (EntityIndex)store.extension.indexMap[StructInfo<AttackComponent>.Index];
        AreEqual(1, index.Count);
        EntityIndexUtils.AddComponentValue(1, 2, index);
        AreEqual(1, index.Count);
    }
    
    [Test]
    public static void Test_Index_EntityIndexValue()
    {
        var index       = new EntityIndex<AttackComponent>();
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
    public static void Test_Index_EntityNode()
    {
        var indexedIntType  = ComponentTypes.Get<IndexedInt>();
        var attackType      = ComponentTypes.Get<AttackComponent>();
        var node  = new EntityNode {
            isOwner     = (int)indexedIntType.bitSet.l0,
            isLinked    = (int)attackType.bitSet.l0,
        };
        AreEqual("Components: [IndexedInt]",        node.IsOwner.ToString());
        AreEqual("Components: [AttackComponent]",   node.IsLinked.ToString());
    }
}

}
