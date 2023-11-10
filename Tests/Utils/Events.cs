using System;
using Friflo.Fliox.Engine.ECS;

namespace Tests.Utils;

internal class Events
{
    internal int seq;
    
    internal static Events SetHandler(GameEntityStore store, Action<ChildNodesChangedArgs> action)
    {
        var events = new Events();
        store.ChildNodesChangedHandler = (object _, in ChildNodesChangedArgs args) => {
            events.seq++;
            action(args);
        };
        return events;
    }
    
    internal static Events SetHandlerSeq(GameEntityStore store, Action<ChildNodesChangedArgs, int> action)
    {
        var events = new Events();
        store.ChildNodesChangedHandler = (object _, in ChildNodesChangedArgs args) => {
            action(args, events.seq++);
        };
        return events;
    }
}