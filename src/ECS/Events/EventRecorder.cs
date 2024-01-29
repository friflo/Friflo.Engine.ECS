// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
    public              long AllEventsCount => allEventsCount;
    #endregion
    
#region fields
    internal            long            allEventsCount;
    internal readonly   EntityEvents[]  componentAdded;
    internal readonly   EntityEvents[]  componentRemoved;
    internal readonly   EntityEvents[]  tagAdded;
    internal readonly   EntityEvents[]  tagRemoved;
    #endregion
    
#region general methods
    public EventRecorder() {
        var schema          = EntityStoreBase.Static.EntitySchema;
        componentAdded      = CreateEntityEvents(schema.components.Length);
        componentRemoved    = CreateEntityEvents(schema.components.Length);
        tagAdded            = CreateEntityEvents(schema.tags.Length);
        tagRemoved          = CreateEntityEvents(schema.tags.Length);
    }
    
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
    
    internal void AddEventHandlers (EntityStore store)
    {
        store.OnComponentAdded      += OnComponentAdded;
        store.OnComponentRemoved    += OnComponentRemoved;
        store.OnTagsChanged         += OnTagsChanged;
    }
    
    internal void RemoveEventHandlers (EntityStore store)
    {
        store.OnComponentAdded      -= OnComponentAdded;
        store.OnComponentRemoved    -= OnComponentRemoved;
        store.OnTagsChanged         -= OnTagsChanged;
    }
    
    private static EntityEvents[] CreateEntityEvents(int length)
    {
        var events = new EntityEvents[length];
        for (int n = 1; n < length; n++) {
            events[n].entityIds = Array.Empty<int>();
        }
        return events;
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

[ExcludeFromCodeCoverage]
internal struct EntityEvents
{
#region properties
    internal    ReadOnlySpan<int>   EntityIds => new (entityIds, 0, entityIdCount);
    #endregion
    
#region fields
    internal    int[]           entityIds;      //  8   - never null
    internal    int             entityIdCount;  //  4
    internal    HashSet<int>    entitySet;      //  8   - can be null. Created / updated on demand.
    internal    int             entitySetPos;   //  4
    #endregion
    
    
    internal bool ContainsId(int entityId)
    {
        var idCount = entityIdCount;
        var set     = entitySet ??= new HashSet<int>(idCount);
        if (entitySetPos < idCount) {
            UpdateHashSet();
        }
        return set.Contains(entityId);
    }
    
    internal void UpdateHashSet()
    {
        var set = entitySet;
        var ids = new ReadOnlySpan<int>(entityIds, entitySetPos, entityIdCount - entitySetPos);
        foreach (var id in ids) {
            set.Add(id);
        }
        entitySetPos = entityIdCount;
    }
}
