// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;

[assembly: InternalsVisibleTo("Tests-internal")]
[assembly: InternalsVisibleTo("Fliox.Tests-internal")]

// ReSharper disable ConvertToAutoProperty
namespace Friflo.Engine.ECS.Utils;

/// <summary>
/// Support a bit set currently limited to 256 bits.<br/>
/// If need an additional Vector256Long[] could be added be added for arbitrary length.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct BitSet
{
#if NET5_0_OR_GREATER
    [FieldOffset(00)] internal  Vector256<long> value;      // 32
#endif
    [FieldOffset(00)] internal  long            l0;         // (8)
    [FieldOffset(08)] internal  long            l1;         // (8)
    [FieldOffset(16)] internal  long            l2;         // (8)
    [FieldOffset(24)] internal  long            l3;         // (8)
    
    // Could extend with Vector256Long[] if 256 components are not enough
    // private readonly  Vector256Long[]   values;


    public readonly          BitSetEnumerator    GetEnumerator() => new BitSetEnumerator(this);
    public readonly override string              ToString()      => AppendString(new StringBuilder()).ToString();
    
    public BitSet(int[] indices) {
        foreach (var index in indices) {
            SetBit(index);
        }
    }
    
#if NET5_0_OR_GREATER
    internal bool IsDefault()                   => value.Equals(default);
    internal bool Equals   (in BitSet other)    => value.Equals(other.value);
#else
    internal bool IsDefault() {
        return l0 == 0 &&
               l1 == 0 &&
               l2 == 0 &&
               l3 == 0;
    }
    
    internal bool Equals (in BitSet other) {
        return l0 == other.l0 &&
               l1 == other.l1 &&
               l2 == other.l2 &&
               l3 == other.l3;
    }
#endif
    
    // hash distribution is probably not good. But executes fast. Leave it for now.
    public readonly int HashCode()
    {
        return unchecked((int)l0) ^ (int)(l0 >> 32) ^
               unchecked((int)l1) ^ (int)(l1 >> 32) ^
               unchecked((int)l2) ^ (int)(l2 >> 32) ^
               unchecked((int)l3) ^ (int)(l3 >> 32);
    }
    
    public readonly int GetBitCount() {
        return
            BitOperations.PopCount((ulong)l0) +
            BitOperations.PopCount((ulong)l1) +
            BitOperations.PopCount((ulong)l2) + 
            BitOperations.PopCount((ulong)l3);
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
    
    public readonly bool Has(int index)
    {
        switch (index) {
            case < 64:      return (l0 & (1L <<  index))        != 0;
            case < 128:     return (l1 & (1L << (index -  64))) != 0;
            case < 192:     return (l2 & (1L << (index - 128))) != 0;
            default:        return (l3 & (1L << (index - 192))) != 0;
        }
    }
    
    public readonly bool HasAll(in BitSet bitSet)
    {
#if NET7_0_OR_GREATER
        return (value & bitSet.value) == bitSet.value;
#else
        return (l0 & bitSet.l0) == bitSet.l0 &&
               (l1 & bitSet.l1) == bitSet.l1 &&
               (l2 & bitSet.l2) == bitSet.l2 &&
               (l3 & bitSet.l3) == bitSet.l3;
#endif
    }
    
    public readonly bool HasAny(in BitSet bitSet)
    {
#if NET7_0_OR_GREATER
        return !(value & bitSet.value).Equals(default);
#else
        return (l0 & bitSet.l0) != 0 ||
               (l1 & bitSet.l1) != 0 ||
               (l2 & bitSet.l2) != 0 ||
               (l3 & bitSet.l3) != 0;
#endif
    }
    
#if NET7_0_OR_GREATER
    internal static BitSet Add (in BitSet left, in BitSet right) {
        return new BitSet {
            value = left.value | right.value
        };
    }
    
    internal static BitSet Remove (in BitSet left, in BitSet right) {
        return new BitSet {
            value = left.value & ~right.value
         };
    }
    
    internal static BitSet Added (in BitSet left, in BitSet right) {
        return new BitSet {
            value = ~left.value & right.value
        };
    }
    
    internal static BitSet Removed (in BitSet left, in BitSet right) {
        return new BitSet {
            value = left.value & ~right.value
        };
    }
    
    internal static BitSet Changed (in BitSet left, in BitSet right) {
        return new BitSet {
            value = left.value ^ right.value
        };
    }
#else
    internal static BitSet Add (in BitSet left, in BitSet right) {
        return new BitSet {
            l0 = left.l0 | right.l0,
            l1 = left.l1 | right.l1,
            l2 = left.l2 | right.l2,
            l3 = left.l3 | right.l3,
        };
    }
    
    internal static BitSet Remove (in BitSet left, in BitSet right) {
        return new BitSet {
            l0 = left.l0 & ~right.l0,
            l1 = left.l1 & ~right.l1,
            l2 = left.l2 & ~right.l2,
            l3 = left.l3 & ~right.l3,
        };
    }
    
    internal static BitSet Added (in BitSet left, in BitSet right) {
        return new BitSet {
            l0 = ~left.l0 & right.l0,
            l1 = ~left.l1 & right.l1,
            l2 = ~left.l2 & right.l2,
            l3 = ~left.l3 & right.l3,
        };
    }
    
    internal static BitSet Removed (in BitSet left, in BitSet right) {
        return new BitSet {
            l0 = left.l0 & ~right.l0,
            l1 = left.l1 & ~right.l1,
            l2 = left.l2 & ~right.l2,
            l3 = left.l3 & ~right.l3,
        };
    }
    
    internal static BitSet Changed (in BitSet left, in BitSet right) {
        return new BitSet {
            l0 = left.l0 ^ right.l0,
            l1 = left.l1 ^ right.l1,
            l2 = left.l2 ^ right.l2,
            l3 = left.l3 ^ right.l3,
        };
    }
#endif
    
    private readonly StringBuilder AppendString(StringBuilder sb) {
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
    private readonly    long    l0;     //  8
    private readonly    long    l1;     //  8
    private readonly    long    l2;     //  8
    private readonly    long    l3;     //  8
    
    private             long    lng;    //  8   - 64 bits
    private             int     lngPos; //  4   - range: [0, 1, 2, 3, 4] - higher values are not assigned
    private             int     curPos; //  4   - range: [0, ..., 255]
    
    internal BitSetEnumerator(in BitSet bitSet) {
        l0  = bitSet.l0;
        l1  = bitSet.l1;
        l2  = bitSet.l2;
        l3  = bitSet.l3;
        lng = l0;
    }
    
    /// <returns>the index of the current bit == 1. The index range is [0, ... , 255]</returns>
    public readonly int Current => curPos;
    
    // --- IEnumerator
    public void Reset() {
        lng     = l0;
        lngPos  = 0;
        curPos  = 0;
    }
    
    public bool MoveNext()
    {
        while (true)
        {
            if (lng != 0) {
                var bitPos  = BitOperations.TrailingZeroCount(lng);
                lng        ^= 1L << bitPos;
                curPos      = (lngPos << 6) + bitPos;
                return true;
            }
            switch (++lngPos) {
            //  case 0      not possible
                case 1:     lng = l1;   break;  // use break instead of continue to reach scope end for test coverage
                case 2:     lng = l2;   break;
                case 3:     lng = l3;   break;
                case 4:     return false;
            }
        }
    }
}
