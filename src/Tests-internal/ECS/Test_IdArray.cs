using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Internal.ECS {

    public class Test_IdArray
    {
        [Test]
        public void Test_IdArray_basics()
        {
            var idArrays    = new IdArrays();
            var array       = new IdArray(); 
            array           = idArrays.Add(array, 100);
            AreEqual(1, array.Count);
        //  AreEqual(new int[] { 100 }, idArrays.Ids(array).ToArray()); todo
            
            array           = idArrays.Add(array, 101);
            AreEqual(2, array.Count);
            AreEqual(new int[] { 100, 101 }, idArrays.Ids(array).ToArray());

            array           = idArrays.Add(array, 102);
            AreEqual(3, array.Count);
            AreEqual(new int[] { 100, 101, 102 }, idArrays.Ids(array).ToArray());
            
            array           = idArrays.Add(array, 103);
            AreEqual(4, array.Count);
            AreEqual(new int[] { 100, 101, 102, 103 }, idArrays.Ids(array).ToArray());
            
            array           = idArrays.Add(array, 104);
            AreEqual(5, array.Count);
            AreEqual(new int[] { 100, 101, 102, 103, 104 }, idArrays.Ids(array).ToArray());
        }
    }
}