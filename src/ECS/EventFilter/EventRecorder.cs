// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
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
/// Used to record events of structural changes like add/remove component/tag.<br/>
/// The recorder is required to filter these events using an <see cref="EventFilter"/>.
/// </summary>
public sealed class EventRecorder
{
#region properties
    /// <summary> Return the number of all recorded events.<br/> Calling <see cref="ClearEvents"/> does not affect the counter.</summary>
    public                  long            AllEventsCount  => allEventsCount;
    
    /// <summary> Record component / tag events if true.<br/> It is required when using an <see cref="EventFilter"/>.</summary>
    public                  bool            Enabled         { get => enabled; set => SetEnabled(value); }
    
    /// <summary>The list of all recorded component events.</summary>
    public     ReadOnlySpan<EntityEvent>    ComponentEvents => componentEvents.Events;
    
    /// <summary>The list of all recorded tag events.</summary>
    public     ReadOnlySpan<EntityEvent>    TagEvents       => tagEvents.      Events;

    public override string ToString()   => GetString();
    #endregion
    
#region fields
    [Browse(Never)] internal            long            allEventsCount;
    /// <remarks>
    /// If <see cref="allEventsCount"/> != <see cref="allEventsCountMapUpdate"/>
    /// the <see cref="EntityEvents.entityChanges"/> must be updated when using an <see cref="EventFilter"/>.
    /// </remarks>
                    internal            long            allEventsCountMapUpdate;
    [Browse(Never)] private             bool            enabled;
                    internal readonly   EntityStore     entityStore;
    [Browse(Never)] internal readonly   EntityEvents    componentEvents;
    [Browse(Never)] internal readonly   EntityEvents    tagEvents;
    #endregion
    
#region general methods
    internal EventRecorder(EntityStore store)
    {
        entityStore         = store;
        componentEvents     = new EntityEvents();
        tagEvents           = new EntityEvents();
    }
    
    /// <summary>
    /// Clear all  <see cref="ComponentEvents"/> and <see cref="TagEvents"/>.
    /// </summary>
    public void ClearEvents()
    {
        Clear(componentEvents);
        Clear(tagEvents);
    }
    
    private static void Clear(EntityEvents events)
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
