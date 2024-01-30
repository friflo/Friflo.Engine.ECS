// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Friflo.Engine.ECS.Utils;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal sealed class EntityEvents
{
#region properties
    internal        ReadOnlySpan<EntityEvent>   Events   => new (events, 0, eventCount);

    public override string                      ToString()  => GetString();

    #endregion
    
#region fields
    internal            EntityEvent[]           events;             //  8   - never null
    internal            int                     eventCount;         //  4
    internal readonly   Dictionary<int, BitSet> entityChanges;      //  8   - can be null. Created / updated on demand.
    internal            int                     entityChangesPos;   //  4
    #endregion
    
    internal EntityEvents() {
        events          = Array.Empty<EntityEvent>();
        entityChanges   = new Dictionary<int, BitSet>();
    }
    
    [ExcludeFromCodeCoverage]
    internal bool ContainsId(int entityId)
    {
        var idCount = eventCount;
        if (entityChangesPos < idCount) {
            UpdateHashSet();
        }
        return entityChanges.ContainsKey(entityId);
    }
    
    internal void UpdateHashSet()
    {
        var changes     = entityChanges;
        var eventSpan   = new ReadOnlySpan<EntityEvent>(events, entityChangesPos, eventCount - entityChangesPos);
        foreach (var ev in eventSpan) {
            ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(changes, ev.id, out _);
            value.SetBit(ev.typeIndex);
        }
        entityChangesPos = eventCount;
    }
    
    private string GetString() {
        return $"events: {eventCount}";
    }
}

