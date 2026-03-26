using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using Tests.Utils;


// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Query;

public partial class SystemGeneratorExample : BaseSystem
{
    [Query]
    static void MovePosition(ref Position position, float move) {
        position.x += move;
    }

    protected override void OnUpdateGroup() {
        foreach (var store in SystemRoot.Stores) {
            MovePositionQuery(store, 42);
        }
    }
}

public static class Test_SystemGeneratorExample
{
    [Test]
    public static void Test_SystemGeneratorExample_run()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity(new Position());
        var root = new SystemRoot(store) {
            new SystemGeneratorExample()
        };
        root.Update(default);
        Assert.That(entity.GetComponent<Position>().x, Is.EqualTo(42));
        
        var start = Mem.GetAllocatedBytes();
        root.Update(default);
        Mem.AssertNoAlloc(start);
    }
}