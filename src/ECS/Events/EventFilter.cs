// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
internal sealed class EventFilter
{
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
    
    private readonly    EntityEvents[]  componentAdded;
    private readonly    EntityEvents[]  componentRemoved;
    private readonly    EntityEvents[]  tagAdded;
    private readonly    EntityEvents[]  tagRemoved;
    
    
    internal EventFilter(EventRecorder recorder)
    {
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
    
    public bool Filter(int entityId)
    {
        if (addedComponents   != null && Contains(addedComponents,   componentAdded,     entityId))  return true;
        if (removedComponents != null && Contains(removedComponents, componentRemoved,   entityId))  return true;
        if (addedTags         != null && Contains(addedTags,         tagAdded,           entityId))  return true;
        if (removedTags       != null && Contains(removedTags,       tagRemoved,         entityId))  return true;
        return false;
    }
    
    private static bool Contains(int[] indexes, EntityEvents[] events, int entityId)
    {
        foreach (var index in indexes)
        {
            if (events[index].entitySet.Contains(entityId)) {
                return true;
            }
        }
        return false;
    }
}