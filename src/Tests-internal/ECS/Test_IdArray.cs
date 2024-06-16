using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Internal.ECS
{
    public class Test_IdArray
    {
        // [Test]
        public void Test_IdArray_Add()
        {
            var heap    = new IdArrayHeap();
            
            var array   = new IdArray();
            AreEqual(0, array.Count);
            AreEqual(new int[] { }, heap.IdSpan(array).ToArray());

            array = heap.AddId(array, 100);
            AreEqual(1, array.Count);
            var span = heap.IdSpan(array);
            AreEqual(new int[] { 100 }, span.ToArray());
            
            array = heap.AddId(array, 101);
            AreEqual(2, array.Count);
            AreEqual(new int[] { 100, 101 }, heap.IdSpan(array).ToArray());

            array = heap.AddId(array, 102);
            AreEqual(3, array.Count);
            AreEqual(new int[] { 100, 101, 102 }, heap.IdSpan(array).ToArray());
            
            array = heap.AddId(array, 103);
            AreEqual(4, array.Count);
            AreEqual(new int[] { 100, 101, 102, 103 }, heap.IdSpan(array).ToArray());
            
            array = heap.AddId(array, 104);
            AreEqual(5, array.Count);
            AreEqual(new int[] { 100, 101, 102, 103, 104 }, heap.IdSpan(array).ToArray());
        }
        
        // [Test]
        public void Test_IdArray_Remove()
        {
            var heap    = new IdArrayHeap();
            {
                var array   = new IdArray();
                array       = heap.AddId(array, 100);
                array       = heap.RemoveAt(array, 0);
                AreEqual(0, array.Count);
            } {
                var array   = new IdArray();
                array       = heap.AddId(array, 100);
                array       = heap.AddId(array, 101);
                array       = heap.RemoveAt(array, 0);
                AreEqual(1, array.Count);
                AreEqual(new int[] { 101 }, heap.IdSpan(array).ToArray());
            }
        }
    }
}