// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Friflo.Engine.ECS.Utils;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
// ReSharper disable UseCollectionExpression
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


[ExcludeFromCodeCoverage]
internal sealed class EventFilter
{
#region properties
    public override string  ToString() => GetString();
    #endregion
    
#region fields
                    private             long            _lastEventCount;
                    private readonly    EventRecorder   _recorder;
    [Browse(Never)] private readonly    EntityStore     _store;
                    //
                    private             EventFilters    componentFilters;
                    private             EventFilters    tagFilters;
    
    [Browse(Never)] private readonly    EntityEvents    componentEvents;
    [Browse(Never)] private readonly    EntityEvents    tagEvents;
    #endregion
    
    
    internal EventFilter(EventRecorder recorder)
    {
        _recorder       = recorder;
        _store          = recorder.entityStore;
        componentEvents = recorder.componentEvents;
        tagEvents       = recorder.tagEvents;
    }
    
    public void ComponentAdded<T>()
        where T : struct, IComponent
    {
        AddFilter(ref componentFilters, StructHeap<T>.StructIndex, SchemaTypeKind.Component, EntityEventAction.Added);
    }
    
    public void ComponentRemoved<T>()
        where T : struct, IComponent
    {
        AddFilter(ref componentFilters, StructHeap<T>.StructIndex, SchemaTypeKind.Component, EntityEventAction.Removed);
    }
    
    public void TagAdded<T>()
        where T : struct, ITag
    {
        AddFilter(ref tagFilters, TagType<T>.TagIndex, SchemaTypeKind.Tag, EntityEventAction.Added);
    }
    
    public void TagRemoved<T>()
        where T : struct, ITag
    {
        AddFilter(ref tagFilters, TagType<T>.TagIndex, SchemaTypeKind.Tag, EntityEventAction.Removed);
    }
    
    private static void AddFilter(ref EventFilters filters, int typeIndex, SchemaTypeKind kind, EntityEventAction action)
    {
        if (filters.items == null || filters.count == filters.items.Length) {
            ArrayUtils.Resize(ref filters.items, Math.Max(4, 2 * filters.count));
        }
        ref var filter  = ref filters.items[filters.count++];
        filter.index    = typeIndex;
        filter.kind     = kind; 
        filter.action   = action;
    }
    
    private void InitFilter()
    {
        _lastEventCount = _recorder.allEventsCount;
        InitTypeFilter(componentEvents);
        InitTypeFilter(tagEvents);
    }
    
    private static void InitTypeFilter(EntityEvents events)
    {
        if (events.eventCount == events.entityChangesPos) {
            return;
        }
        events.entityChanges ??= new Dictionary<int, BitSet>();
        events.UpdateHashSet();
    }
    
    public bool Filter(int entityId)
    {
        if (_lastEventCount != _recorder.allEventsCount) {
            InitFilter();
        }
        if (componentFilters.items != null && ContainsComponentEvent(entityId)) return true;
        if (tagFilters      .items != null && ContainsTagEvent      (entityId)) return true;
        return false;
    }
    
    private bool ContainsComponentEvent(int entityId)
    {
        if (!componentEvents.entityChanges.TryGetValue(entityId, out var bitSet)) {
            return false;
        }
        var componentTypes  = _store.nodes[entityId].archetype.componentTypes;
        var filters         = componentFilters;
        for (int n = 0; n < filters.count; n++)
        {
            TypeFilter filter = filters.items[n];
            if (!bitSet.Has(filter.index)) {
                continue;
            }
            bool hasComponent   = componentTypes.bitSet.Has(filter.index);
            bool addedFilter    = filter.action == EntityEventAction.Added;
            if (hasComponent == addedFilter) {
                return true;
            }
        }
        return false;
    }
    
    private bool ContainsTagEvent(int entityId)
    {
        if (!tagEvents.entityChanges.TryGetValue(entityId, out var bitSet)) {
            return false;
        }
        var tags    = _store.nodes[entityId].archetype.tags;
        var filters = tagFilters;
        for (int n = 0; n < filters.count; n++)
        {
            var filter = filters.items[n];
            if (!bitSet.Has(filter.index)) {
                continue;
            }
            bool hasTag         = tags.bitSet.Has(filter.index);
            bool addedFilter    = filter.action == EntityEventAction.Added;
            if (hasTag == addedFilter) {
                return true;
            }
        }
        return false;
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();

        sb.Append("added: [");
        var start = sb.Length;
        componentFilters.AppendString(sb, EntityEventAction.Added);
        tagFilters.      AppendString(sb, EntityEventAction.Added);
        if (start != sb.Length) sb.Length -= 2;
        sb.Append(']');

        if (sb.Length > 0) sb.Append("  ");
        sb.Append("removed: [");
        start = sb.Length;
        componentFilters.AppendString(sb, EntityEventAction.Removed);
        tagFilters.      AppendString(sb, EntityEventAction.Removed);
        if (start != sb.Length) sb.Length -= 2;
        sb.Append(']');

        return sb.ToString();
    }
}