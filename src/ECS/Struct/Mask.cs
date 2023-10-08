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

public readonly struct ArchetypeMask
{
    private readonly    BitSet  mask;
    
    public  override    string  ToString() => GetString();
    
    internal ArchetypeMask(StructHeap[] heaps, StructHeap newComp) {
        if (newComp != null) {
            mask.SetBit(newComp.structIndex);
        }
        foreach (var heap in heaps) {
            mask.SetBit(heap.structIndex);
        }
    }
    
    internal ArchetypeMask(Signature signature) {
        foreach (var type in signature.types) {
            mask.SetBit(type.index);
        }
    }

    internal bool Has(in ArchetypeMask other) {
        return (mask.value & other.mask.value) == mask.value;
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append("Mask: [");
        foreach (var index in mask) {
            var tagType = EntityStore.Static.ComponentSchema.GetStructComponentAt(index);
            sb.Append('#');
            sb.Append(tagType.type.Name);
            sb.Append(", ");
        }
        sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }
}

/// <summary>
/// Support a bit set currently limited to 256 bits.<br/>
/// If need an additional Vector256Long[] could be added be added for arbitrary length.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct BitSet
{
    [FieldOffset(00)] internal  Vector256<long> value;
    
    [FieldOffset(00)] internal  long            l0;
    [FieldOffset(08)] internal  long            l1;
    [FieldOffset(16)] internal  long            l2;
    [FieldOffset(24)] internal  long            l3;
    
    // Could extend with Vector256Long[] if 256 struct components are not enough
    // private readonly  Vector256Long[]   values;

    public                  BitSet256Enumerator GetEnumerator() => new BitSet256Enumerator(this); 
    
    public override             string          ToString() => AppendString(new StringBuilder()).ToString();
    
    public BitSet(int[] indices) {
        SetBits(indices);
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
    
    public void SetBits(int[] indices) {
        foreach (var index in indices) {
            SetBit(index);
        }
    }
    
    internal long At(int index)
    {
        switch (index) {
            case 0:     return l0;
            case 1:     return l1;
            case 2:     return l2;
            case 3:     return l3;
            default:
                throw new IndexOutOfRangeException($"index: {index}");
        }
    }
    
    internal StringBuilder AppendString(StringBuilder sb) {
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

public struct BitSet256Enumerator
{
    private readonly    BitSet      bitSet;
    private             int         curPos;
    private             int         lngPos;
    private             long        lng;
    
    internal BitSet256Enumerator(in BitSet bitSet) {
        this.bitSet = bitSet;
        lng         = bitSet.l0;
    }
    
    public int Current => curPos;
    
    // --- IEnumerator
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
                lng = bitSet.At(lngPos);
            }
        }
        return false;  
    }
}
