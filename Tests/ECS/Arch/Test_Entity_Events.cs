using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch;


public static class Test_Entity_Events
{
    [Test]
    public static void Test_Entity_Events_OnTagsChanged()
    {
        var store       = new EntityStore();
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        
        var entity1EventCount = 0;
        var tagsChanged1 = (TagsChanged args)    => {
            switch (entity1EventCount++) {
                case 0:     AreEqual("entity: 1 - event > Add Tags: [#TestTag]", args.ToString()); break;
                default:    Fail("unexpected"); break;
            }
        };
        var tagsChanged2 = (TagsChanged args)    => { };
        
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
        var onComponentChanged = (ComponentChanged args)    => {
            switch (entity1EventCount++) {
                case 0:     AreEqual("entity: 1 - event > Add Component: [Position]", args.ToString()); break;
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
        var onScriptChanged = (ScriptChanged args)    => {
            switch (entity1EventCount++) {
                case 0:     AreEqual("entity: 1 - event > Add Script: [*TestScript1]", args.ToString()); break;
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
        var onChildEntitiesChanged = (ChildEntitiesChanged args)    => {
            switch (entity1EventCount++) {
                case 0:     AreEqual("entity: 1 - event > Add Child[0] = 10", args.ToString()); break;
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
    
    /// <summary>
    /// Note: Add signal handlers with different event types in specified order<b/>
    /// to cover null items in <see cref="EntityStore.Intern.signalHandlers"/>
    /// </summary>
    [Test]
    public static void Test_Entity_Signals()
    {
        {
            // --- See: Note
            var store2  = new EntityStore();
            var entity2 = store2.CreateEntity();
            entity2.AddSignalHandler<MyEvent1>(_ => { });
            entity2.AddSignalHandler<MyEvent2>(_ => { });
            entity2.AddSignalHandler<MyEvent3>(_ => { });
        }
        var store   = new EntityStore();
        var entity  = store.CreateEntity(1);

        var entity1SignalCount = 0;
        var onMyEvent = (Signal<MyEvent2> signal)    => {
            switch (entity1SignalCount++) {
                case 0:
                    Mem.AreEqual(1,                                 signal.Entity.Id);
                    Mem.AreEqual(1,                                 signal.EntityId);
                    Mem.AreSame (store,                             signal.Store);
                    Mem.AreEqual(42,                                signal.Event.value);
                    Mem.AreEqual("entity: 1 - signal > MyEvent2",   signal.ToString());
                    break;
                case 1:
                    Mem.AreEqual(43,                                signal.Event.value);
                    break;
                default:
                    Fail("unexpected");
                    break;
            }
        };
        var onMyEvent2 = (Signal<MyEvent2> signal) => { };
        entity.AddSignalHandler(onMyEvent);
        entity.AddSignalHandler(onMyEvent2);
        
        entity.EmitSignal(new MyEvent2 { value = 42 });    // handler allocates memory for assertion
        var start = Mem.GetAllocatedBytes();
        entity.EmitSignal(new MyEvent2 { value = 43 });
        Mem.AssertNoAlloc(start);

        entity.RemoveSignalHandler(onMyEvent);
        entity.RemoveSignalHandler(onMyEvent2);
        entity.RemoveSignalHandler(onMyEvent); // remove already removed handler
        entity.EmitSignal(new MyEvent2 { value = 13 });
        
        AreEqual(2, entity1SignalCount);
        
        entity.EmitSignal(new MyEvent1()); // no signal handler added
        entity.EmitSignal(new MyEvent3()); // no signal handler added
        
        entity.AddSignalHandler<MyEvent1>(_ => { });
    }
}

internal struct MyEvent1 { }
internal struct MyEvent2 { internal int value; }
internal struct MyEvent3 { }

