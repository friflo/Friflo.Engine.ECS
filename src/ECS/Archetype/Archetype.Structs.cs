// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[CLSCompliant(true)]
public struct ArchetypeStructs : IEnumerable<ComponentType>
{
    internal    BitSet  bitSet;     // 32
    
    public      int     Count       => bitSet.GetBitCount();
   
    public ArchetypeStructsEnumerator   GetEnumerator()                             => new ArchetypeStructsEnumerator (this);
    // --- IEnumerable
    IEnumerator                         IEnumerable.GetEnumerator()                 => new ArchetypeStructsEnumerator (this);
    // --- IEnumerable<>
    IEnumerator<ComponentType>          IEnumerable<ComponentType>.GetEnumerator()  => new ArchetypeStructsEnumerator (this);

    public   override                   string    ToString() => GetString();
    
    internal ArchetypeStructs(StructHeap[] heaps) {
        foreach (var heap in heaps) {
            SetBit(heap.structIndex);
        }
    }
    
    internal ArchetypeStructs(in SignatureIndexes indexes)
    {
        switch (indexes.length) {
            case 0: return;
            case 1: goto Type1;
            case 2: goto Type2;
            case 3: goto Type3;
            case 4: goto Type4;
            case 5: goto Type5;
        //  default: throw new IndexOutOfRangeException(); // unreachable - already ensured at SignatureIndexes
        }
        Type5:   SetBit(indexes.T5);
        Type4:   SetBit(indexes.T4);
        Type3:   SetBit(indexes.T3);
        Type2:   SetBit(indexes.T2);
        Type1:   SetBit(indexes.T1);
    }
    
    // ----------------------------------------- structs getter -----------------------------------------
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
    
    [Pure]
    public  bool    HasAll (in ArchetypeStructs structs)
    {
        return bitSet.HasAll(structs.bitSet);
    }
    
    [Pure]
    public  bool    HasAny (in ArchetypeStructs structs)
    {
        return bitSet.HasAny(structs.bitSet);
    }
    
    // ----------------------------------------- mutate Mask -----------------------------------------
    internal void SetBit(int structIndex) {
        bitSet.SetBit(structIndex);
    }
    
    internal void ClearBit(int structIndex) {
        bitSet.ClearBit(structIndex);
    }
    
    public void Add<T>()
        where T : struct, IStructComponent
    {
        SetBit(StructHeap<T>.StructIndex);
    }
    
    public void Add(in ArchetypeStructs structs)
    {
        bitSet.value |= structs.bitSet.value;
    }
    
    public void Remove<T>()
        where T : struct, IStructComponent
    {
        ClearBit(StructHeap<T>.StructIndex);
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
        structs.SetBit(StructHeap<T>.StructIndex);
        return structs;
    }
    
    public static ArchetypeStructs Get<T1, T2>()
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
    {
        var structs = new ArchetypeStructs();
        structs.SetBit(StructHeap<T1>.StructIndex);
        structs.SetBit(StructHeap<T2>.StructIndex);
        return structs;
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append("Structs: [");
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

public struct ArchetypeStructsEnumerator : IEnumerator<ComponentType>
{
    private BitSetEnumerator    bitSetEnumerator;

    // --- IEnumerator
    public  void                Reset()             => bitSetEnumerator.Reset();

            object              IEnumerator.Current => Current;

    public  ComponentType       Current => EntityStore.Static.ComponentSchema.GetStructComponentAt(bitSetEnumerator.Current);
    
    internal ArchetypeStructsEnumerator(in ArchetypeStructs structs) {
        bitSetEnumerator = structs.bitSet.GetEnumerator();
    }
    
    // --- IEnumerator
    public bool MoveNext() => bitSetEnumerator.MoveNext();
    public void Dispose() { }
}