// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable UseCollectionExpression
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
internal sealed class EventFilter
{
    private             long            lastEventCount;
    //
    private             int[]           addedComponents;
    private             int             addedComponentsCount;
    //
    private             int[]           removedComponents;
    private             int             removedComponentsCount;
    //
    private             int[]           addedTags;
    private             int             addedTagsCount;
    //
    private             int[]           removedTags;
    private             int             removedTagsCount;
    
    private readonly    EventRecorder   recorder;
    private readonly    EntityEvents[]  componentAdded;
    private readonly    EntityEvents[]  componentRemoved;
    private readonly    EntityEvents[]  tagAdded;
    private readonly    EntityEvents[]  tagRemoved;
    
    
    internal EventFilter(EventRecorder recorder)
    {
        this.recorder       = recorder;
        componentAdded      = recorder.componentAdded;
        componentRemoved    = recorder.componentRemoved;
        tagAdded            = recorder.tagAdded;
        tagRemoved          = recorder.tagRemoved;
    }
    
    public void ComponentAdded<T>()
        where T : struct, IComponent
    {
        AddFilter(ref addedComponents, ref addedComponentsCount, StructHeap<T>.StructIndex);
    }
    
    public void ComponentRemoved<T>()
        where T : struct, IComponent
    {
        AddFilter(ref removedComponents, ref removedComponentsCount, StructHeap<T>.StructIndex);
    }
    
    public void TagAdded<T>()
        where T : struct, ITag
    {
        AddFilter(ref addedTags, ref addedTagsCount, TagType<T>.TagIndex);
    }
    
    public void TagRemoved<T>()
        where T : struct, ITag
    {
        AddFilter(ref removedTags, ref removedTagsCount, TagType<T>.TagIndex);
    }
    
    private static void AddFilter(ref int[] filter, ref int count, int typeIndex)
    {
        if (filter == null || count == filter.Length) {
            ArrayUtils.Resize(ref filter, Math.Max(4, 2 * count));
        }
        filter[count++] = typeIndex;
    }
    
    
    private void InitFilter()
    {
        lastEventCount = recorder.allEventsCount;
        InitTypeFilter(addedComponents,   addedComponentsCount,   componentAdded);
        InitTypeFilter(removedComponents, removedComponentsCount, componentRemoved);
        InitTypeFilter(addedTags,         addedTagsCount,         tagAdded);
        InitTypeFilter(removedTags,       removedTagsCount,       tagRemoved);
    }
    
    private static void InitTypeFilter(int[] indexes, int count, EntityEvents[] events)
    {
        for (int n = 0; n < count; n++)
        {
            var entityEvents = events[indexes[n]];
            entityEvents.entitySet ??= new HashSet<int>();
            entityEvents.UpdateHashSet();
        }
    }
    
    public bool Filter(int entityId)
    {
        if (lastEventCount != recorder.allEventsCount) {
            InitFilter();
        }
        if (addedComponents   != null && Contains(addedComponents,   addedComponentsCount,   componentAdded,   entityId))  return true;
        if (removedComponents != null && Contains(removedComponents, removedComponentsCount, componentRemoved, entityId))  return true;
        if (addedTags         != null && Contains(addedTags,         addedTagsCount,         tagAdded,         entityId))  return true;
        if (removedTags       != null && Contains(removedTags,       removedTagsCount,       tagRemoved,       entityId))  return true;
        return false;
    }
    
    private static bool Contains(int[] indexes, int count, EntityEvents[] events, int entityId)
    {
        for (int n = 0; n < count; n++)
        {
            if (events[indexes[n]].entitySet.Contains(entityId)) {
                return true;
            }
        }
        return false;
    }
}