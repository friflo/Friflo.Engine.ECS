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
        AreEqual("entity: 1 - Add Position", command.ToString());
        
        var schema          = EntityStore.GetEntitySchema();
        var componentType   = schema.GetComponentType<Position>();
        var commands        = componentType.CreateComponentCommands();
        AreEqual("[Position] commands - Count: 0", commands.ToString());
    }
    
    #pragma warning disable CS0618 // Type or member is obsolete  TODO remove
    [Test]
    public static void Test_CommandBuffer_IncreaseCommands()
    {
        int count   = 10;
        var store   = new EntityStore(PidType.UsePidAsId);
        for (int n = 0; n < count; n++) {
            store.CreateEntity();
        }
        var ecb     = new EntityCommandBuffer(store);
        
        for (int n = 0; n < 10; n++) {
            ecb.AddComponent<Position>(n + 1);    
        }
        
    }
}

