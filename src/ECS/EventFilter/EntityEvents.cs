// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Friflo.Engine.ECS.Utils;

// ReSharper disable InconsistentNaming
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


/// <summary>
/// The type of an entity change in <see cref="EntityEvent"/>. 
/// </summary>
public enum EntityEventAction : byte
{
    /// <summary> A component / tag was removed </summary>
    Removed = 0,
    /// <summary> A component / tag was added </summary>
    Added   = 1
}

/// <summary>
/// The information about a structural change recorded by the <see cref="EventRecorder"/>.
/// </summary>
public struct EntityEvent
{
    /// <summary>The id of the changed entity.</summary>
    public      int                 Id;         //  4
    
    /// <summary>The change type - add / remove - of a component / tag. </summary>
    public      EntityEventAction   Action;     //  1
    
    /// <summary> The index in <see cref="EntitySchema"/> properties <see cref="EntitySchema.Components"/> or <see cref="EntitySchema.Tags"/>. </summary>
    public      byte                TypeIndex;  //  1
    
    /// <summary> The kind - component / tag - of the structural change. </summary>
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
            UpdateEntityChanges();
        }
        return entityChanges.ContainsKey(entityId);
    }
    
    internal void UpdateEntityChanges()
    {
        var changes     = entityChanges;
        var eventSpan   = new ReadOnlySpan<EntityEvent>(events, entityChangesPos, eventCount - entityChangesPos);
        bool exists     = false;
        foreach (var ev in eventSpan) {
            ref var value = ref MapUtils.GetValueRefOrAddDefault(changes, ev.Id, ref exists);
            value.SetBit(ev.TypeIndex);
        }
        entityChangesPos = eventCount;
    }
    
    private string GetString() {
        return $"events: {eventCount}";
    }
}

