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
            AreEqual(0, array.Count);
            AreEqual(new int[] { }, array.GetIdSpan(heap).ToArray());

            array.AddId(100, heap);
            AreEqual(1, array.Count);
            var span = array.GetIdSpan(heap);
            AreEqual(new int[] { 100 }, span.ToArray());
            
            array.AddId(101, heap);
            AreEqual(2, array.Count);
            AreEqual(new int[] { 100, 101 }, array.GetIdSpan(heap).ToArray());

            array.AddId(102, heap);
            AreEqual(3, array.Count);
            AreEqual(new int[] { 100, 101, 102 }, array.GetIdSpan(heap).ToArray());
            
            array.AddId(103, heap);
            AreEqual(4, array.Count);
            AreEqual(new int[] { 100, 101, 102, 103 }, array.GetIdSpan(heap).ToArray());
            
            array.AddId(104, heap);
            AreEqual(5, array.Count);
            AreEqual(new int[] { 100, 101, 102, 103, 104 }, array.GetIdSpan(heap).ToArray());
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
                array.AddId(100, heap);
                array.AddId(101, heap);
                array.RemoveAt(0, heap);
                AreEqual(1, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual(new int[] { 101 }, ids.ToArray());
            } {
                var array   = new IdArray();
                array.AddId(100, heap);
                array.AddId(101, heap);
                array.RemoveAt(1, heap);
                AreEqual(1, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual(new int[] { 100 }, ids.ToArray());
            }
        }
    }
}