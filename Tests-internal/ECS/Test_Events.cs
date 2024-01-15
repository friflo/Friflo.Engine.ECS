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
        AreEqual("EventHandler[0]", entity.DebugEventHandlers.ToString());
        
        entity.OnComponentChanged     += _ => { };
        entity.OnTagsChanged          += _ => { };
        entity.OnScriptChanged        += _ => { };
        entity.OnChildEntitiesChanged += _ => { };
        entity.AddSignalHandler<MyEvent>(_ => { });
        
        var handlers = entity.DebugEventHandlers;
        AreEqual(5, handlers.Count);
        AreEqual(5, handlers.array.Length);
        AreEqual("EventHandler[5]", entity.DebugEventHandlers.ToString());
        
        AreEqual(typeof(ComponentChanged),      handlers[0].Type);
        AreEqual(typeof(TagsChanged),           handlers[1].Type);
        AreEqual(typeof(ScriptChanged),         handlers[2].Type);
        AreEqual(typeof(ChildEntitiesChanged),  handlers[3].Type);
        AreEqual(typeof(MyEvent),               handlers[4].Type);
        
        AreEqual("OnComponentChanged - Count: 1",       handlers[0].ToString());
        AreEqual("OnTagsChanged - Count: 1",            handlers[1].ToString());
        AreEqual("OnScriptChanged - Count: 1",          handlers[2].ToString());
        AreEqual("OnChildEntitiesChanged - Count: 1",   handlers[3].ToString());
        AreEqual("Signal: MyEvent - Count: 1",          handlers[4].ToString());
    }
    
    private struct MyEvent { }
}