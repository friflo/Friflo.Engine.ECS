using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.ECS {

public static class Test_EventFilter
{
    [Test]
    public static void Test_EventFilter_filter_ToString()
    {
        var componentEvents = new EntityEvents();
        AreEqual("events: 0", componentEvents.ToString());
        
        var store           = new EntityStore(PidType.UsePidAsId);
        var recorder        = store.EventRecorder;
        
        var positionAdded   = new EventFilter(recorder);
        var positionRemoved = new EventFilter(recorder);
        var tagAdded        = new EventFilter(recorder);
        var tagRemoved      = new EventFilter(recorder);
        
        positionAdded.  ComponentAdded  <Position>();
        positionRemoved.ComponentRemoved<Position>();
        tagAdded.       TagAdded        <TestTag>();
        tagRemoved.     TagRemoved      <TestTag>();
        
        AreEqual("added: [Position]  removed: []",  positionAdded.ToString());
        AreEqual("added: []  removed: [Position]",  positionRemoved.ToString());
        AreEqual("added: [#TestTag]  removed: []",  tagAdded.ToString());
        AreEqual("added: []  removed: [#TestTag]",  tagRemoved.ToString());
        
        AreEqual("added: [Position]  removed: []",  positionAdded.componentFilters.ToString());
        AreEqual("added: []  removed: [Position]",  positionRemoved.componentFilters.ToString());
        AreEqual("added: [#TestTag]  removed: []",  tagAdded.tagFilters.ToString());
        AreEqual("added: []  removed: [#TestTag]",  tagRemoved.tagFilters.ToString());
        
        AreEqual("[]",                              positionAdded.tagFilters.ToString());
        
        var filter = new EventFilter(recorder);
        filter.ComponentAdded   <Position>();
        filter.ComponentRemoved <Position>();
        filter.TagAdded         <TestTag>();
        filter.TagRemoved       <TestTag>();
        
        AreEqual("added: [Position, #TestTag]  removed: [Position, #TestTag]", filter.ToString());
    }
}

}
