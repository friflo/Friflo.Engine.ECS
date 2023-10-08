// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public struct ArchetypeMask
{
    private Vector256Long   mask;
    
    // Could extend with Vector256Long[] if 256 struct components are not enough
    // private readonly   Vector256Long[]   masks;

    public override string ToString() => GetString();
    
    public ArchetypeMask(int _) {
        // masks = new Vector256Long[] { default };
    }

    internal ArchetypeMask(StructHeap[] heaps, StructHeap newComp) {
        if (newComp != null) {
            mask.SetBit(newComp.structIndex);
        }
        foreach (var heap in heaps) {
            mask.SetBit(heap.structIndex);
        }
    }
    
    public ArchetypeMask(int[] indices) {
        foreach (var index in indices) {
            mask.SetBit(index);
        }
    }
    
    public ArchetypeMask(Signature signature) {
        foreach (var type in signature.types) {
            mask.SetBit(type.index);
        }
    }
    
    public void SetBit(int bit) {
        mask.SetBit(bit);
    }

    internal bool Has(in ArchetypeMask other) {
        return (mask.value & other.mask.value) == mask.value;
    }
    
    private string GetString() {
        return mask.AppendString(new StringBuilder()).ToString();
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

    public override             string          ToString() => AppendString(new StringBuilder()).ToString();
    
    internal void SetBit(int index)
    {
        switch (index) {
            case < 64:      l0 |= 1L <<  index;          return;
            case < 128:     l1 |= 1L << (index - 64);    return;
            case < 192:     l2 |= 1L << (index - 128);   return;
            default:        l3 |= 1L << (index - 192);   return;
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