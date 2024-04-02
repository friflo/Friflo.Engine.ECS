// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class EntityExtensions
{
internal static ComponentTypes GetComponentTypes<T1>()
        where T1 : struct, IComponent
    {
        var result  = new ComponentTypes();
        ref var bitSet = ref result.bitSet;
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        return result;
    }
    
    internal static ComponentTypes GetComponentTypes<T1, T2>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        var result  = new ComponentTypes();
        ref var bitSet = ref result.bitSet;
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        return result;
    }
    
    internal static ComponentTypes GetComponentTypes<T1, T2, T3>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        var result  = new ComponentTypes();
        ref var bitSet = ref result.bitSet;
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        return result;
    }
    
    internal static ComponentTypes GetComponentTypes<T1, T2, T3, T4>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        var result  = new ComponentTypes();
        ref var bitSet = ref result.bitSet;
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
        return result;
    }
    
    internal static ComponentTypes GetComponentTypes<T1, T2, T3, T4, T5>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
    {
        var result  = new ComponentTypes();
        ref var bitSet = ref result.bitSet;
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
        bitSet.SetBit(StructHeap<T5>.StructIndex);
        return result;
    }
    
    internal static ComponentTypes GetComponentTypes<T1, T2, T3, T4, T5, T6>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
    {
        var result  = new ComponentTypes();
        ref var bitSet = ref result.bitSet;
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
        bitSet.SetBit(StructHeap<T5>.StructIndex);
        bitSet.SetBit(StructHeap<T6>.StructIndex);
        return result;
    }
    
    internal static ComponentTypes GetComponentTypes<T1, T2, T3, T4, T5, T6, T7>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
    {
        var result  = new ComponentTypes();
        ref var bitSet = ref result.bitSet;
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
        bitSet.SetBit(StructHeap<T5>.StructIndex);
        bitSet.SetBit(StructHeap<T6>.StructIndex);
        bitSet.SetBit(StructHeap<T7>.StructIndex);
        return result;
    }
    
    internal static ComponentTypes GetComponentTypes<T1, T2, T3, T4, T5, T6, T7, T8>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
        where T8 : struct, IComponent
    {
        var result  = new ComponentTypes();
        ref var bitSet = ref result.bitSet;
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
        bitSet.SetBit(StructHeap<T5>.StructIndex);
        bitSet.SetBit(StructHeap<T6>.StructIndex);
        bitSet.SetBit(StructHeap<T7>.StructIndex);
        bitSet.SetBit(StructHeap<T8>.StructIndex);
        return result;
    }
    
    internal static ComponentTypes GetComponentTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
        where T8 : struct, IComponent
        where T9 : struct, IComponent
    {
        var result  = new ComponentTypes();
        ref var bitSet = ref result.bitSet;
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
        bitSet.SetBit(StructHeap<T5>.StructIndex);
        bitSet.SetBit(StructHeap<T6>.StructIndex);
        bitSet.SetBit(StructHeap<T7>.StructIndex);
        bitSet.SetBit(StructHeap<T8>.StructIndex);
        bitSet.SetBit(StructHeap<T9>.StructIndex);
        return result;
    }
    
    internal static ComponentTypes GetComponentTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
        where T8 : struct, IComponent
        where T9 : struct, IComponent
        where T10 : struct, IComponent
    {
        var result  = new ComponentTypes();
        ref var bitSet = ref result.bitSet;
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
        bitSet.SetBit(StructHeap<T5>.StructIndex);
        bitSet.SetBit(StructHeap<T6>.StructIndex);
        bitSet.SetBit(StructHeap<T7>.StructIndex);
        bitSet.SetBit(StructHeap<T8>.StructIndex);
        bitSet.SetBit(StructHeap<T9>.StructIndex);
        bitSet.SetBit(StructHeap<T10>.StructIndex);
        return result;
    }
}
