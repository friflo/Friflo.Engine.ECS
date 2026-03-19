using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators;

public partial class SystemGeneratorExample : BaseSystem
{
    [Query]
    static void MoveExample(ref Position position, float move) {
        position.x += move;
    }

    protected override void OnUpdateGroup() {
        foreach (var store in SystemRoot.Stores) {
            MoveExampleQuery(store, 42);
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
        AreEqual(42, entity.GetComponent<Position>().x);
    }
}