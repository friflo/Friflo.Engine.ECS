// ReSharper disable once CheckNamespace
namespace Fliox.Engine.ECS;

internal class StructEnumerator<T> where T : struct
{
    private readonly StructHeap<T> heap;
    
    internal  StructEnumerator(StructHeap<T> heap) {
        this.heap = heap;
    }
}
