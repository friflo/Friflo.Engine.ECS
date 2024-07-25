// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Threading;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable InconsistentNaming
// ReSharper disable UseCollectionExpression
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Used to filter structural changes made to an entity like added / removed components / tags using <see cref="HasEvent"/>.<br/>
/// The <see cref="EntityStore.EventRecorder"/> must be enabled to get add / remove events.<br/>
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#eventfilter">Example.</a>
/// </summary>
public sealed class EventFilter
{
#region properties
    public override string  ToString() => GetString();
    #endregion
    
#region fields
                    private  readonly   EventRecorder   _recorder;
    [Browse(Never)] private  readonly   EntityStore     _store;
                    //
                    internal            EventFilters    componentFilters;
                    internal            EventFilters    tagFilters;
    
    [Browse(Never)] private  readonly   EntityEvents    componentEvents;
    [Browse(Never)] private  readonly   EntityEvents    tagEvents;
    #endregion
    
    /// <summary>
    /// Create and event filter for the passed <see cref="EventRecorder"/>. 
    /// </summary>
    public EventFilter(EventRecorder recorder)
    {
        _recorder       = recorder;
        _store          = recorder.entityStore;
        componentEvents = recorder.componentEvents;
        tagEvents       = recorder.tagEvents;
    }
    
    /// <summary> Enable filtering add component events of the given <see cref="IComponent"/> type <typeparamref name="T"/>.</summary>
    public void ComponentAdded<T>()
        where T : struct, IComponent
    {
        AddFilter(ref componentFilters, StructInfo<T>.Index, SchemaTypeKind.Component, EntityEventAction.Added);
    }
    
    /// <summary> Enable filtering remove component events of the given <see cref="IComponent"/> type <typeparamref name="T"/>.</summary>
    public void ComponentRemoved<T>()
        where T : struct, IComponent
    {
        AddFilter(ref componentFilters, StructInfo<T>.Index, SchemaTypeKind.Component, EntityEventAction.Removed);
    }
    
    /// <summary> Enable filtering add tag events of the given <see cref="ITag"/> type <typeparamref name="T"/>.</summary>
    public void TagAdded<T>()
        where T : struct, ITag
    {
        AddFilter(ref tagFilters, TagInfo<T>.Index, SchemaTypeKind.Tag, EntityEventAction.Added);
    }
    
    /// <summary> Enable filtering remove tag events of the given <see cref="ITag"/> type <typeparamref name="T"/>.</summary>
    public void TagRemoved<T>()
        where T : struct, ITag
    {
        AddFilter(ref tagFilters, TagInfo<T>.Index, SchemaTypeKind.Tag, EntityEventAction.Removed);
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
    
    /// <remarks>
    /// The <see cref="componentEvents"/> lock ensures that the <see cref="EntityEvents.entityChanges"/> Dictionary
    /// is updated from a single thread only.<br/>
    /// See remarks as <see cref="HasEvent"/>
    /// </remarks>
    private void UpdateFilter()
    {
        lock (componentEvents)
        {
            UpdateEntityChanges(componentEvents);
            UpdateEntityChanges(tagEvents);
            Interlocked.Exchange(ref _recorder.allEventsCountMapUpdate, _recorder.allEventsCount);
        }
    }
    
    private static void UpdateEntityChanges(EntityEvents events)
    {
        if (events.eventCount == events.entityChangesPos) {
            return;
        }
        events.UpdateEntityChanges();
    }
    
    /// <summary>
    /// Returns true if a component or tag was added / removed to / from the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    /// <remarks>
    /// Therefore <see cref="EntityStore.EventRecorder"/> needs to be enabled and<br/> 
    /// the component / tag (add / remove) events of interest need to be added to the <see cref="EventFilter"/>.<br/>
    /// <br/>
    /// <b>Note</b>: <see cref="HasEvent"/> can be called from any thread.<br/>
    /// No structural changes like adding / removing components/tags must not be executed at the same time by another thread.
    /// </remarks>
    public bool HasEvent(int entityId)
    {
        var recorder = _recorder;
        var eventCount = Interlocked.Read(ref recorder.allEventsCountMapUpdate);
        if (eventCount != recorder.allEventsCount) {
            UpdateFilter();
        }
        if (componentFilters.count > 0 && ContainsComponentEvent(entityId)) return true;
        if (tagFilters      .count > 0 && ContainsTagEvent      (entityId)) return true;
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
            TypeFilter filter = filters.items[n];
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