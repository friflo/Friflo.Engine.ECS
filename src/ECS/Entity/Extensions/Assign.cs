// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable UseNullPropagation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class EntityExtensions
{
    internal static void AssignComponents<T1>(
        Archetype   archetype,
        int         compIndex,
        in T1       component1)
            where T1 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        ((StructHeap<T1>)heapMap[StructInfo<T1>.Index]).components[compIndex] = component1;
    }

    internal static void AssignComponents<T1, T2>(
        Archetype   archetype,
        int         compIndex,
        in T1       component1,
        in T2       component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        ((StructHeap<T1>)heapMap[StructInfo<T1>.Index]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructInfo<T2>.Index]).components[compIndex] = component2;
    }

    internal static void AssignComponents<T1, T2, T3>(
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
        ((StructHeap<T1>)heapMap[StructInfo<T1>.Index]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructInfo<T2>.Index]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructInfo<T3>.Index]).components[compIndex] = component3;
    }

    internal static void AssignComponents<T1, T2, T3, T4>(
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
        ((StructHeap<T1>)heapMap[StructInfo<T1>.Index]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructInfo<T2>.Index]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructInfo<T3>.Index]).components[compIndex] = component3;
        ((StructHeap<T4>)heapMap[StructInfo<T4>.Index]).components[compIndex] = component4;
    }

    internal static void AssignComponents<T1, T2, T3, T4, T5>(
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
        ((StructHeap<T1>)heapMap[StructInfo<T1>.Index]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructInfo<T2>.Index]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructInfo<T3>.Index]).components[compIndex] = component3;
        ((StructHeap<T4>)heapMap[StructInfo<T4>.Index]).components[compIndex] = component4;
        ((StructHeap<T5>)heapMap[StructInfo<T5>.Index]).components[compIndex] = component5;
    }
    
    internal static void AssignComponents<T1, T2, T3, T4, T5, T6>(
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
        ((StructHeap<T1>)heapMap[StructInfo<T1>.Index]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructInfo<T2>.Index]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructInfo<T3>.Index]).components[compIndex] = component3;
        ((StructHeap<T4>)heapMap[StructInfo<T4>.Index]).components[compIndex] = component4;
        ((StructHeap<T5>)heapMap[StructInfo<T5>.Index]).components[compIndex] = component5;
        ((StructHeap<T6>)heapMap[StructInfo<T6>.Index]).components[compIndex] = component6;
    }
    
   
    internal static void AssignComponents<T1, T2, T3, T4, T5, T6, T7>(
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
        ((StructHeap<T1>)heapMap[StructInfo<T1>.Index]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructInfo<T2>.Index]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructInfo<T3>.Index]).components[compIndex] = component3;
        ((StructHeap<T4>)heapMap[StructInfo<T4>.Index]).components[compIndex] = component4;
        ((StructHeap<T5>)heapMap[StructInfo<T5>.Index]).components[compIndex] = component5;
        ((StructHeap<T6>)heapMap[StructInfo<T6>.Index]).components[compIndex] = component6;
        ((StructHeap<T7>)heapMap[StructInfo<T7>.Index]).components[compIndex] = component7;
    }
    
    internal static void AssignComponents<T1, T2, T3, T4, T5, T6, T7, T8>(
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
        ((StructHeap<T1>)heapMap[StructInfo<T1>.Index]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructInfo<T2>.Index]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructInfo<T3>.Index]).components[compIndex] = component3;
        ((StructHeap<T4>)heapMap[StructInfo<T4>.Index]).components[compIndex] = component4;
        ((StructHeap<T5>)heapMap[StructInfo<T5>.Index]).components[compIndex] = component5;
        ((StructHeap<T6>)heapMap[StructInfo<T6>.Index]).components[compIndex] = component6;
        ((StructHeap<T7>)heapMap[StructInfo<T7>.Index]).components[compIndex] = component7;
        ((StructHeap<T8>)heapMap[StructInfo<T8>.Index]).components[compIndex] = component8;
    }
    
    internal static void AssignComponents<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
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
        ((StructHeap<T1>)heapMap[StructInfo<T1>.Index]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructInfo<T2>.Index]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructInfo<T3>.Index]).components[compIndex] = component3;
        ((StructHeap<T4>)heapMap[StructInfo<T4>.Index]).components[compIndex] = component4;
        ((StructHeap<T5>)heapMap[StructInfo<T5>.Index]).components[compIndex] = component5;
        ((StructHeap<T6>)heapMap[StructInfo<T6>.Index]).components[compIndex] = component6;
        ((StructHeap<T7>)heapMap[StructInfo<T7>.Index]).components[compIndex] = component7;
        ((StructHeap<T8>)heapMap[StructInfo<T8>.Index]).components[compIndex] = component8;
        ((StructHeap<T9>)heapMap[StructInfo<T9>.Index]).components[compIndex] = component9;
    }
    
    internal static void AssignComponents<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
        ((StructHeap<T1>)heapMap[StructInfo<T1>.Index]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructInfo<T2>.Index]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructInfo<T3>.Index]).components[compIndex] = component3;
        ((StructHeap<T4>)heapMap[StructInfo<T4>.Index]).components[compIndex] = component4;
        ((StructHeap<T5>)heapMap[StructInfo<T5>.Index]).components[compIndex] = component5;
        ((StructHeap<T6>)heapMap[StructInfo<T6>.Index]).components[compIndex] = component6;
        ((StructHeap<T7>)heapMap[StructInfo<T7>.Index]).components[compIndex] = component7;
        ((StructHeap<T8>)heapMap[StructInfo<T8>.Index]).components[compIndex] = component8;
        ((StructHeap<T9>)heapMap[StructInfo<T9>.Index]).components[compIndex] = component9;
        ((StructHeap<T10>)heapMap[StructInfo<T10>.Index]).components[compIndex] = component10;
    }
}
