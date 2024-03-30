// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using Friflo.Engine.ECS.Utils;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal static class EntityGeneric
{
#region set bits
    internal static void SetBits<T1, T2>(ref BitSet bitSet)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
    }
    
    internal static void SetBits<T1, T2, T3>(ref BitSet bitSet)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
    }
    
    internal static void SetBits<T1, T2, T3, T4>(ref BitSet bitSet)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
    }
    
    internal static void SetBits<T1, T2, T3, T4, T5>(ref BitSet bitSet)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
    {
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
        bitSet.SetBit(StructHeap<T5>.StructIndex);
    }
    #endregion
    
#region set components

    internal static void SetComponents<T1>(
        Archetype   archetype,
        int     compIndex,
        in T1   component1)
        where T1 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        ((StructHeap<T1>)heapMap[StructHeap<T1>.StructIndex]).components[compIndex] = component1;
    }

    internal static void SetComponents<T1, T2>(
        Archetype   archetype,
        int     compIndex,
        in T1   component1,
        in T2   component2)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        ((StructHeap<T1>)heapMap[StructHeap<T1>.StructIndex]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructHeap<T2>.StructIndex]).components[compIndex] = component2;
    }

    internal static void SetComponents<T1, T2, T3>(
        Archetype   archetype,
        int     compIndex,
        in T1   component1,
        in T2   component2,
        in T3   component3)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        ((StructHeap<T1>)heapMap[StructHeap<T1>.StructIndex]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructHeap<T2>.StructIndex]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructHeap<T3>.StructIndex]).components[compIndex] = component3;
    }

    internal static void SetComponents<T1, T2, T3, T4>(
        Archetype   archetype,
        int     compIndex,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        ((StructHeap<T1>)heapMap[StructHeap<T1>.StructIndex]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructHeap<T2>.StructIndex]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructHeap<T3>.StructIndex]).components[compIndex] = component3;
        ((StructHeap<T4>)heapMap[StructHeap<T4>.StructIndex]).components[compIndex] = component4;
    }

    internal static void SetComponents<T1, T2, T3, T4, T5>(
        Archetype   archetype,
        int     compIndex,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        ((StructHeap<T1>)heapMap[StructHeap<T1>.StructIndex]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructHeap<T2>.StructIndex]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructHeap<T3>.StructIndex]).components[compIndex] = component3;
        ((StructHeap<T4>)heapMap[StructHeap<T4>.StructIndex]).components[compIndex] = component4;
        ((StructHeap<T5>)heapMap[StructHeap<T5>.StructIndex]).components[compIndex] = component5;
    }
    #endregion
}