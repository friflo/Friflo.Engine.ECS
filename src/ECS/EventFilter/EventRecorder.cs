// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable InlineTemporaryVariable
// ReSharper disable CoVariantArrayConversion
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Experimental POC
/// </summary>
public sealed class EventRecorder
{
#region properties
    public                  long            AllEventsCount  => allEventsCount;
    public                  bool            Enabled         { get => enabled; set => SetEnabled(value); }
    public     ReadOnlySpan<EntityEvent>    ComponentEvents => componentEvents.Events;
    public     ReadOnlySpan<EntityEvent>    TagEvents       => tagEvents.      Events;

    public override string ToString()   => GetString();
    #endregion
    
#region fields
    [Browse(Never)] internal            long            allEventsCount;
    /// <remarks>
    /// If <see cref="allEventsCount"/> != <see cref="allEventCountMapUpdate"/>
    /// the <see cref="EntityEvents.entityChanges"/> must be updated when using an <see cref="EventFilter"/>.
    /// </remarks>
                    internal            long            allEventCountMapUpdate;
    [Browse(Never)] private             bool            enabled;
                    internal readonly   EntityStore     entityStore;
                    internal readonly   EntityEvents    componentEvents;
                    internal readonly   EntityEvents    tagEvents;
    #endregion
    
#region general methods
    internal EventRecorder(EntityStore store)
    {
        entityStore         = store;
        componentEvents     = new EntityEvents();
        tagEvents           = new EntityEvents();
    }
    
    public void Reset()
    {
        ResetEvents(componentEvents);
        ResetEvents(tagEvents);
    }
    
    private static void ResetEvents(EntityEvents events)
    {
        events.entityChanges.Clear();
        events.eventCount       = 0;
        events.entityChangesPos = 0;
    }
    
    private void SetEnabled(bool enabled)
    {
        if (this.enabled == enabled) {
            return;
        }
        this.enabled    = enabled;
        var store       = entityStore;
        if (enabled) {
            store.OnComponentAdded      += OnComponentAdded;
            store.OnComponentRemoved    += OnComponentRemoved;
            store.OnTagsChanged         += OnTagsChanged;
            return;
        }
        store.OnComponentAdded      -= OnComponentAdded;
        store.OnComponentRemoved    -= OnComponentRemoved;
        store.OnTagsChanged         -= OnTagsChanged;
    }
    
    private string GetString() {
        if (enabled) {
            return $"All events: {AllEventsCount}";
        }
        return "disabled";
    }
    #endregion
    
#region event handler
    private void OnComponentAdded(ComponentChanged change)
    {
        allEventsCount++;
        ref var ev      = ref AddEvent(componentEvents);
        ev.Id           = change.EntityId;
        ev.Action       = EntityEventAction.Added;
        ev.TypeIndex    = (byte)change.StructIndex;
        ev.Kind         = SchemaTypeKind.Component;
    }
    
    private void OnComponentRemoved(ComponentChanged change)
    {
        allEventsCount++;
        ref var ev      = ref AddEvent(componentEvents);
        ev.Id           = change.EntityId;
        ev.Action       = EntityEventAction.Removed;
        ev.TypeIndex    = (byte)change.StructIndex;
        ev.Kind         = SchemaTypeKind.Component;
    }
    
    private void OnTagsChanged(TagsChanged change)
    {
        var addedTags   = change.AddedTags;
        var addedCount  = addedTags.Count;
        if (addedCount > 0) {
            allEventsCount += addedCount;
            foreach (var tag in addedTags)
            {
                ref var ev      = ref AddEvent(tagEvents);
                ev.Id           = change.EntityId;
                ev.Action       = EntityEventAction.Added;
                ev.TypeIndex    = (byte)tag.TagIndex;
                ev.Kind         = SchemaTypeKind.Tag;
            }
        }
        var removedTags     = change.RemovedTags;
        var removedCount    = removedTags.Count;
        if (removedCount > 0) {
            allEventsCount += removedCount;
            foreach (var tag in removedTags)
            {
                ref var ev      = ref AddEvent(tagEvents);
                ev.Id           = change.EntityId;
                ev.Action       = EntityEventAction.Removed;
                ev.TypeIndex    = (byte)tag.TagIndex;
                ev.Kind         = SchemaTypeKind.Tag;
            }
        }
    }
    
    private static ref EntityEvent AddEvent(EntityEvents events)
    {
        int count = events.eventCount; 
        if (count == events.events.Length) {
            ArrayUtils.Resize(ref events.events, Math.Max(4, 2 * count));
        }
        events.eventCount   = count + 1;
        return ref events.events[count];
    }
    #endregion
}
