using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

#pragma warning disable CA1861

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Internal.ECS
{

    public class Test_IdArray
    {
        [Test]
        public void Test_IdArray_Add()
        {
            var heap    = new IdArrayHeap();
            
            var array   = new IdArray();
            AreEqual("count: 0", array.ToString());
            AreEqual(0, array.Count);
            AreEqual(new int[] { }, array.GetIdSpan(heap).ToArray());

            array.AddId(100, heap);
            AreEqual(1, array.Count);
            AreEqual("count: 1  id: 100", array.ToString());
            var span = array.GetIdSpan(heap);
            AreEqual(new int[] { 100 }, span.ToArray());
            AreEqual(0, heap.Count);
            
            array.AddId(101, heap);
            AreEqual(2, array.Count);
            AreEqual("count: 2  index: 1  start: 0", array.ToString());
            AreEqual(new int[] { 100, 101 }, array.GetIdSpan(heap).ToArray());
            AreEqual(1, heap.Count);

            array.AddId(102, heap);
            AreEqual(3, array.Count);
            AreEqual(new int[] { 100, 101, 102 }, array.GetIdSpan(heap).ToArray());
            AreEqual(1, heap.Count);
            
            array.AddId(103, heap);
            AreEqual(4, array.Count);
            AreEqual(new int[] { 100, 101, 102, 103 }, array.GetIdSpan(heap).ToArray());
            AreEqual(1, heap.Count);
            
            array.AddId(104, heap);
            AreEqual(5, array.Count);
            AreEqual(new int[] { 100, 101, 102, 103, 104 }, array.GetIdSpan(heap).ToArray());
            AreEqual(1, heap.Count);
            AreEqual("count: 1", heap.ToString());
            
            AreEqual("arraySize: 2 count: 0", heap.GetPool(1).ToString());
            AreEqual("arraySize: 4 count: 0", heap.GetPool(2).ToString());
            AreEqual("arraySize: 8 count: 1", heap.GetPool(3).ToString());
        }
        
        [Test]
        public void Test_IdArray_Remove()
        {
            var heap    = new IdArrayHeap();
            {
                var array   = new IdArray();
                array.AddId(100, heap);
                array.RemoveAt(0, heap);
                AreEqual(0, array.Count);
            } {
                var array   = new IdArray();
                array.AddId(200, heap);
                array.AddId(201, heap);
                array.RemoveAt(0, heap);
                AreEqual(1, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual(new int[] { 201 }, ids.ToArray());
                AreEqual(0, heap.Count);
            } {
                var array   = new IdArray();
                array.AddId(300, heap);
                array.AddId(301, heap);
                array.RemoveAt(1, heap);
                AreEqual(1, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual(new int[] { 300 }, ids.ToArray());
                AreEqual(0, heap.Count);
            } {
                var array   = new IdArray();
                array.AddId(400, heap);
                array.AddId(401, heap);
                array.AddId(402, heap);
                array.RemoveAt(0, heap);
                AreEqual(2, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual(new int[] { 401, 402 }, ids.ToArray());
                AreEqual(1, heap.Count);
            } {
                var array   = new IdArray();
                array.AddId(500, heap);
                array.AddId(501, heap);
                array.AddId(502, heap);
                array.RemoveAt(2, heap);
                AreEqual(2, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual(new int[] { 500, 501 }, ids.ToArray());
                AreEqual(2, heap.Count);
            } {
                var array   = new IdArray();
                array.AddId(600, heap);
                array.AddId(601, heap);
                array.AddId(602, heap);
                array.AddId(603, heap);
                array.RemoveAt(0, heap);
                AreEqual(3, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual(new int[] { 601, 602, 603 }, ids.ToArray());
                AreEqual(3, heap.Count);
            } {
                var array   = new IdArray();
                array.AddId(700, heap);
                array.AddId(701, heap);
                array.AddId(702, heap);
                array.AddId(703, heap);
                array.RemoveAt(3, heap);
                AreEqual(3, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual(new int[] { 700, 701, 702 }, ids.ToArray());
                AreEqual(4, heap.Count);
            }
        }
        
        [Test]
        public void Test_IdArray_clear_freeList()
        {
            var heap    = new IdArrayHeap();
            var arrays  = new IdArray[100];
            int id      = 0;
            for (int n = 0; n < 100; n++) {
                ref var array = ref arrays[n]; 
                for (int i = 0; i < 10; i++) {
                    array.AddId(id++, heap);
                }
            }
            id = 0;
            for (int n = 0; n < 100; n++) {
                var span = arrays[n].GetIdSpan(heap);
                for (int i = 0; i < 10; i++) {
                    Mem.AreEqual(id++, span[i]);
                }
            }
            var pool16 = heap.GetPool(4);
            for (int n = 0; n < 100; n++) {
                Mem.AreEqual(n, pool16.FreeCount);
                ref var array = ref arrays[n]; 
                for (int i = 0; i < 10; i++) {
                    array.RemoveAt(array.Count - 1, heap);
                }
            }
            AreEqual(0, pool16.FreeCount);
        }
        
        [Test]
        public void Test_IdArray_exceptions()
        {
            var heap    = new IdArrayHeap();
            var array   = new IdArray();
            Throws<IndexOutOfRangeException>(() => {
                array.RemoveAt(0, heap);    
            });
        }
        
        [Test]
        public void Test_IdArray_Add_Remove_One_Perf()
        {
            int repeat  = 100; // 100_000_000;
            //  #PC: Add_Remove_One: repeat: 100000000 duration: 732 ms
            var heap    = new IdArrayHeap();
            var array   = new IdArray();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < repeat; i++) {
                array.AddId(42, heap);
                array.RemoveAt(0, heap);
            }
            Console.WriteLine($"Add_Remove_One: repeat: {repeat} duration: {sw.ElapsedMilliseconds} ms");
            AreEqual(0, heap.Count);
        }
        
        [Test]
        public void Test_IdArray_Add_Remove_Many_Perf()
        {
            int count   = 100; // 1_000_000
            int repeat  = 100;
            //  #PC: Add_Remove_Many_Perf - count: 1000000 repeat: 100 duration: 1472 ms
            var heap    = new IdArrayHeap();
            var array   = new IdArray();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < repeat; i++) {
                for (int n = 0; n < count; n++) {
                    array.AddId(n, heap);
                }
                for (int n = 0; n < count; n++) {
                    array.RemoveAt(array.Count - 1, heap);
                }
            }
            Console.WriteLine($"Add_Remove_Many_Perf - count: {count} repeat: {repeat} duration: {sw.ElapsedMilliseconds} ms");
            AreEqual(0, heap.Count);
        }
        
        [Test]
        public unsafe void Test_IdArray_size_of()
        {
            AreEqual(8, sizeof(IdArray));
        }
        
        [Test]
        public void Test_IdArray_LeadingZeroCount()
        {
            for (uint n = 0; n < 1000; n++) {
                AreEqual(System.Numerics.BitOperations.LeadingZeroCount(n), IdArrayHeap.LeadingZeroCount(n));    
            }
            for (uint n = int.MaxValue - 1000; n < int.MaxValue; n++) {
                AreEqual(System.Numerics.BitOperations.LeadingZeroCount(n), IdArrayHeap.LeadingZeroCount(n));    
            }
        }
        
        [Test]
        public void Test_IdArray_EntitySpan()
        {
            var store = new EntityStore();
            store.CreateEntity(1);
            store.CreateEntity(2);
            store.CreateEntity(42);

            // --- Length: 0
            var span0 = new EntitySpan(store, default, 0);
            AreEqual("Entity[0]", span0.ToString());
            AreEqual(0, span0.Length);
            var entities = span0.Entities;
            AreEqual(0,     entities.Count);
            AreSame (store, span0.Store);
            
            // --- Length: 1
            var span1 = new EntitySpan(store, default, 42);
            AreEqual("Entity[1]", span1.ToString());
            AreEqual(1, span1.Length);
            entities = span1.Entities;
            AreEqual(1,     entities.Count);
            AreSame (store, entities[0].Store);
            AreEqual(42,    entities[0].Id);
            
            // --- Length: 2
            var span2 = new EntitySpan(store, new int[] { 1, 2 }, 0);
            AreEqual("Entity[2]", span2.ToString());
            AreEqual(2, span2.Length);
            entities = span2.Entities;
            AreEqual(2,     entities.Count);
            AreSame (store, entities[0].Store);
            AreEqual(1,     entities[0].Id);
            AreEqual(2,     entities[1].Id);

            int count = 0;
            foreach (var entity in span2) {
                switch (count++) {
                    case 0: AreEqual(1, entity.Id); break;
                    case 1: AreEqual(2, entity.Id); break;
                }
            }
            AreEqual(2, count);
        }
    }
}