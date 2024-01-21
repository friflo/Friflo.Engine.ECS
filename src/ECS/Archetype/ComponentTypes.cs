// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Friflo.Engine.ECS.Utils;

// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// <see cref="ComponentTypes"/> define a set of <see cref="IComponent"/>'s used to list the
/// component <see cref="System.Type"/>'s of an <see cref="Archetype"/>.
/// </summary>
[CLSCompliant(true)]
public struct ComponentTypes : IEnumerable<ComponentType>
{
    internal        BitSet  bitSet;     // 32
    
    /// <summary>Return the number of contained <see cref="IComponent"/>'s.</summary>
    public readonly int                         Count                                       => bitSet.GetBitCount();
   
    public readonly ComponentTypesEnumerator    GetEnumerator()                             => new ComponentTypesEnumerator (this);

    // --- IEnumerable
           readonly IEnumerator                 IEnumerable.GetEnumerator()                 => new ComponentTypesEnumerator (this);

    // --- IEnumerable<>
           readonly IEnumerator<ComponentType>  IEnumerable<ComponentType>.GetEnumerator()  => new ComponentTypesEnumerator (this);

    public override string                      ToString() => GetString();
    
    internal ComponentTypes(StructHeap[] heaps) {
        foreach (var heap in heaps) {
            bitSet.SetBit(heap.structIndex);
        }
    }
    
    internal ComponentTypes(in SignatureIndexes indexes)
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
        Type5:   bitSet.SetBit(indexes.T5);
        Type4:   bitSet.SetBit(indexes.T4);
        Type3:   bitSet.SetBit(indexes.T3);
        Type2:   bitSet.SetBit(indexes.T2);
        Type1:   bitSet.SetBit(indexes.T1);
    }
    
    // ----------------------------------------- component getter -----------------------------------------
    public readonly bool    Has<T1> ()
        where T1 : struct, IComponent
    {
        return bitSet.Has(StructHeap<T1>.StructIndex);
    }
    
    public readonly bool    Has<T1, T2> ()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        return bitSet.Has(StructHeap<T1>.StructIndex) &&
               bitSet.Has(StructHeap<T2>.StructIndex);
    }

    public readonly bool    Has<T1, T2, T3> ()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        return bitSet.Has(StructHeap<T1>.StructIndex) &&
               bitSet.Has(StructHeap<T2>.StructIndex) &&
               bitSet.Has(StructHeap<T3>.StructIndex);
    }
    
    public readonly bool HasAll (in ComponentTypes componentTypes)
    {
        return bitSet.HasAll(componentTypes.bitSet);
    }
    
    public readonly bool HasAny (in ComponentTypes componentTypes)
    {
        return bitSet.HasAny(componentTypes.bitSet);
    }
    
    // ----------------------------------------- mutate Mask -----------------------------------------
    public void Add<T>()
        where T : struct, IComponent
    {
        bitSet.SetBit(StructHeap<T>.StructIndex);
    }
    
    public void Add(in ComponentTypes componentTypes)
    {
        bitSet.value |= componentTypes.bitSet.value;
    }
    
    public void Remove<T>()
        where T : struct, IComponent
    {
        bitSet.ClearBit(StructHeap<T>.StructIndex);
    }
    
    public void Remove(in ComponentTypes componentTypes)
    {
        bitSet.value &= ~componentTypes.bitSet.value;
    }
    
    // ----------------------------------------- static methods -----------------------------------------    
    public static ComponentTypes Get<T1>()
        where T1 : struct, IComponent
    {
        var componentTypes = new ComponentTypes();
        componentTypes.bitSet.SetBit(StructHeap<T1>.StructIndex);
        return componentTypes;
    }
    
    public static ComponentTypes Get<T1, T2>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        var componentTypes = new ComponentTypes();
        componentTypes.bitSet.SetBit(StructHeap<T1>.StructIndex);
        componentTypes.bitSet.SetBit(StructHeap<T2>.StructIndex);
        return componentTypes;
    }
    
    public static ComponentTypes Get<T1, T2, T3>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        var componentTypes = new ComponentTypes();
        componentTypes.bitSet.SetBit(StructHeap<T1>.StructIndex);
        componentTypes.bitSet.SetBit(StructHeap<T2>.StructIndex);
        componentTypes.bitSet.SetBit(StructHeap<T3>.StructIndex);
        return componentTypes;
    }
    
    public static ComponentTypes Get<T1, T2, T3, T4>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        var componentTypes = new ComponentTypes();
        componentTypes.bitSet.SetBit(StructHeap<T1>.StructIndex);
        componentTypes.bitSet.SetBit(StructHeap<T2>.StructIndex);
        componentTypes.bitSet.SetBit(StructHeap<T3>.StructIndex);
        componentTypes.bitSet.SetBit(StructHeap<T4>.StructIndex);
        return componentTypes;
    }
    
    public static ComponentTypes Get<T1, T2, T3, T4, T5>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
    {
        var componentTypes = new ComponentTypes();
        componentTypes.bitSet.SetBit(StructHeap<T1>.StructIndex);
        componentTypes.bitSet.SetBit(StructHeap<T2>.StructIndex);
        componentTypes.bitSet.SetBit(StructHeap<T3>.StructIndex);
        componentTypes.bitSet.SetBit(StructHeap<T4>.StructIndex);
        componentTypes.bitSet.SetBit(StructHeap<T5>.StructIndex);
        return componentTypes;
    }
    
    internal string GetString()
    {
        var sb = new StringBuilder();
        sb.Append("Components: [");
        var hasTypes    = false;
        foreach (var index in bitSet) {
            var structType = EntityStoreBase.Static.EntitySchema.components[index];
            sb.Append(structType.Name);
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

/// <summary>
/// Return the <see cref="IComponent"/>'s of <see cref="ComponentTypes"/>.
/// </summary>
public struct ComponentTypesEnumerator : IEnumerator<ComponentType>
{
    internal BitSetEnumerator       bitSetEnumerator;   // 48

    // --- IEnumerator
    public          void            Reset()             => bitSetEnumerator.Reset();

           readonly object          IEnumerator.Current => Current;

    public readonly ComponentType   Current => EntityStoreBase.Static.EntitySchema.components[bitSetEnumerator.Current];
    
    internal ComponentTypesEnumerator(in ComponentTypes componentTypes) {
        bitSetEnumerator = new BitSetEnumerator(componentTypes.bitSet);
    }
    
    // --- IEnumerator
    public          bool MoveNext() => bitSetEnumerator.MoveNext();
    public readonly void Dispose() { }
}