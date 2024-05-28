using Friflo.Engine.ECS.Utils;
using NUnit.Framework;

// ReSharper disable IntVariableOverflowInUncheckedContext
// ReSharper disable InconsistentNaming
namespace Internal.ECS {

public static class Test_BitSet
{
    [Test]
    public static void Test_BitSet_shims()
    {
        ulong allBits = 0xffffffff_ffffffff;
        Assert.AreEqual(64, BitSet.NumberOfSetBits((long)allBits));
        
        Assert.AreEqual(0,  BitSet.NumberOfSetBits(0));
        
        Assert.AreEqual(32, BitSet.NumberOfSetBits(0x0f0f0f0f_0f0f0f0f));
        
        
        Assert.AreEqual(64,  BitSet.TrailingZeroCount(0));
        Assert.AreEqual( 0,  BitSet.TrailingZeroCount((long)allBits));
        Assert.AreEqual( 0,  BitSet.TrailingZeroCount(1));
        
        Assert.AreEqual(31,  BitSet.TrailingZeroCount(1  << 31));
        Assert.AreEqual(32,  BitSet.TrailingZeroCount(1L << 32));
        Assert.AreEqual(63,  BitSet.TrailingZeroCount(1L << 63));
        
        for (int n = 1; n < 64; n++) {
            long value = 1L << n;
            Assert.AreEqual(n,  BitSet.TrailingZeroCount(value));
        }
    }
}

}