using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
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
    
    [Test]
    public static void Test_CommandBuffer_debug_properties()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        
        var ecb = store.GetCommandBuffer();
        
        AreEqual(0, ecb.ComponentCommandsCount);
        
        ecb.CreateEntity();
        
        ecb.AddTag          <TestTag>(1);
        ecb.AddComponent    <Position>(1);
        ecb.RemoveComponent <Position>(1);
        
        
        AreEqual(2,                                 ecb.ComponentCommandsCount);
        var componentCommands = (ComponentCommands<Position>)ecb.ComponentCommands[0];
        AreEqual(2,                                 componentCommands.commandCount);
        AreEqual("[Position] commands - Count: 2",  componentCommands.ToString());
        AreEqual(2,                                 componentCommands.Commands.Length);
        AreEqual("entity: 1 - Add [Position]",      componentCommands.Commands[0].ToString());
        AreEqual("entity: 1 - Remove [Position]",   componentCommands.Commands[1].ToString());
        
        AreEqual(1,                                 ecb.TagCommandsCount);
        AreEqual(1,                                 ecb.TagCommands.Length);
        AreEqual("entity: 1 - Add [#TestTag]",      ecb.TagCommands[0].ToString());
        
        AreEqual(1,                                 ecb.EntityCommandsCount);
        AreEqual(1,                                 ecb.EntityCommands.Length);
        AreEqual("Create entity - id: 1",           ecb.EntityCommands[0].ToString());
    }
}

