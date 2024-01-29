// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
                    //
                    private             EventFilters    componentsAdded     = new EventFilters { action = EventFilterAction.Added };
                    private             EventFilters    componentsRemoved   = new EventFilters { action = EventFilterAction.Removed };
                    private             EventFilters    tagsAdded           = new EventFilters { action = EventFilterAction.Added };
                    private             EventFilters    tagsRemoved         = new EventFilters { action = EventFilterAction.Removed };
    
    [Browse(Never)] private readonly    EntityEvents[]  eventsComponentAdded;
    [Browse(Never)] private readonly    EntityEvents[]  eventsComponentRemoved;
    [Browse(Never)] private readonly    EntityEvents[]  eventsTagAdded;
    [Browse(Never)] private readonly    EntityEvents[]  eventsTagRemoved;
    #endregion
    
    
    internal EventFilter(EventRecorder recorder)
    {
        this._recorder           = recorder;
        eventsComponentAdded    = recorder.componentAdded;
        eventsComponentRemoved  = recorder.componentRemoved;
        eventsTagAdded          = recorder.tagAdded;
        eventsTagRemoved        = recorder.tagRemoved;
    }
    
    public void ComponentAdded<T>()
        where T : struct, IComponent
    {
        AddFilter(ref componentsAdded, StructHeap<T>.StructIndex, SchemaTypeKind.Component);
    }
    
    public void ComponentRemoved<T>()
        where T : struct, IComponent
    {
        AddFilter(ref componentsRemoved, StructHeap<T>.StructIndex, SchemaTypeKind.Component);
    }
    
    public void TagAdded<T>()
        where T : struct, ITag
    {
        AddFilter(ref tagsAdded, TagType<T>.TagIndex, SchemaTypeKind.Tag);
    }
    
    public void TagRemoved<T>()
        where T : struct, ITag
    {
        AddFilter(ref tagsRemoved, TagType<T>.TagIndex, SchemaTypeKind.Tag);
    }
    
    private static void AddFilter(ref EventFilters filters, int typeIndex, SchemaTypeKind kind)
    {
        if (filters.items == null || filters.count == filters.items.Length) {
            ArrayUtils.Resize(ref filters.items, Math.Max(4, 2 * filters.count));
        }
        ref var filter  = ref filters.items[filters.count++];
        filter.index    = typeIndex;
        filter.kind     = kind; 
    }
    
    
    private void InitFilter()
    {
        _lastEventCount = _recorder.allEventsCount;
        InitTypeFilter(componentsAdded,   eventsComponentAdded);
        InitTypeFilter(componentsRemoved, eventsComponentRemoved);
        InitTypeFilter(tagsAdded,         eventsTagAdded);
        InitTypeFilter(tagsRemoved,       eventsTagRemoved);
    }
    
    private static void InitTypeFilter(in EventFilters filters, EntityEvents[] events)
    {
        for (int n = 0; n < filters.count; n++)
        {
            ref var entityEvents = ref events[filters.items[n].index];
            if (entityEvents.entityIdCount == entityEvents.entitySetPos) {
                continue;
            }
            entityEvents.entitySet ??= new HashSet<int>();
            entityEvents.UpdateHashSet();
        }
    }
    
    public bool Filter(int entityId)
    {
        if (_lastEventCount != _recorder.allEventsCount) {
            InitFilter();
        }
        if (componentsAdded  .items != null && Contains(componentsAdded,   eventsComponentAdded,   entityId)) return true;
        if (componentsRemoved.items != null && Contains(componentsRemoved, eventsComponentRemoved, entityId)) return true;
        if (tagsAdded        .items != null && Contains(tagsAdded,         eventsTagAdded,         entityId)) return true;
        if (tagsRemoved      .items != null && Contains(tagsRemoved,       eventsTagRemoved,       entityId)) return true;
        return false;
    }
    
    private static bool Contains(EventFilters filters, EntityEvents[] events, int entityId)
    {
        for (int n = 0; n < filters.count; n++)
        {
            if (events[filters.items[n].index].entitySet.Contains(entityId)) {
                return true;
            }
        }
        return false;
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        if (componentsAdded.count > 0 || tagsAdded.count > 0) {
            sb.Append("added: [");
            componentsAdded.AppendString(sb);
            tagsAdded.      AppendString(sb);
            sb.Length -= 2;
            sb.Append(']');
        }
        if (componentsRemoved.count > 0 || tagsRemoved.count > 0) {
            if (sb.Length > 0) sb.Append(",  ");
            sb.Append("removed: [");
            componentsRemoved.AppendString(sb);
            tagsRemoved.      AppendString(sb);
            sb.Length -= 2;
            sb.Append(']');
        }
        return sb.ToString();
    }
}