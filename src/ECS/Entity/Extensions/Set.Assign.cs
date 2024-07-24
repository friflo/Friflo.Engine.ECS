// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable UseNullPropagation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class EntityExtensions
{
    private static bool SetAssignComponents<T1>(
        Archetype   archetype,
        int         compIndex,
        in T1       component1)
        where T1 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        var heap1 = heapMap[StructInfo<T1>.Index];

        if (heap1 == null) {
            return false;
        }
        ((StructHeap<T1>)heap1).components[compIndex] = component1;
        return true;
    }
    
    private static bool SetAssignComponents<T1, T2>(
        Archetype   archetype,
        int         compIndex,
        in T1       component1,
        in T2       component2)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        var heap1 = heapMap[StructInfo<T1>.Index];
        var heap2 = heapMap[StructInfo<T2>.Index];
        if (heap1 == null ||
            heap2 == null) {
            return false;
        }
        ((StructHeap<T1>)heap1).components[compIndex] = component1;
        ((StructHeap<T2>)heap2).components[compIndex] = component2;
        return true;
    }
    
    private static bool SetAssignComponents<T1, T2, T3>(
        Archetype   archetype,
        int         compIndex,
        in T1       component1,
        in T2       component2,
        in T3       component3)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        var heap1 = heapMap[StructInfo<T1>.Index];
        var heap2 = heapMap[StructInfo<T2>.Index];
        var heap3 = heapMap[StructInfo<T3>.Index];
        if (heap1 == null ||
            heap2 == null ||
            heap3 == null) {
            return false;
        }
        ((StructHeap<T1>)heap1).components[compIndex] = component1;
        ((StructHeap<T2>)heap2).components[compIndex] = component2;
        ((StructHeap<T3>)heap3).components[compIndex] = component3;
        return true;
    }
    
    private static bool SetAssignComponents<T1, T2, T3, T4>(
        Archetype   archetype,
        int         compIndex,
        in T1       component1,
        in T2       component2,
        in T3       component3,
        in T4       component4)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        var heap1 = heapMap[StructInfo<T1>.Index];
        var heap2 = heapMap[StructInfo<T2>.Index];
        var heap3 = heapMap[StructInfo<T3>.Index];
        var heap4 = heapMap[StructInfo<T4>.Index];
        if (heap1 == null ||
            heap2 == null ||
            heap3 == null ||
            heap4 == null) {
            return false;
        }
        ((StructHeap<T1>)heap1).components[compIndex] = component1;
        ((StructHeap<T2>)heap2).components[compIndex] = component2;
        ((StructHeap<T3>)heap3).components[compIndex] = component3;
        ((StructHeap<T4>)heap4).components[compIndex] = component4;
        return true;
    }
    
    private static bool SetAssignComponents<T1, T2, T3, T4, T5>(
        Archetype   archetype,
        int         compIndex,
        in T1       component1,
        in T2       component2,
        in T3       component3,
        in T4       component4,
        in T5       component5)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        var heap1 = heapMap[StructInfo<T1>.Index];
        var heap2 = heapMap[StructInfo<T2>.Index];
        var heap3 = heapMap[StructInfo<T3>.Index];
        var heap4 = heapMap[StructInfo<T4>.Index];
        var heap5 = heapMap[StructInfo<T5>.Index];
        if (heap1 == null ||
            heap2 == null ||
            heap3 == null ||
            heap4 == null ||
            heap5 == null) {
            return false;
        }
        ((StructHeap<T1>)heap1).components[compIndex] = component1;
        ((StructHeap<T2>)heap2).components[compIndex] = component2;
        ((StructHeap<T3>)heap3).components[compIndex] = component3;
        ((StructHeap<T4>)heap4).components[compIndex] = component4;
        ((StructHeap<T5>)heap5).components[compIndex] = component5;
        return true;
    }
    
    private static bool SetAssignComponents<T1, T2, T3, T4, T5, T6>(
        Archetype   archetype,
        int         compIndex,
        in T1       component1,
        in T2       component2,
        in T3       component3,
        in T4       component4,
        in T5       component5,
        in T6       component6)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        var heap1 = heapMap[StructInfo<T1>.Index];
        var heap2 = heapMap[StructInfo<T2>.Index];
        var heap3 = heapMap[StructInfo<T3>.Index];
        var heap4 = heapMap[StructInfo<T4>.Index];
        var heap5 = heapMap[StructInfo<T5>.Index];
        var heap6 = heapMap[StructInfo<T6>.Index];
        if (heap1 == null ||
            heap2 == null ||
            heap3 == null ||
            heap4 == null ||
            heap5 == null ||
            heap6 == null) {
            return false;
        }
        ((StructHeap<T1>)heap1).components[compIndex] = component1;
        ((StructHeap<T2>)heap2).components[compIndex] = component2;
        ((StructHeap<T3>)heap3).components[compIndex] = component3;
        ((StructHeap<T4>)heap4).components[compIndex] = component4;
        ((StructHeap<T5>)heap5).components[compIndex] = component5;
        ((StructHeap<T6>)heap6).components[compIndex] = component6;
        return true;
    }
    
    private static bool SetAssignComponents<T1, T2, T3, T4, T5, T6, T7>(
        Archetype   archetype,
        int         compIndex,
        in T1       component1,
        in T2       component2,
        in T3       component3,
        in T4       component4,
        in T5       component5,
        in T6       component6,
        in T7       component7)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        var heap1 = heapMap[StructInfo<T1>.Index];
        var heap2 = heapMap[StructInfo<T2>.Index];
        var heap3 = heapMap[StructInfo<T3>.Index];
        var heap4 = heapMap[StructInfo<T4>.Index];
        var heap5 = heapMap[StructInfo<T5>.Index];
        var heap6 = heapMap[StructInfo<T6>.Index];
        var heap7 = heapMap[StructInfo<T7>.Index];
        if (heap1 == null ||
            heap2 == null ||
            heap3 == null ||
            heap4 == null ||
            heap5 == null ||
            heap6 == null ||
            heap7 == null) {
            return false;
        }
        ((StructHeap<T1>)heap1).components[compIndex] = component1;
        ((StructHeap<T2>)heap2).components[compIndex] = component2;
        ((StructHeap<T3>)heap3).components[compIndex] = component3;
        ((StructHeap<T4>)heap4).components[compIndex] = component4;
        ((StructHeap<T5>)heap5).components[compIndex] = component5;
        ((StructHeap<T6>)heap6).components[compIndex] = component6;
        ((StructHeap<T7>)heap7).components[compIndex] = component7;
        return true;
    }
        
    private static bool SetAssignComponents<T1, T2, T3, T4, T5, T6, T7, T8>(
        Archetype   archetype,
        int         compIndex,
        in T1       component1,
        in T2       component2,
        in T3       component3,
        in T4       component4,
        in T5       component5,
        in T6       component6,
        in T7       component7,
        in T8       component8)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
        where T8 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        var heap1 = heapMap[StructInfo<T1>.Index];
        var heap2 = heapMap[StructInfo<T2>.Index];
        var heap3 = heapMap[StructInfo<T3>.Index];
        var heap4 = heapMap[StructInfo<T4>.Index];
        var heap5 = heapMap[StructInfo<T5>.Index];
        var heap6 = heapMap[StructInfo<T6>.Index];
        var heap7 = heapMap[StructInfo<T7>.Index];
        var heap8 = heapMap[StructInfo<T8>.Index];
        if (heap1 == null ||
            heap2 == null ||
            heap3 == null ||
            heap4 == null ||
            heap5 == null ||
            heap6 == null ||
            heap7 == null ||
            heap8 == null) {
            return false;
        }
        ((StructHeap<T1>)heap1).components[compIndex] = component1;
        ((StructHeap<T2>)heap2).components[compIndex] = component2;
        ((StructHeap<T3>)heap3).components[compIndex] = component3;
        ((StructHeap<T4>)heap4).components[compIndex] = component4;
        ((StructHeap<T5>)heap5).components[compIndex] = component5;
        ((StructHeap<T6>)heap6).components[compIndex] = component6;
        ((StructHeap<T7>)heap7).components[compIndex] = component7;
        ((StructHeap<T8>)heap8).components[compIndex] = component8;
        return true;
    }
    
    private static bool SetAssignComponents<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Archetype   archetype,
        int         compIndex,
        in T1       component1,
        in T2       component2,
        in T3       component3,
        in T4       component4,
        in T5       component5,
        in T6       component6,
        in T7       component7,
        in T8       component8,
        in T9       component9)
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
        var heapMap = archetype.heapMap;
        var heap1 = heapMap[StructInfo<T1>.Index];
        var heap2 = heapMap[StructInfo<T2>.Index];
        var heap3 = heapMap[StructInfo<T3>.Index];
        var heap4 = heapMap[StructInfo<T4>.Index];
        var heap5 = heapMap[StructInfo<T5>.Index];
        var heap6 = heapMap[StructInfo<T6>.Index];
        var heap7 = heapMap[StructInfo<T7>.Index];
        var heap8 = heapMap[StructInfo<T8>.Index];
        var heap9 = heapMap[StructInfo<T9>.Index];
        if (heap1 == null ||
            heap2 == null ||
            heap3 == null ||
            heap4 == null ||
            heap5 == null ||
            heap6 == null ||
            heap7 == null ||
            heap8 == null ||
            heap9 == null) {
            return false;
        }
        ((StructHeap<T1>)heap1).components[compIndex] = component1;
        ((StructHeap<T2>)heap2).components[compIndex] = component2;
        ((StructHeap<T3>)heap3).components[compIndex] = component3;
        ((StructHeap<T4>)heap4).components[compIndex] = component4;
        ((StructHeap<T5>)heap5).components[compIndex] = component5;
        ((StructHeap<T6>)heap6).components[compIndex] = component6;
        ((StructHeap<T7>)heap7).components[compIndex] = component7;
        ((StructHeap<T8>)heap8).components[compIndex] = component8;
        ((StructHeap<T9>)heap9).components[compIndex] = component9;
        return true;
    }
    
    private static bool SetAssignComponents<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Archetype   archetype,
        int         compIndex,
        in T1       component1,
        in T2       component2,
        in T3       component3,
        in T4       component4,
        in T5       component5,
        in T6       component6,
        in T7       component7,
        in T8       component8,
        in T9       component9,
        in T10      component10)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
        where T8 : struct, IComponent
        where T9 : struct, IComponent
        where T10: struct, IComponent
    {
        var heapMap = archetype.heapMap;
        var heap1 = heapMap[StructInfo<T1>.Index];
        var heap2 = heapMap[StructInfo<T2>.Index];
        var heap3 = heapMap[StructInfo<T3>.Index];
        var heap4 = heapMap[StructInfo<T4>.Index];
        var heap5 = heapMap[StructInfo<T5>.Index];
        var heap6 = heapMap[StructInfo<T6>.Index];
        var heap7 = heapMap[StructInfo<T7>.Index];
        var heap8 = heapMap[StructInfo<T8>.Index];
        var heap9 = heapMap[StructInfo<T9>.Index];
        var heap10= heapMap[StructInfo<T10>.Index];
        if (heap1 == null ||
            heap2 == null ||
            heap3 == null ||
            heap4 == null ||
            heap5 == null ||
            heap6 == null ||
            heap7 == null ||
            heap8 == null ||
            heap9 == null ||
            heap10== null) {
            return false;
        }
        ((StructHeap<T1>)heap1).components[compIndex] = component1;
        ((StructHeap<T2>)heap2).components[compIndex] = component2;
        ((StructHeap<T3>)heap3).components[compIndex] = component3;
        ((StructHeap<T4>)heap4).components[compIndex] = component4;
        ((StructHeap<T5>)heap5).components[compIndex] = component5;
        ((StructHeap<T6>)heap6).components[compIndex] = component6;
        ((StructHeap<T7>)heap7).components[compIndex] = component7;
        ((StructHeap<T8>)heap8).components[compIndex] = component8;
        ((StructHeap<T9>)heap9).components[compIndex] = component9;
        ((StructHeap<T10>)heap10).components[compIndex] = component10;
        return true;
    }
}
