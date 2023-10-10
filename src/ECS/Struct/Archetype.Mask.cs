// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Text;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public struct ArchetypeMask : IEnumerable<ComponentType>
{
    internal    BitSet  bitSet;
    
   
    public ArchetypeMaskEnumerator  GetEnumerator()                             => new ArchetypeMaskEnumerator (this);
    // --- IEnumerable
    IEnumerator                     IEnumerable.GetEnumerator()                 => new ArchetypeMaskEnumerator (this);
    // --- IEnumerable<>
    IEnumerator<ComponentType>      IEnumerable<ComponentType>.GetEnumerator()  => new ArchetypeMaskEnumerator (this);

    public   override               string    ToString() => GetString();
    
    private ArchetypeMask(in BitSet bitSet) {
        this.bitSet = bitSet;
    }
    
    internal ArchetypeMask(StructHeap[] heaps) {
        foreach (var heap in heaps) {
            bitSet.SetBit(heap.structIndex);
        }
    }
    
    internal ArchetypeMask(Signature signature) {
        foreach (var type in signature.types) {
            bitSet.SetBit(type.structIndex);
        }
    }
    
    // ----------------------------------------- read mask -----------------------------------------
    internal bool Has(in ArchetypeMask other) {
        return (bitSet.value & other.bitSet.value) == bitSet.value;
    }
    
    public  bool    Has<T> ()
        where T : struct, IStructComponent
    {
        return bitSet.Has(StructHeap<T>.StructIndex);
    }
    
    public  bool    Has<T1, T2> ()
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
    {
        return bitSet.Has(StructHeap<T1>.StructIndex) &&
               bitSet.Has(StructHeap<T2>.StructIndex);
    }

    public  bool    Has<T1, T2, T3> ()
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
        where T3 : struct, IStructComponent
    {
        return bitSet.Has(StructHeap<T1>.StructIndex) &&
               bitSet.Has(StructHeap<T2>.StructIndex) &&
               bitSet.Has(StructHeap<T3>.StructIndex);
    }
    
    public  bool    HasAll (in ArchetypeMask mask)
    {
        return bitSet.HasAll(mask.bitSet);
    }
    
    public  bool    HasAny (in ArchetypeMask mask)
    {
        return bitSet.HasAny(mask.bitSet);
    }
    
    // ----------------------------------------- mutate Mask -----------------------------------------
    
    public void Add<T>()
        where T : struct, IStructComponent
    {
        bitSet.SetBit(StructHeap<T>.StructIndex);
    }
    
    public void Add(in ArchetypeMask mask)
    {
        bitSet.value |= mask.bitSet.value;
    }
    
    public void Remove<T>()
        where T : struct, IStructComponent
    {
        bitSet.ClearBit(StructHeap<T>.StructIndex);
    }
    
    public void Remove(in ArchetypeMask mask)
    {
        bitSet.value &= ~mask.bitSet.value;
    }
    
    // ----------------------------------------- static methods -----------------------------------------    
    public static ArchetypeMask Get<T>()
        where T : struct, IStructComponent
    {
        BitSet bitSet = default;
        bitSet.SetBit(StructHeap<T>.StructIndex);
        return new ArchetypeMask(bitSet);
    }
    
    public static ArchetypeMask Get<T1, T2>()
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
    {
        BitSet bitSet = default;
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        return new ArchetypeMask(bitSet);
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append("Mask: [");
        var hasTypes    = false;
        foreach (var index in bitSet) {
            var structType = EntityStore.Static.ComponentSchema.GetStructComponentAt(index);
            sb.Append('#');
            sb.Append(structType.type.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        if (hasTypes) {
            sb.Length -= 2;
        }
        sb.Append(']');
        return sb.ToString();
    }
}

public struct ArchetypeMaskEnumerator : IEnumerator<ComponentType>
{
    private BitSetEnumerator    bitSetEnumerator;

    // --- IEnumerator
    public  void                Reset()             => bitSetEnumerator.Reset();

            object              IEnumerator.Current => Current;

    public  ComponentType       Current => EntityStore.Static.ComponentSchema.GetStructComponentAt(bitSetEnumerator.Current);
    
    internal ArchetypeMaskEnumerator(in ArchetypeMask mask) {
        bitSetEnumerator = mask.bitSet.GetEnumerator();
    }
    
    // --- IEnumerator
    public bool MoveNext() => bitSetEnumerator.MoveNext();
    public void Dispose() { }
}