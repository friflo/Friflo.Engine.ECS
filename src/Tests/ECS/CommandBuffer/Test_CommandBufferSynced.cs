using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS.Buffer {

public static class Test_CommandBufferSynced
{
    [Test]
    public static void Test_CommandBuffe_commands()
    {
        var store       = new EntityStore();
        var cb          = store.GetCommandBuffer();
        cb.ReuseBuffer  = true;
        AddCommands(cb);
        AssertCommands(cb);
        
        cb.Playback();
        AssertChanges(store);
        
        AddCommands(cb);
        cb.Clear();
        AssertChanges(store);
    }
    
    [Test]
    public static void Test_CommandBufferSynced_commands()
    {
        var store       = new EntityStore();
        var cb          = store.GetCommandBuffer();
        cb.ReuseBuffer  = true;
        var synced      = cb.Synced;
        AddCommands(synced);
        AssertCommands(cb);
        
        synced.Playback();
        AssertChanges(store);
        
        AddCommands(synced);
        synced.Clear();
        AssertChanges(store);
    }
    
    private static void AddCommands(ICommandBuffer cb)
    {
        // --- entity
        var entity1 = cb.CreateEntity();
        var entity2 = cb.CreateEntity();
        var child1  = cb.CreateEntity();
        var child2  = cb.CreateEntity();
        cb.DeleteEntity(entity2);
        
        // --- component
        cb.AddComponent<Position>   (entity1);
        cb.AddComponent             (entity1, new Scale3(4,5,6));
        cb.AddComponent             (entity1, new EntityName("Test"));
        
        cb.RemoveComponent<EntityName>(entity1);
        cb.SetComponent             (entity1, new Position(1,2,3));
        
        // --- tag
        cb.AddTag<TestTag>          (entity1);
        cb.AddTags                  (entity1, Tags.Get<TestTag2, TestTag3, TestTag4>());
        cb.RemoveTag<TestTag3>      (entity1);
        cb.RemoveTags               (entity1, Tags.Get<TestTag4>());
        
        // --- script
        cb.AddScript                (entity1, new TestScript1());
        cb.AddScript                (entity1, new TestScript2());
        cb.RemoveScript<TestScript2>(entity1);
        
        // --- child
        cb.AddChild     (entity1, child1);
        cb.AddChild     (entity1, child2);
        cb.RemoveChild  (entity1, child2);
    }
    
    private static void AssertCommands(CommandBuffer cb)
    {
        AreEqual(5, cb.EntityCommandsCount);
        AreEqual(5, cb.ComponentCommandsCount);
        AreEqual(6, cb.TagCommandsCount);
        AreEqual(3, cb.ScriptCommandsCount);
        AreEqual(3, cb.ChildCommandsCount);
    }
    
    private static void AssertChanges(EntityStore store)
    {
        var entity1 = store.GetEntityById(1);
        IsFalse(store.TryGetEntityById(2, out _));
        var child1 = store.GetEntityById(3);
        var child2 = store.GetEntityById(4);
        
        // --- component
        AreEqual(new Position(1,2,3), entity1.GetComponent<Position>());
        AreEqual(new Scale3(4,5,6),   entity1.GetComponent<Scale3>());
        
        // --- tags
        IsTrue(Tags.Get<TestTag, TestTag2>() == entity1.Tags);
        
        // --- script
        NotNull(entity1.GetScript<TestScript1>());
        IsNull (entity1.GetScript<TestScript2>());
        
        // --- child
        AreEqual(entity1, child1.Parent);
        IsTrue  (child2.Parent.IsNull);
    }

}
}