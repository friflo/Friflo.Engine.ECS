using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch;

#pragma warning disable CS0618 // Type or member is obsolete

public static class Test_Entity_Events
{
    [Test]
    public static void Test_Entity_Events_OnTagsChanged()
    {
        var store       = new EntityStore();
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        
        var entity1EventCount = 0;
        var tagsChanged1 = (TagsChangedArgs args)    => {
            switch (entity1EventCount++) {
                case 0:     AreEqual("entity: 1 - tags change: Tags: [#TestTag]", args.ToString()); break;
                default:    Fail("unexpected"); break;
            }
        };
        var tagsChanged2 = (TagsChangedArgs args)    => { };
        
        entity1.OnTagsChanged -= tagsChanged1;  // remove event handler not added before. 
        entity1.OnTagsChanged += tagsChanged1;
        entity1.OnTagsChanged -= tagsChanged1;
        
        entity2.OnTagsChanged -= tagsChanged1;  // cover missing element in Dictionary<int, Action<TArgs>>

        entity1.OnTagsChanged += tagsChanged1;
        entity1.OnTagsChanged += tagsChanged2;  // cover adding action to Dictionary<int, Action<TArgs>>

        entity1.AddTag<TestTag>();
        entity2.AddTag<TestTag>();
        
        entity1.OnTagsChanged -= tagsChanged1;
        entity1.OnTagsChanged -= tagsChanged2;
        
        entity1.AddTag<TestTag>();
        entity2.AddTag<TestTag>();
        
        AreEqual(1, entity1EventCount);
    }
    
    [Test]
    public static void Test_Entity_Events_OnComponentChanged()
    {
        var store       = new EntityStore();
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);

        var entity1EventCount = 0;
        var onComponentChanged = (ComponentChangedArgs args)    => {
            switch (entity1EventCount++) {
                case 0:     AreEqual("entity: 1 - Add component: 'pos' [Position]", args.ToString()); break;
            }
        };
        entity1.OnComponentChanged += onComponentChanged; 

        entity1.AddComponent<Position>();
        entity2.AddComponent<Position>();
        
        var start = Mem.GetAllocatedBytes();
        entity1.AddComponent<Position>(); // event handler invocation executes without allocation
        Mem.AssertNoAlloc(start);
        
        entity1.OnComponentChanged -= onComponentChanged; 
        
        entity1.AddComponent<Rotation>();
        
        AreEqual(2, entity1EventCount);
    }
    
    [Test]
    public static void Test_Entity_Events_OnScriptChanged()
    {
        var store       = new EntityStore();
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);

        var entity1EventCount = 0;
        var onScriptChanged = (ScriptChangedArgs args)    => {
            switch (entity1EventCount++) {
                case 0:     AreEqual("entity: 1 - Add script: 'script1' [*TestScript1]", args.ToString()); break;
                default:    Fail("unexpected"); break;
            }
        };
        entity1.OnScriptChanged += onScriptChanged; 

        entity1.AddScript(new TestScript1());
        entity2.AddScript(new TestScript2());
        
        entity1.OnScriptChanged -= onScriptChanged; 
        
        entity1.AddScript(new TestScript1());
        
        AreEqual(1, entity1EventCount);
    }
    
    [Test]
    public static void Test_Entity_Events_OnChildEntitiesChanged()
    {
        var store       = new EntityStore();
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        var child10     = store.CreateEntity(10);
        var child11     = store.CreateEntity(11);
        var child12     = store.CreateEntity(12);

        var entity1EventCount = 0;
        var onChildEntitiesChanged = (ChildEntitiesChangedArgs args)    => {
            switch (entity1EventCount++) {
                case 0:     AreEqual("entity: 1 - Add ChildIds[0] = 10", args.ToString()); break;
                default:    Fail("unexpected"); break;
            }
        };
        entity1.OnChildEntitiesChanged += onChildEntitiesChanged; 

        entity1.AddChild(child10);
        entity2.AddChild(child11);
        
        entity1.OnChildEntitiesChanged -= onChildEntitiesChanged; 
        
        entity1.AddChild(child12);
        
        AreEqual(1, entity1EventCount);
    }
}

#pragma warning restore CS0618 // Type or member is obsolete

