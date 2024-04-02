

// ReSharper disable CheckNamespace
namespace Friflo.Engine.ECS;

internal static class ComponentSet<T1,T2,T3,T4,T5>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
{
    internal static readonly SignatureIndexes Signature = new SignatureIndexes(5,
        StructHeap<T1>.StructIndex,
        StructHeap<T2>.StructIndex,
        StructHeap<T3>.StructIndex,
        StructHeap<T4>.StructIndex,
        StructHeap<T5>.StructIndex);

        
    internal static readonly ComponentTypes Type = GetType(
        StructHeap<T1>.StructIndex,
        StructHeap<T2>.StructIndex,
        StructHeap<T3>.StructIndex,
        StructHeap<T4>.StructIndex,
        StructHeap<T5>.StructIndex);
    
    private static ComponentTypes GetType(int index1, int index2, int index3, int index4, int index5) {
        var result  = new ComponentTypes();
        result.bitSet.SetBit(index1);
        result.bitSet.SetBit(index2);
        result.bitSet.SetBit(index3);
        result.bitSet.SetBit(index4);
        result.bitSet.SetBit(index5);
        return result;
    }
}