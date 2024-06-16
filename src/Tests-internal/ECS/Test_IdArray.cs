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
            var array1      = new IdArray(); 
            array1          = idArrays.Add(array1, 42);
            AreEqual(1, array1.Count);

        }
    }
}