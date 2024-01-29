// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
internal class EventFilter
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private readonly    int[]           addedComponents;
    private readonly    int[]           removedComponents;
    private readonly    int[]           addedTags;
    private readonly    int[]           removedTags;
    
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
    
    public bool Filter(int entityId)
    {
        if (Contains(addedComponents,   componentAdded,     entityId))  return true;
        if (Contains(removedComponents, componentRemoved,   entityId))  return true;
        if (Contains(addedTags,         tagAdded,           entityId))  return true;
        if (Contains(removedTags,       tagRemoved,         entityId))  return true;
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