using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Internal.ECS {

    public class Test_IdArray
    {
        [Test]
        public void Test_IdArray_Add()
        {
            var heap    = new IdArrayHeap();
            
            var array   = new IdArray();
            AreEqual(0, array.Count);
            AreEqual(new int[] { }, heap.IdSpan(array).ToArray());

            array = heap.Add(array, 100);
            AreEqual(1, array.Count);
            var span = heap.IdSpan(array);
            AreEqual(new int[] { 100 }, span.ToArray());
            
            array = heap.Add(array, 101);
            AreEqual(2, array.Count);
            AreEqual(new int[] { 100, 101 }, heap.IdSpan(array).ToArray());

            array = heap.Add(array, 102);
            AreEqual(3, array.Count);
            AreEqual(new int[] { 100, 101, 102 }, heap.IdSpan(array).ToArray());
            
            array = heap.Add(array, 103);
            AreEqual(4, array.Count);
            AreEqual(new int[] { 100, 101, 102, 103 }, heap.IdSpan(array).ToArray());
            
            array = heap.Add(array, 104);
            AreEqual(5, array.Count);
            AreEqual(new int[] { 100, 101, 102, 103, 104 }, heap.IdSpan(array).ToArray());
        }
    }
}