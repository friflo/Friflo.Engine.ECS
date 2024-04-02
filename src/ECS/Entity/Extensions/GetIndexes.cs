// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class EntityExtensions
{
    internal static SignatureIndexes GetIndexes<T1>()
        where T1 : struct, IComponent
    {
        return new SignatureIndexes(1,
            T1: StructHeap<T1>.StructIndex);
    }
    
    internal static SignatureIndexes GetIndexes<T1, T2>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        return new SignatureIndexes(2,
            T1: StructHeap<T1>.StructIndex,
            T2: StructHeap<T2>.StructIndex);
    }
    
    internal static SignatureIndexes GetIndexes<T1, T2, T3>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        return new SignatureIndexes(3,
            T1: StructHeap<T1>.StructIndex,
            T2: StructHeap<T2>.StructIndex,
            T3: StructHeap<T3>.StructIndex);
    }
    
    internal static SignatureIndexes GetIndexes<T1, T2, T3, T4>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        return new SignatureIndexes(4,
            T1: StructHeap<T1>.StructIndex,
            T2: StructHeap<T2>.StructIndex,
            T3: StructHeap<T3>.StructIndex,
            T4: StructHeap<T4>.StructIndex);
    }
    
    internal static SignatureIndexes GetIndexes<T1, T2, T3, T4, T5>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
    {
        return new SignatureIndexes(5,
            T1: StructHeap<T1>.StructIndex,
            T2: StructHeap<T2>.StructIndex,
            T3: StructHeap<T3>.StructIndex,
            T4: StructHeap<T4>.StructIndex,
            T5: StructHeap<T5>.StructIndex);
    }
    
    internal static SignatureIndexes GetIndexes<T1, T2, T3, T4, T5, T6>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
    {
        return new SignatureIndexes(6,
            T1: StructHeap<T1>.StructIndex,
            T2: StructHeap<T2>.StructIndex,
            T3: StructHeap<T3>.StructIndex,
            T4: StructHeap<T4>.StructIndex,
            T5: StructHeap<T5>.StructIndex,
            T6: StructHeap<T6>.StructIndex);
    }
    
    internal static SignatureIndexes GetIndexes<T1, T2, T3, T4, T5, T6, T7>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
    {
        return new SignatureIndexes(7,
            T1: StructHeap<T1>.StructIndex,
            T2: StructHeap<T2>.StructIndex,
            T3: StructHeap<T3>.StructIndex,
            T4: StructHeap<T4>.StructIndex,
            T5: StructHeap<T5>.StructIndex,
            T6: StructHeap<T6>.StructIndex,
            T7: StructHeap<T7>.StructIndex);
    }
    
    internal static SignatureIndexes GetIndexes<T1, T2, T3, T4, T5, T6, T7, T8>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
        where T8 : struct, IComponent
    {
        return new SignatureIndexes(8,
            T1: StructHeap<T1>.StructIndex,
            T2: StructHeap<T2>.StructIndex,
            T3: StructHeap<T3>.StructIndex,
            T4: StructHeap<T4>.StructIndex,
            T5: StructHeap<T5>.StructIndex,
            T6: StructHeap<T6>.StructIndex,
            T7: StructHeap<T7>.StructIndex,
            T8: StructHeap<T8>.StructIndex);
    }
    
    internal static SignatureIndexes GetIndexes<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
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
        return new SignatureIndexes(9,
            T1: StructHeap<T1>.StructIndex,
            T2: StructHeap<T2>.StructIndex,
            T3: StructHeap<T3>.StructIndex,
            T4: StructHeap<T4>.StructIndex,
            T5: StructHeap<T5>.StructIndex,
            T6: StructHeap<T6>.StructIndex,
            T7: StructHeap<T7>.StructIndex,
            T8: StructHeap<T8>.StructIndex,
            T9: StructHeap<T9>.StructIndex);
    }
    
    internal static SignatureIndexes GetIndexes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
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
        return new SignatureIndexes(10,
            T1: StructHeap<T1>.StructIndex,
            T2: StructHeap<T2>.StructIndex,
            T3: StructHeap<T3>.StructIndex,
            T4: StructHeap<T4>.StructIndex,
            T5: StructHeap<T5>.StructIndex,
            T6: StructHeap<T6>.StructIndex,
            T7: StructHeap<T7>.StructIndex,
            T8: StructHeap<T8>.StructIndex,
            T9: StructHeap<T9>.StructIndex,
            T10:StructHeap<T10>.StructIndex);
    }
}
