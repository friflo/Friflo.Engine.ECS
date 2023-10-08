using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.ECS;

public static class Test_BitSet
{
    [Test]
    public static void Test_BitSet_SetBit()
    {
        var bitSet = new BitSet256();
        bitSet.SetBit(1);
        bitSet.SetBit(2);
        AreEqual("0000000000000006", bitSet.ToString());
        
        bitSet.SetBit(255);
        AreEqual("0000000000000006 0000000000000000 0000000000000000 8000000000000000", bitSet.ToString());
    }
    
    [Test]
    public static void Test_BitSet_ForEach()
    {
        {
            var start = Mem.GetAllocatedBytes();
            int count = 0;
            var bitSet = new BitSet256();
            foreach (var _ in bitSet) {
                count++;
            }
            Mem.AssertNoAlloc(start);
            AreEqual(0, count);
        } {
            var start = Mem.GetAllocatedBytes();
            int count = 0;
            var bitSet = new BitSet256();
            bitSet.SetBit(1);
            foreach (var _ in bitSet) {
                count++;
            }
            Mem.AssertNoAlloc(start);
            AreEqual(1, count);
        } {
            var start = Mem.GetAllocatedBytes();
            var bitSet = new BitSet256();
            for (int n = 0; n < 256; n++) {
                bitSet.SetBit(n);
            }
            int count = 0;
            foreach (var index in bitSet) {
                if (count != index) {
                    Fail($"Expect: {count}, was: {index}");
                }
                count++;
            };
            Mem.AssertNoAlloc(start);
            AreEqual(256, count);
        }
    }
}