using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Collections;

public static class Test_EntityList
{
    [Test]
    public static void Test_EntityList_AddTreeEntities()
    {
        var count       = 10;   // 1_000_000 ~ #PC: 7860 ms
        var entityCount = 100;
        
        var store   = new EntityStore();
        var root    = store.CreateEntity();
        var arch    = store.GetArchetype(ComponentTypes.Get<Position, Rotation>());
        
        for (int n = 1; n < entityCount; n++) {
            root.AddChild(arch.CreateEntity());
        }
        var list = new EntityList(store);
        
        var sw = new Stopwatch();
        sw.Start();
        long start = 0; 
        var tags = Tags.Get<Disabled>();
        for (int n = 0; n < count; n++) {
            list.Clear();
            list.AddEntityTree(root);
            list.RemoveTags(tags);
            list.AddTags(tags);
            if (n == 0) start = Mem.GetAllocatedBytes();
        }
        Mem.AssertNoAlloc(start);
        Console.WriteLine($"AddTreeEntities - duration: {sw.ElapsedMilliseconds} ms");
        AreEqual(entityCount, list.Count);
        AreEqual(entityCount, list.Ids.Length);
        
        var query = store.Query().AllTags(Tags.Get<Disabled>());
        AreEqual(entityCount, query.Count);
        IsFalse (root.Enabled);
    }
    
    [Test]
    public static void Test_EntityList_ApplyBatch()
    {
        var store   = new EntityStore();
        var entity = store.CreateEntity(1);
        
        var list = new EntityList(store);
        list.AddEntity(entity.Id);
        
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
        list.AddEntity(store.CreateEntity(1).Id);
        list.AddEntity(store.CreateEntity(2).Id);
        
        AreEqual("Count: 2",    list.ToString());
        AreEqual(2,             list.Count);
        AreEqual(1,             list[0].Id);
        AreEqual(2,             list[1].Id);
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
}