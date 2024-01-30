// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Friflo.Engine.ECS.Utils;

// ReSharper disable InconsistentNaming
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


public enum EntityEventAction : byte
{
    Removed = 0,
    Added   = 1
}


public struct EntityEvent {
    public      int                 Id;         //  4
    public      EntityEventAction   Action;     //  1
    public      byte                TypeIndex;  //  1
    public      SchemaTypeKind      Kind;       //  1   - used only for ToString()

    public override string          ToString() => GetString();
    
    private string GetString()
    {
        var schema = EntityStoreBase.Static.EntitySchema;
        switch (Kind) {
            case SchemaTypeKind.Component:
                return $"id: {Id} - {Action} [{schema.components[TypeIndex].Name}]";
            case SchemaTypeKind.Tag:
                return $"id: {Id} - {Action} [#{schema.tags[TypeIndex].Name}]";
        }
        throw new InvalidOperationException("unexpected kind");
    }
}


internal sealed class EntityEvents
{
#region properties
    internal        ReadOnlySpan<EntityEvent>   Events   => new (events, 0, eventCount);

    public override string                      ToString()  => GetString();

    #endregion
    
#region fields
    internal            EntityEvent[]           events;             //  8   - never null
    internal            int                     eventCount;         //  4   - number of recorded events
    internal readonly   Dictionary<int, BitSet> entityChanges;      //  8   - never null
    internal            int                     entityChangesPos;   //  4   - last event in events[] added to entityChanges map
    #endregion
    
    internal EntityEvents() {
        events          = Array.Empty<EntityEvent>();
        entityChanges   = new Dictionary<int, BitSet>();
    }
    
    [ExcludeFromCodeCoverage]
    internal bool ContainsId(int entityId)
    {
        if (entityChangesPos < eventCount) {
            UpdateHashSet();
        }
        return entityChanges.ContainsKey(entityId);
    }
    
    internal void UpdateHashSet()
    {
        var changes     = entityChanges;
        var eventSpan   = new ReadOnlySpan<EntityEvent>(events, entityChangesPos, eventCount - entityChangesPos);
        foreach (var ev in eventSpan) {
            ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(changes, ev.Id, out _);
            value.SetBit(ev.TypeIndex);
        }
        entityChangesPos = eventCount;
    }
    
    private string GetString() {
        return $"events: {eventCount}";
    }
}

