using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_CommandBuffer
{
    [Test]
    public static void Test_CommandBuffer_ToString()
    {
        var command = new ComponentCommand<Position> {
            change      = ComponentChangedAction.Add,
            entityId    = 1
        };
        AreEqual("entity: 1 - Add [Position]", command.ToString());
        
        var schema          = EntityStore.GetEntitySchema();
        var componentType   = schema.GetComponentType<Position>();
        var commands        = componentType.CreateComponentCommands();
        AreEqual("[Position] commands - Count: 0", commands.ToString());
    }
}

