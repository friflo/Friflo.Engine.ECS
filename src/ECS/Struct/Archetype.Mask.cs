// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public readonly struct ArchetypeMask
{
    private readonly    BitSet  mask;
    
    public  override    string  ToString() => GetString();
    
    internal ArchetypeMask(StructHeap[] heaps) {
        foreach (var heap in heaps) {
            mask.SetBit(heap.structIndex);
        }
    }
    
    internal ArchetypeMask(Signature signature) {
        foreach (var type in signature.types) {
            mask.SetBit(type.structIndex);
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
