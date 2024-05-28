using Friflo.Engine.ECS.Utils;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming

namespace Tests.ECS.Base {

public static class Test_BitSet
{
    [Test]
    public static void Test_BitSet_SetBit()
    {
        {
            var bitSet = new BitSet();
            bitSet.SetBit(1);
            AreEqual(1, bitSet.GetBitCount());
            bitSet.SetBit(2);
            AreEqual(2, bitSet.GetBitCount());
            AreEqual("0000000000000006",                                                    bitSet.ToString());
            IsTrue(bitSet.Has(1));
            IsTrue(bitSet.Has(2));
            
            bitSet.SetBit(255);
            AreEqual("0000000000000006 0000000000000000 0000000000000000 8000000000000000", bitSet.ToString());
            AreEqual(3, bitSet.GetBitCount());
            IsTrue(bitSet.Has(255));
        } {
            var bitSet = new BitSet(new [] { 0 });
            AreEqual(1, bitSet.GetBitCount());
            AreEqual("0000000000000001",                                                    bitSet.ToString());
            IsTrue(bitSet.Has(0));
        } {
            var bitSet = new BitSet(new [] { 64 });
            AreEqual(1, bitSet.GetBitCount());
            AreEqual("0000000000000000 0000000000000001",                                   bitSet.ToString());
            IsTrue(bitSet.Has(64));
        } {
            var bitSet = new BitSet(new [] { 128 });
            AreEqual(1, bitSet.GetBitCount());
            AreEqual("0000000000000000 0000000000000000 0000000000000001",                  bitSet.ToString());
            IsTrue(bitSet.Has(128));
        } {
            var bitSet = new BitSet(new [] { 0, 64, 128, 192 });
            AreEqual(4, bitSet.GetBitCount());
            AreEqual("0000000000000001 0000000000000001 0000000000000001 0000000000000001", bitSet.ToString());
            IsTrue(bitSet.Has(64));
            IsTrue(bitSet.Has(128));
            IsTrue(bitSet.Has(192));
        } {
            var bitSet = new BitSet(new [] { 63, 127, 191, 255 });
            AreEqual(4, bitSet.GetBitCount());
            AreEqual("8000000000000000 8000000000000000 8000000000000000 8000000000000000", bitSet.ToString());
        }
    }
    
    [Test]
    public static void Test_BitSet_Enumerator() {
        var bitSet      = new BitSet();
        var enumerator  = bitSet.GetEnumerator();
        var count = 0;
        while (enumerator.MoveNext()) {
            count++;
        }
        AreEqual(0, count);
        
        enumerator.Reset();
        count = 0;
        while (enumerator.MoveNext()) {
            count++;
        }
        AreEqual(0, count);
    }
    
    [Test]
    public static void Test_BitSet_SetAllBits()
    {
        var bitSet = new BitSet();
        for (int n = 0; n < 256; n++) {
            if (bitSet.Has(n)) {
                Fail($"Expect bit == false: index: {n}");
            }
            bitSet.SetBit(n);
            AreEqual(n + 1, bitSet.GetBitCount());
            if (!bitSet.Has(n)) {
                Fail($"Expect bit == true: index: {n}");
            }
        }
        AreEqual("ffffffffffffffff ffffffffffffffff ffffffffffffffff ffffffffffffffff", bitSet.ToString());
        var allBits = bitSet;
        IsTrue(allBits.HasAll(allBits));
        IsTrue(allBits.HasAny(allBits));
        
        for (int n = 0; n < 256; n++) {
            var singleBit = new BitSet();
            singleBit.SetBit(n);
            IsTrue(allBits.HasAll(singleBit));
            IsTrue(allBits.HasAny(singleBit));
            
            IsFalse(singleBit.HasAll(allBits));
            IsTrue (singleBit.HasAny(allBits));
        }
    }
    
    [Test]
    public static void Test_BitSet_ClearBit()
    {
        var allBits = new BitSet();
        for (int n = 0; n < 256; n++) {
            allBits.SetBit(n);
        }
        AreEqual("ffffffffffffffff ffffffffffffffff ffffffffffffffff ffffffffffffffff", allBits.ToString());
        
        var bits = allBits;
        for (int n = 0; n < 256; n++) {
            IsTrue(bits.Has(n));
            bits.ClearBit(n);
            IsFalse(bits.Has(n));
        }
    }
    
    [Test]
    public static void Test_BitSet_ForEach()
    {
        {
            var start = Mem.GetAllocatedBytes();
            int count = 0;
            var bitSet = new BitSet();
            foreach (var _ in bitSet) {
                count++;
            }
            Mem.AssertNoAlloc(start);
            AreEqual(0, count);
        } {
            var start = Mem.GetAllocatedBytes();
            int count = 0;
            var bitSet = new BitSet();
            bitSet.SetBit(1);
            foreach (var _ in bitSet) {
                count++;
            }
            Mem.AssertNoAlloc(start);
            AreEqual(1, count);
        } {
            var start = Mem.GetAllocatedBytes();
            var bitSet = new BitSet();
            for (int n = 0; n < 256; n++) {
                bitSet.SetBit(n);
            }
            int count = 0;
            foreach (var index in bitSet) {
                if (count != index) {
                    Fail($"Expect: {count}, was: {index}");
                }
                count++;
            }
            Mem.AssertNoAlloc(start);
            AreEqual(256, count);
        }
    }
    
    [Test]
    public static void Test_BitSet_HashCode_Perf()
    {
        var bitSet = new BitSet(new [] { 1, 10, 20, 30, 60, 64, 100, 110, 120, 170, 180, 200, 240, 250});
        var count = 10; // 10_000_000_000 ~ #PC: 2.355 ms
        for (long n = 0; n < count; n++)
        {
            _ = bitSet.HashCode();
        }

    }
}

}