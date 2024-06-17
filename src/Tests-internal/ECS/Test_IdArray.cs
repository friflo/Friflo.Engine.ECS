using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
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
            AreEqual("count: 2", array.ToString());
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
        public void Test_IdArray_exceptions()
        {
            var heap    = new IdArrayHeap();
            var array   = new IdArray();
            Throws<IndexOutOfRangeException>(() => {
                array.RemoveAt(0, heap);    
            });
        }
        
        [Test]
        public void Test_IdArray_Perf()
        {
            int count   = 100; // 1_000_000
            int repeat  = 50;
            //  #PC: IdArray Perf: count: 1000000 repeat: 50 duration: 3715 ms
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
            Console.WriteLine($"IdArray Perf: count: {count} repeat: {repeat} duration: {sw.ElapsedMilliseconds} ms");
            AreEqual(0, heap.Count);
        }
        
        [Test]
        public unsafe void Test_IdArray_size_of()
        {
            AreEqual(8, sizeof(IdArray));
        }
    }
}