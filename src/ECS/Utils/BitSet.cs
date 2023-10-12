// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// Support a bit set currently limited to 256 bits.<br/>
/// If need an additional Vector256Long[] could be added be added for arbitrary length.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct BitSet
{
    [FieldOffset(00)] internal  Vector256<long> value;      // 32
    
    [FieldOffset(00)] internal  long            l0;         // (8)
    [FieldOffset(08)] internal  long            l1;         // (8)
    [FieldOffset(16)] internal  long            l2;         // (8)
    [FieldOffset(24)] internal  long            l3;         // (8)
    
    // Could extend with Vector256Long[] if 256 struct components are not enough
    // private readonly  Vector256Long[]   values;

    public              BitSetEnumerator    GetEnumerator() => new BitSetEnumerator(this);
    public override     string              ToString()      => AppendString(new StringBuilder()).ToString();
    
    public BitSet(int[] indices) {
        foreach (var index in indices) {
            SetBit(index);
        }
    }
    
    // hash distribution is probably not good. But executes fast. Leave it for now.
    public override int GetHashCode()
    {
        return unchecked((int)l0) ^ (int)(l0 >> 32) ^
               unchecked((int)l1) ^ (int)(l1 >> 32) ^
               unchecked((int)l2) ^ (int)(l2 >> 32) ^
               unchecked((int)l3) ^ (int)(l3 >> 32);
    }
        
    public void SetBit(int index)
    {
        switch (index) {
            case < 64:      l0 |= 1L <<  index;          return;
            case < 128:     l1 |= 1L << (index - 64);    return;
            case < 192:     l2 |= 1L << (index - 128);   return;
            default:        l3 |= 1L << (index - 192);   return;
        }
    }
    
    public void ClearBit(int index)
    {
        switch (index) {
            case < 64:      l0 &= ~(1L <<  index);          return;
            case < 128:     l1 &= ~(1L << (index - 64));    return;
            case < 192:     l2 &= ~(1L << (index - 128));   return;
            default:        l3 &= ~(1L << (index - 192));   return;
        }
    }
    
    public bool Has(int index)
    {
        switch (index) {
            case < 64:      return (l0 & (1L <<  index))        != 0;
            case < 128:     return (l1 & (1L << (index -  64))) != 0;
            case < 192:     return (l2 & (1L << (index - 128))) != 0;
            default:        return (l3 & (1L << (index - 192))) != 0;
        }
    }
    
    public bool HasAll(in BitSet bitSet)
    {
        return (value & bitSet.value) == bitSet.value;
    }
    
    public bool HasAny(in BitSet bitSet)
    {
        return (value & bitSet.value) != default;
    }
    
    public long LongAt(int index)
    {
        switch (index) {
            case 0:     return l0;
            case 1:     return l1;
            case 2:     return l2;
            case 3:     return l3;
            default:
                throw new IndexOutOfRangeException();
        }
    }
    
    private StringBuilder AppendString(StringBuilder sb) {
        if (l3 != 0) {
            sb.Append($"{l0:x16} {l1:x16} {l2:x16} {l3:x16}");
        } else if (l2 != 0) {
            sb.Append($"{l0:x16} {l1:x16} {l2:x16}");
        } else if (l1 != 0) {
            sb.Append($"{l0:x16} {l1:x16}");
        } else {
            sb.Append($"{l0:x16}");
        }
        return sb;
    }
}

public struct BitSetEnumerator
{
    private readonly    BitSet      bitSet;
    private             int         curPos; // range: [0, ..., 255]
    private             int         lngPos; // range: [0, 1, 2, 3, 4]
    private             long        lng;    // 64 bits
    
    internal BitSetEnumerator(in BitSet bitSet) {
        this.bitSet = bitSet;
        lng         = bitSet.l0;
    }
    
    /// <returns>the index of the current bit == 1. The index range is [0, ... , 255]</returns>
    public int Current => curPos;
    
    // --- IEnumerator
    public void Reset() {
        lng     = bitSet.l0;
        curPos  = 0;
        lngPos  = 0;
    }
    
    public bool MoveNext()
    {
        if (lngPos < 4)
        {
            while (true)
            {
                if (lng != 0) {
                    var bitPos  = BitOperations.TrailingZeroCount(lng);
                    lng        ^= 1L << bitPos;
                    curPos      = (lngPos << 6) + bitPos;
                    return true;
                }
                lngPos++;
                if (lngPos == 4) {
                    return false;
                }
                lng = bitSet.LongAt(lngPos);
            }
        }
        return false;  
    }
}
