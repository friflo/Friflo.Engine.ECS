// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Experimental POC
/// </summary>
[ExcludeFromCodeCoverage]
internal class EventRecorder
{
    private readonly    EntityEvents[]  componentAdded;
    private readonly    EntityEvents[]  componentRemoved;
    private readonly    EntityEvents[]  tagAdded;
    private readonly    EntityEvents[]  tagRemoved;
        
    public EventRecorder() {
        var schema          = EntityStoreBase.Static.EntitySchema;
        componentAdded      = CreateEntityEvents(schema.components.Length);
        componentRemoved    = CreateEntityEvents(schema.components.Length);
        tagAdded            = CreateEntityEvents(schema.tags.Length);
        tagRemoved          = CreateEntityEvents(schema.tags.Length);
    }
    
    internal void ObserveStore (EntityStore store)
    {
        store.OnComponentAdded      += OnComponentAdded;
        store.OnComponentRemoved    += OnComponentRemoved;
        store.OnTagsChanged         += OnTagsChanged;
    }
    
    private static EntityEvents[] CreateEntityEvents(int length)
    {
        var events = new EntityEvents[length];
        for (int n = 1; n < length; n++) {
            events[n] = new EntityEvents {
                entityIds = Array.Empty<int>()
            };
        }
        return events;
    }
    
    public void Reset() {
        
    }
    
    private void OnComponentAdded(ComponentChanged change)
    {
        ref var events  = ref componentAdded[change.StructIndex];
        int count       = events.entityIdCount; 
        if (count == events.entityIds.Length) {
            ArrayUtils.Resize(ref events.entityIds, Math.Max(4, 2 * count));
        }
        events.entityIdCount    = count + 1;
        events.entityIds[count] = change.EntityId;
    }
    
    private void OnComponentRemoved(ComponentChanged change)
    {
        ref var events  = ref componentRemoved[change.StructIndex];
        int count       = events.entityIdCount; 
        if (count == events.entityIds.Length) {
            ArrayUtils.Resize(ref events.entityIds, Math.Max(4, 2 * count));
        }
        events.entityIdCount    = count + 1;
        events.entityIds[count] = change.EntityId;
    }
    
    private void OnTagsChanged(TagsChanged change)
    {
        foreach (var tag in change.AddedTags)
        {
            ref var events  = ref tagAdded[tag.TagIndex];
            int count       = events.entityIdCount; 
            if (count == events.entityIds.Length) {
                ArrayUtils.Resize(ref events.entityIds, Math.Max(4, 2 * count));
            }
            events.entityIdCount    = count + 1;
            events.entityIds[count] = change.EntityId;
        }
        foreach (var tag in change.RemovedTags)
        {
            ref var events  = ref tagRemoved[tag.TagIndex];
            int count       = events.entityIdCount; 
            if (count == events.entityIds.Length) {
                ArrayUtils.Resize(ref events.entityIds, Math.Max(4, 2 * count));
            }
            events.entityIdCount    = count + 1;
            events.entityIds[count] = change.EntityId;
        }
    }
}

[ExcludeFromCodeCoverage]
internal struct EntityEvents
{
    internal    int[]           entityIds;      //  8   - never null
    internal    int             entityIdCount;  //  4
    private     HashSet<int>    entitySet;      //  8   - can be null
    private     int             entitySetPos;   //  4
    
    
    internal bool ContainsId(int id)
    {
        entitySet ??= new HashSet<int>(entityIdCount);
        if (entitySetPos < entityIdCount)
        {
            var count       = entityIdCount;
            entitySetPos    = count;
            for (int n = entitySetPos; n < count; n++) {
                entitySet.Add(entityIds[n]);
            }
        }
        return entitySet.Contains(id);
    }
}
