// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Text;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public struct ArchetypeStructs : IEnumerable<ComponentType>
{
    internal    BitSet  bitSet;     // 32
    
   
    public ArchetypeMaskEnumerator  GetEnumerator()                             => new ArchetypeMaskEnumerator (this);
    // --- IEnumerable
    IEnumerator                     IEnumerable.GetEnumerator()                 => new ArchetypeMaskEnumerator (this);
    // --- IEnumerable<>
    IEnumerator<ComponentType>      IEnumerable<ComponentType>.GetEnumerator()  => new ArchetypeMaskEnumerator (this);

    public   override               string    ToString() => GetString();
    
    internal ArchetypeStructs(StructHeap[] heaps) {
        foreach (var heap in heaps) {
            bitSet.SetBit(heap.structIndex);
        }
    }
    
    // ----------------------------------------- structs getter -----------------------------------------
    internal bool Has(in ArchetypeStructs other) {
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
    
    public  bool    HasAll (in ArchetypeStructs structs)
    {
        return bitSet.HasAll(structs.bitSet);
    }
    
    public  bool    HasAny (in ArchetypeStructs structs)
    {
        return bitSet.HasAny(structs.bitSet);
    }
    
    // ----------------------------------------- mutate Mask -----------------------------------------
    
    public void Add<T>()
        where T : struct, IStructComponent
    {
        bitSet.SetBit(StructHeap<T>.StructIndex);
    }
    
    public void Add(in ArchetypeStructs structs)
    {
        bitSet.value |= structs.bitSet.value;
    }
    
    public void Remove<T>()
        where T : struct, IStructComponent
    {
        bitSet.ClearBit(StructHeap<T>.StructIndex);
    }
    
    public void Remove(in ArchetypeStructs structs)
    {
        bitSet.value &= ~structs.bitSet.value;
    }
    
    // ----------------------------------------- static methods -----------------------------------------    
    public static ArchetypeStructs Get<T>()
        where T : struct, IStructComponent
    {
        var structs = new ArchetypeStructs();
        structs.bitSet.SetBit(StructHeap<T>.StructIndex);
        return structs;
    }
    
    public static ArchetypeStructs Get<T1, T2>()
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
    {
        var structs = new ArchetypeStructs();
        structs.bitSet.SetBit(StructHeap<T1>.StructIndex);
        structs.bitSet.SetBit(StructHeap<T2>.StructIndex);
        return structs;
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append("Mask: [");
        var hasTypes    = false;
        foreach (var index in bitSet) {
            var structType = EntityStore.Static.ComponentSchema.GetStructComponentAt(index);
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
    
    internal ArchetypeMaskEnumerator(in ArchetypeStructs structs) {
        bitSetEnumerator = structs.bitSet.GetEnumerator();
    }
    
    // --- IEnumerator
    public bool MoveNext() => bitSetEnumerator.MoveNext();
    public void Dispose() { }
}