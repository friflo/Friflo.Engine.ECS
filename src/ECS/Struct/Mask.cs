using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public readonly struct ArchetypeMask
{
    private readonly   Vector256Long[]   masks;

    public override string ToString() => GetString();
    
    public ArchetypeMask(int i) {
        masks = new Vector256Long[] { default };
    }

    internal ArchetypeMask(StructHeap[] heaps, StructHeap newComp) {
        Vector256Long vec256 = default;
        if (newComp != null) {
            vec256.SetBit(newComp.structIndex);
        }
        foreach (var heap in heaps) {
            vec256.SetBit(heap.structIndex);
        }
        masks = new [] { vec256 };
    }
    
    public ArchetypeMask(int[] indices) {
        Vector256Long vec256 = default;
        foreach (var index in indices) {
            vec256.SetBit(index);
        }
        masks = new [] { vec256 };
    }
    
    public ArchetypeMask(ReadOnlySpan<int> indices) {
        Vector256Long vec256 = default;
        foreach (var index in indices) {
            vec256.SetBit(index);
        }
        masks = new [] { vec256 };
    }
    
    public void SetBit(int bit) {
        masks[0].SetBit(bit);
    }

    internal bool Has(in ArchetypeMask other) {
        return (masks[0].value & other.masks[0].value) == masks[0].value;
    }
    
    private string GetString() {
        var sb = new StringBuilder();
        foreach (var mask in masks) {
            if (mask.l3 != 0) {
                sb.Append($"{mask.l0:x16} {mask.l1:x16} {mask.l2:x16} {mask.l3:x16}");
            } else if (mask.l2 != 0) {
                sb.Append($"{mask.l0:x16} {mask.l1:x16} {mask.l2:x16}");
            } else if (mask.l1 != 0) {
                sb.Append($"{mask.l0:x16} {mask.l1:x16}");
            } else {
                sb.Append($"{mask.l0:x16}");
            }
        }
        return sb.ToString();
    }
}

[StructLayout(LayoutKind.Explicit)]
internal struct Vector256Long
{
    [FieldOffset(00)] internal  Vector256<long> value;
    
    [FieldOffset(00)] internal  long            l0;
    [FieldOffset(08)] internal  long            l1;
    [FieldOffset(16)] internal  long            l2;
    [FieldOffset(24)] internal  long            l3;

    public override             string          ToString() => value.ToString();
    
    internal void SetBit(int index)
    {
        switch (index) {
            case < 64:      l0 |= 1L <<  index;          return;
            case < 128:     l1 |= 1L << (index - 64);    return;
            case < 192:     l2 |= 1L << (index - 128);   return;
            default:        l3 |= 1L << (index - 192);   return;
        }
    }
}