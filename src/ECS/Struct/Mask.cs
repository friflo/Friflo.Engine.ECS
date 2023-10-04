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

    internal ArchetypeMask(StructHeap[] heaps, StructHeap newComp) {
        Vector256Long vec256 = default;
        if (newComp != null) {
            SetBit(newComp.structIndex, ref vec256);
        }
        foreach (var heap in heaps) {
            SetBit(heap.structIndex, ref vec256);
        }
        masks = new [] { vec256 };
    }
    
    public ArchetypeMask(int[] indices) {
        Vector256Long vec256 = default;
        foreach (var index in indices) {
            SetBit(index, ref vec256);
        }
        masks = new [] { vec256 };
    }
    
    public ArchetypeMask(ReadOnlySpan<int> indices) {
        Vector256Long vec256 = default;
        foreach (var index in indices) {
            SetBit(index, ref vec256);
        }
        masks = new [] { vec256 };
    }

    
    private static void SetBit(int index, ref Vector256Long vec256)
    {
        switch (index) {
            case < 64:      vec256.l0 |= 1L <<  index;          return;
            case < 128:     vec256.l1 |= 1L << (index - 64);    return;
            case < 192:     vec256.l2 |= 1L << (index - 128);   return;
            default:        vec256.l3 |= 1L << (index - 192);   return;
        }
    }
    
    internal bool Has(in ArchetypeMask other) {
        return (masks[0].value & other.masks[0].value) == masks[0].value;
    }
    
    private string GetString() {
        var sb = new StringBuilder();
        foreach (var mask in masks) {
            sb.Append(mask.value.ToString());
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
}