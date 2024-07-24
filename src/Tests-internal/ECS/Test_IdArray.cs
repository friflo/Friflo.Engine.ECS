using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Collections;
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
        public unsafe void Test_IdArray_sizeof()
        {
            // AreEqual(8, Marshal.SizeOf(typeof(IdArrayHeap))); // Important -> to use IdArrayHeap as a TreeNode field
            AreEqual(8, sizeof(IdArray));
        }
        
        [Test]
        public void Test_IdArray_IdArrayHeap() {
            IdArrayHeap heap = default;
            AreEqual("null", heap.ToString());
            
            heap    = new IdArrayHeap();
            AreEqual("count: 0", heap.ToString());
        }
    
        [Test]
        public void Test_IdArray_Add()
        {
            var store   = new EntityStore();
            var heap    = new IdArrayHeap();
            
            var array   = new IdArray();
            AreEqual("count: 0", array.ToString());
            AreEqual(0, array.Count);
            AreEqual("{ }", array.GetSpan(heap, store).Debug());

            array.Add(100, heap);
            AreEqual(1, array.Count);
            AreEqual("count: 1  id: 100", array.ToString());
            var span = array.GetSpan(heap, store);
            AreEqual("{ 100 }", span.Debug());
            AreEqual(0, heap.Count);
            
            array.Add(101, heap);
            AreEqual(2, array.Count);
            AreEqual("count: 2  index: 1  start: 0", array.ToString());
            AreEqual("{ 100, 101 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);

            array.Add(102, heap);
            AreEqual(3, array.Count);
            AreEqual("{ 100, 101, 102 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);
            
            array.Add(103, heap);
            AreEqual(4, array.Count);
            AreEqual("{ 100, 101, 102, 103 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);
            
            array.Add(104, heap);
            AreEqual(5, array.Count);
            AreEqual("{ 100, 101, 102, 103, 104 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);
            AreEqual("count: 1", heap.ToString());
            
            AreEqual("arraySize: 2 count: 0", heap.GetPool(1).ToString());
            AreEqual("arraySize: 4 count: 0", heap.GetPool(2).ToString());
            AreEqual("arraySize: 8 count: 1", heap.GetPool(3).ToString());
        }
        
        [Test]
        public void Test_IdArray_InsertAt()
        {
            var store   = new EntityStore();
            var heap    = new IdArrayHeap();
            
            var array   = new IdArray();
            AreEqual(0, array.Count);
            AreEqual("{ }", array.GetSpan(heap, store).Debug());

            array.InsertAt(0, 100, heap);
            AreEqual("{ 100 }", array.GetSpan(heap, store).Debug());
            AreEqual(0, heap.Count);
            
            array.InsertAt(0, 99, heap);
            AreEqual("{ 99, 100 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);
            
            array.RemoveAt(0, heap);
            AreEqual("{ 100 }", array.GetSpan(heap, store).Debug());
            AreEqual(0, heap.Count);
            
            array.InsertAt(1, 101, heap);
            AreEqual("{ 100, 101 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);

            array.InsertAt(0, 99, heap);
            AreEqual("{ 99, 100, 101 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);
            
            array.InsertAt(3, 102, heap);
            AreEqual("{ 99, 100, 101, 102 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);
            
            array.InsertAt(0, 98, heap);
            AreEqual("{ 98, 99, 100, 101, 102 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);
            
            array.RemoveAt(0, heap);
            AreEqual("{ 99, 100, 101, 102 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);
            
            array.InsertAt(4, 103, heap);
            AreEqual("{ 99, 100, 101, 102, 103 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);
            
            AreEqual("arraySize: 2 count: 0", heap.GetPool(1).ToString());
            AreEqual("arraySize: 4 count: 0", heap.GetPool(2).ToString());
            AreEqual("arraySize: 8 count: 1", heap.GetPool(3).ToString());
        }
        
        [Test]
        public void Test_IdArray_SetArray()
        {
            var store   = new EntityStore();
            var heap    = new IdArrayHeap();
            var array   = new IdArray();

            array.SetArray(new int[] { 100 }, heap);
            AreEqual("{ 100 }", array.GetSpan(heap, store).Debug());
            AreEqual(0, heap.Count);
            
            array.SetArray(new int[] { 101, 102 }, heap);
            AreEqual("{ 101, 102 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);
            
            array.SetArray(new int[] { 103, 104, 105 }, heap);
            AreEqual("{ 103, 104, 105 }", array.GetSpan(heap, store).Debug());
            AreEqual(1, heap.Count);
            
            array.SetArray(new int[] { }, heap);
            AreEqual("{ }", array.GetSpan(heap, store).Debug());
            AreEqual(0, heap.Count);
        }
        
        [Test]
        public void Test_IdArray_Remove()
        {
            var store   = new EntityStore();
            var heap    = new IdArrayHeap();
            {
                var array   = new IdArray();
                array.Add(100, heap);
                array.RemoveAt(0, heap);
                AreEqual(0, array.Count);
            } {
                var array   = new IdArray();
                array.Add(200, heap);
                array.Add(201, heap);
                array.RemoveAt(0, heap);
                AreEqual(1, array.Count);
                var ids     = array.GetSpan(heap, store);
                AreEqual("{ 201 }", ids.Debug());
                AreEqual(0, heap.Count);
            } {
                var array   = new IdArray();
                array.Add(300, heap);
                array.Add(301, heap);
                array.RemoveAt(1, heap);
                AreEqual(1, array.Count);
                var ids     = array.GetSpan(heap, store);
                AreEqual("{ 300 }", ids.Debug());
                AreEqual(0, heap.Count);
            } {
                var array   = new IdArray();
                array.Add(400, heap);
                array.Add(401, heap);
                array.Add(402, heap);
                array.RemoveAt(0, heap);
                AreEqual(2, array.Count);
                var ids     = array.GetSpan(heap, store);
                AreEqual("{ 401, 402 }", ids.Debug());
                AreEqual(1, heap.Count);
            } {
                var array   = new IdArray();
                array.Add(500, heap);
                array.Add(501, heap);
                array.Add(502, heap);
                array.RemoveAt(2, heap);
                AreEqual(2, array.Count);
                var ids     = array.GetSpan(heap, store);
                AreEqual("{ 500, 501 }", ids.Debug());
                AreEqual(2, heap.Count);
            } {
                var array   = new IdArray();
                array.Add(600, heap);
                array.Add(601, heap);
                array.Add(602, heap);
                array.Add(603, heap);
                array.RemoveAt(0, heap);
                AreEqual(3, array.Count);
                var ids     = array.GetSpan(heap, store);
                AreEqual("{ 603, 601, 602 }", ids.Debug());
                AreEqual(3, heap.Count);
            } {
                var array   = new IdArray();
                array.Add(700, heap);
                array.Add(701, heap);
                array.Add(702, heap);
                array.Add(703, heap);
                array.RemoveAt(3, heap);
                AreEqual(3, array.Count);
                var ids     = array.GetSpan(heap, store);
                AreEqual("{ 700, 701, 702 }", ids.Debug());
                AreEqual(4, heap.Count);
            }
        }
        
        [Test]
        public void Test_IdArray_clear_freeList()
        {
            var store   = new EntityStore();
            var heap    = new IdArrayHeap();
            var arrays  = new IdArray[100];
            int id      = 0;
            for (int n = 0; n < 100; n++) {
                ref var array = ref arrays[n]; 
                for (int i = 0; i < 10; i++) {
                    array.Add(id++, heap);
                }
            }
            id = 0;
            for (int n = 0; n < 100; n++) {
                var span = arrays[n].GetSpan(heap, store);
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
            Throws<IndexOutOfRangeException>(() => {
                array.RemoveAt(-1, heap);    
            });
            Throws<IndexOutOfRangeException>(() => {
                array.InsertAt(-1, 42, heap);    
            });
            Throws<IndexOutOfRangeException>(() => {
                array.InsertAt(1, 42, heap);    
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
                array.Add(42, heap);
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
                    array.Add(n, heap);
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
        

    }
}