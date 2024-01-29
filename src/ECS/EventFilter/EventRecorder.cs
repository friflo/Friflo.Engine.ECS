// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable CoVariantArrayConversion
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Experimental POC
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class EventRecorder
{
#region properties
    public          long AllEventsCount => allEventsCount;
    public          bool Enabled        { get => enabled; set => SetEnabled(value); }

    public override string ToString()   => GetString();

    #endregion
    
#region fields
    [Browse(Never)] internal            long            allEventsCount;
    [Browse(Never)] private             bool            enabled;
    [Browse(Never)] private  readonly   EntityStore     entityStore;
                    internal readonly   EntityEvents[]  componentAdded;
                    internal readonly   EntityEvents[]  componentRemoved;
                    internal readonly   EntityEvents[]  tagAdded;
                    internal readonly   EntityEvents[]  tagRemoved;
    #endregion
    
#region general methods
    public EventRecorder(EntityStore store)
    {
        entityStore         = store;
        var schema          = EntityStoreBase.Static.EntitySchema;
        componentAdded      = CreateEntityEvents(schema.components);
        componentRemoved    = CreateEntityEvents(schema.components);
        tagAdded            = CreateEntityEvents(schema.tags);
        tagRemoved          = CreateEntityEvents(schema.tags);
    }
    
    public ReadOnlySpan<int> ComponentAddedEntities  <T>() where T : struct, IComponent => componentAdded  [StructHeap<T>.StructIndex].EntityIds;
    public ReadOnlySpan<int> ComponentRemovedEntities<T>() where T : struct, IComponent => componentRemoved[StructHeap<T>.StructIndex].EntityIds;
    
    public ReadOnlySpan<int> TagAddedEntities  <T>() where T : struct, ITag => tagAdded  [TagType<T>.TagIndex].EntityIds;
    public ReadOnlySpan<int> TagRemovedEntities<T>() where T : struct, ITag => tagRemoved[TagType<T>.TagIndex].EntityIds;
    
    public void Reset()
    {
        ResetEvents(componentAdded);
        ResetEvents(componentRemoved);
        ResetEvents(tagAdded);
        ResetEvents(tagRemoved);
    }
    
    private static void ResetEvents(EntityEvents[] eventsArray)
    {
        // todo could use bit mask to reset events only if necessary
        foreach (ref var events in eventsArray.AsSpan()) {
            events.entitySet?.Clear();
            events.entityIdCount = 0;
            events.entitySetPos = 0;
        }
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
    
    private static EntityEvents[] CreateEntityEvents(SchemaType[] types)
    {
        var length      = types.Length;
        var eventsArray = new EntityEvents[length];
        for (int n = 1; n < length; n++) {
            var events = new EntityEvents(types[n]) { entityIds = Array.Empty<int>() };
            eventsArray[n] = events;
        }
        return eventsArray;
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
        AddEvent(componentAdded, change.StructIndex, change.EntityId);
    }
    
    private void OnComponentRemoved(ComponentChanged change)
    {
        allEventsCount++;
        AddEvent(componentRemoved, change.StructIndex, change.EntityId);
    }
    
    private void OnTagsChanged(TagsChanged change)
    {
        var addedCount = change.AddedTags.Count;
        if (addedCount > 0) {
            allEventsCount += addedCount;
            foreach (var tag in change.AddedTags) {
                AddEvent(tagAdded, tag.TagIndex, change.EntityId);
            }
        }
        var removedCount = change.RemovedTags.Count;
        if (removedCount > 0) {
            allEventsCount += removedCount;
            foreach (var tag in change.RemovedTags) {
                AddEvent(tagRemoved, tag.TagIndex, change.EntityId);
            }
        }
    }
    
    private static void AddEvent(EntityEvents[] typeEvents, int typeIndex, int entityId)
    {
        ref var events  = ref typeEvents[typeIndex];
        int count       = events.entityIdCount; 
        if (count == events.entityIds.Length) {
            ArrayUtils.Resize(ref events.entityIds, Math.Max(4, 2 * count));
        }
        events.entityIdCount    = count + 1;
        events.entityIds[count] = entityId;
    }
    #endregion
}
