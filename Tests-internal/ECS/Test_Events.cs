using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_Events
{
    [Test]
    public static void Test_Events_EntityHandlers()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity();
        AreEqual("event types: 0, handlers: 0", entity.DebugEventHandlers.ToString());
        
        entity.OnComponentChanged     += _ => { };
        entity.OnComponentChanged     += _ => { };
        entity.OnTagsChanged          += _ => { };
        entity.OnScriptChanged        += _ => { };
        entity.OnChildEntitiesChanged += _ => { };
        entity.AddSignalHandler<MyEvent>(_ => { });
        
        var handlers = entity.DebugEventHandlers;
        AreEqual(5, handlers.TypeCount);
        AreEqual(6, handlers.HandlerCount);
        AreEqual(5, handlers.Array.Length);
        AreEqual("event types: 5, handlers: 6", entity.DebugEventHandlers.ToString());
        
        AreEqual(typeof(ComponentChanged),      handlers[0].Type);
        AreEqual(typeof(TagsChanged),           handlers[1].Type);
        AreEqual(typeof(ScriptChanged),         handlers[2].Type);
        AreEqual(typeof(ChildEntitiesChanged),  handlers[3].Type);
        AreEqual(typeof(MyEvent),               handlers[4].Type);
        
        AreEqual("ComponentChanged - Count: 2",       handlers[0].ToString());
        AreEqual("TagsChanged - Count: 1",            handlers[1].ToString());
        AreEqual("ScriptChanged - Count: 1",          handlers[2].ToString());
        AreEqual("ChildEntitiesChanged - Count: 1",   handlers[3].ToString());
        AreEqual("Signal: MyEvent - Count: 1",        handlers[4].ToString());
    }
    
    private struct MyEvent { }
}