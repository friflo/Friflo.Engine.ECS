// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStore
{
#region add / remove script events - experimental
    private void ScriptChanged(object sender, ScriptChangedArgs args)
    {
        if (!intern.entityScriptChanged.TryGetValue(args.entityId, out var handlers)) {
            return;
        }
        foreach (var handler in handlers) {
            handler(args);
        }
    }
    
    internal static void AddScriptChangedHandler(EntityStore store, int entityId, Action<ScriptChangedArgs> handler)
    {
        if (AddEntityHandler(entityId, handler, ref store.intern.entityScriptChanged)) {
            store.intern.scriptAdded     += store.ScriptChanged;
            store.intern.scriptRemoved   += store.ScriptChanged;
        }
    }
    
    internal static void RemoveScriptChangedHandler(EntityStore store, int entityId, Action<ScriptChangedArgs> handler)
    {
        if (RemoveEntityHandler(entityId, handler, store.intern.entityScriptChanged)) {
            store.intern.scriptAdded     -= store.ScriptChanged;
            store.intern.scriptRemoved   -= store.ScriptChanged;
        }
    }
    #endregion



    
#region add / remove child entity events - experimental
    private void ChildEntitiesChanged(object sender, ChildEntitiesChangedArgs args)
    {
        if (!intern.entityChildEntitiesChanged.TryGetValue(args.parentId, out var handlers)) {
            return;
        }
        foreach (var handler in handlers) {
            handler(args);
        }
    }
    
    internal static void AddChildEntitiesChangedHandler   (EntityStore store, int entityId, Action<ChildEntitiesChangedArgs> handler)
    {
        if (AddEntityHandler(entityId, handler, ref store.intern.entityChildEntitiesChanged)) {
            store.intern.childEntitiesChanged     += store.ChildEntitiesChanged;
        }
    }
    
    internal static void RemoveChildEntitiesChangedHandler(EntityStore store, int entityId, Action<ChildEntitiesChangedArgs> handler)
    {
        if (RemoveEntityHandler(entityId, handler, store.intern.entityChildEntitiesChanged)) {
            store.intern.childEntitiesChanged     -= store.ChildEntitiesChanged;
        }
    }
    #endregion
}
