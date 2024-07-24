using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

namespace Internal.ECS {

// ReSharper disable InconsistentNaming
public static class Test_StructHeap
{
    [Test]
    public static void Test_StructHeap_Padding() {
        var schema = EntityStore.GetEntitySchema();
        
        var type    = schema.GetComponentType<MyComponent1>();
        var heap    = (StructHeap<MyComponent1>)type.CreateHeap();
        AreEqual(512, heap.components.Length);
    }
}

}