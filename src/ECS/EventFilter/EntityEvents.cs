// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
internal struct EntityEvents
{
#region properties
    internal        ReadOnlySpan<EntityEvent>   Events   => new (events, 0, eventCount);

    public override string                      ToString()  => GetString();

    #endregion
    
#region fields
    internal            EntityEvent[]                       events;         //  8   - never null
    internal            int                                 eventCount;     //  4
    internal            Dictionary<int, EntityEventAction>  entityMap;      //  8   - can be null. Created / updated on demand.
    internal            int                                 entitySetPos;   //  4
    private  readonly   SchemaType                          type;           //  8
    #endregion
    
    internal EntityEvents(SchemaType type) {
        this.type = type;
    }
    
    internal bool ContainsId(int entityId)
    {
        var idCount = eventCount;
        var map     = entityMap ??= new Dictionary<int, EntityEventAction>(idCount);
        if (entitySetPos < idCount) {
            UpdateHashSet();
        }
        return map.ContainsKey(entityId);
    }
    
    internal void UpdateHashSet()
    {
        var set = entityMap;
        var eventSpan = new ReadOnlySpan<EntityEvent>(events, entitySetPos, eventCount - entitySetPos);
        foreach (var ev in eventSpan) {
            set[ev.id] = ev.action;
        }
        entitySetPos = eventCount;
    }
    
    private string GetString() {
        if (type == null) {
            return "";
        }
        string marker = type.Kind == SchemaTypeKind.Component ? "" : "#";
        return $"[{marker}{type.Name}] events: {eventCount}";
    }
}

