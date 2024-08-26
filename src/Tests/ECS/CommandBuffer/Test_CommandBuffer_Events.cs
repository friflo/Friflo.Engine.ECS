using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS.Buffer {

public static class Test_CommandBuffer_Events
{
    [Test]
    public static void Test_CommandBuffer_Events_Components()
    {
        var store   = new EntityStore();
        int addCount = 0;
        store.OnComponentAdded += changed => {
            switch (addCount++) {
                case 0:
                    Mem.IsTrue(ComponentChangedAction.Add == changed.Action);
                    break;
                case 1:
                    Mem.IsTrue(ComponentChangedAction.Update == changed.Action);
                    var old = changed.OldComponent<Position>();
                    Mem.AreEqual(new Position(1,1,1), old);
                    break;
           }
        };
        int removeCount = 0;
        store.OnComponentRemoved += changed => {
            switch (removeCount++) {
                case 0:
                    Mem.IsTrue(ComponentChangedAction.Remove == changed.Action);
                    var old = changed.OldComponent<Position>();
                    Mem.AreEqual(new Position(2,2,2), old);
                    break;
            }
        };
        var entity  = store.CreateEntity();
        var ecb     = store.GetCommandBuffer();
        ecb.ReuseBuffer = true;

        ecb.AddComponent(entity.Id, new Position(1,1,1));
        ecb.Playback(); // Add component
        
        ecb.AddComponent(entity.Id, new Position(2,2,2));
        ecb.Playback(); // Update component

        ecb.RemoveComponent<Position>(entity.Id);
        ecb.Playback(); // Remove component
        
        ecb.RemoveComponent<Position>(entity.Id);
        ecb.Playback(); // Remove component - already removed
        
        AreEqual(2, addCount);
        AreEqual(1, removeCount);
    }
    
    [Test]
    public static void Test_CommandBuffer_Events_Tags()
    {
        var store   = new EntityStore();
        int tagEventCount = 0;
        store.OnTagsChanged += changed => {
            switch (tagEventCount++) {
                case 0:
                    Mem.IsTrue(Tags.Get<TestTag>() == changed.AddedTags);
                    break;
                case 1:
                    Mem.IsTrue(Tags.Get<TestTag>() == changed.RemovedTags);
                    break;
            }
        };
        var entity  = store.CreateEntity();
        var ecb     = store.GetCommandBuffer();
        ecb.ReuseBuffer = true;
        
        ecb.AddTag<TestTag>(entity.Id);
        ecb.Playback(); // Add Tag

        ecb.AddTag<TestTag>(entity.Id);
        ecb.Playback(); // Add Tag - already added
        
        ecb.RemoveTag<TestTag>(entity.Id);
        ecb.Playback(); // Remove Tag
        
        ecb.RemoveTag<TestTag>(entity.Id);
        ecb.Playback(); // Remove Tag - already removed
        
        AreEqual(2, tagEventCount);
    }
    
    [Test]
    public static void Test_CommandBuffer_AddRemoveComponent_Perf()
    {
        int repeat  = 10; // 1_000_000 ~ #PC: 10.298 sec
        var store   = new EntityStore();
        var entities = new Entity[100];
        for (int n = 0; n < entities.Length; n++) {
            entities[n] = store.CreateEntity();
        }
        var ecb     = store.GetCommandBuffer();
        ecb.ReuseBuffer = true;
        for (int i = 0; i < repeat; i++) {
            foreach (var entity in entities) {
                ecb.AddComponent(entity.Id, new Position());
            }
            ecb.Playback();
            foreach (var entity in entities) {
                ecb.RemoveComponent<Position>(entity.Id);
            }
            ecb.Playback();
        }
    }
}

}