// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static Friflo.Fliox.Engine.ECS.StructUtils;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// Hard rule: this file/section MUST NOT use GameEntity instances

// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="RawEntityStore"/> enables using an <see cref="EntityStore"/> without <see cref="GameEntity"/>'s.<br/>
/// <br/>
/// The focus of the this <see cref="EntityStore"/> implementation is performance.<br/>
/// The key is to eliminate heap consumption and GC costs caused by <see cref="GameEntity"/> instances.<br/>
/// A <see cref="RawEntityStore"/> stores only an array of blittable <see cref="RawEntity"/>'s -
/// structs having no reference type fields<br/>
/// </summary>
/// <remarks>
/// The downside of this approach are:<br/>
/// <list type="bullet">
///   <item>Entities can be created only programmatically but not within the editor which requires (managed) <see cref="GameEntity"/>'s.</item>
///   <item>The API to access / query / mutate <see cref="RawEntity"/>'s is less convenient.<br/>
///     It requires always two parameters - a <see cref="RawEntityStore"/> + entity <c>id</c> - instead of a single <see cref="GameEntity"/> reference.
///   </item>
/// </list>
/// </remarks>
public sealed class RawEntityStore : EntityStore
{
    [Browse(Never)] private            RawEntity[]             entities;          //  8 + all raw entities

    public RawEntityStore()
    {
        entities = Array.Empty<RawEntity>();
    }
        
    public void EnsureEntityCapacity(int length) {
        EnsureEntitiesLength(sequenceId + length);
    }

    private void EnsureEntitiesLength(int length)
    {
        var curLength = entities.Length;
        if (length <= curLength) {
            return;
        }
        var newLength = Math.Max(length, 2 * entities.Length);
        Utils.Resize(ref entities, newLength);
        // Note: Assigning each new entity a default value ensures they get filled into the memory cache.
        //       As a result subsequent calls to CreateEntity() are faster in perf test 
        for (int n = curLength; n < length; n++) {
            entities[n] = default;
        }
    }
    
    public int CreateEntity() {
        var id      = sequenceId++;
        CreateEntity(id);
        return id;
    }
    
    public int CreateEntity(int id) {
        EnsureEntitiesLength(id + 1);
        nodeCount++;
        if (nodeMaxId < id) {
            nodeMaxId = id;
        }
        return id;
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage] // assert invariant
    private void AssertIdInRawEntities(int id) {
        if (id < entities.Length) {
            return;
        }
        throw new InvalidOperationException("expect id < entities.length");
    }
    
    public int GetEntityComponentCount(int id) {
        return archetypes[entities[id].archIndex].componentCount;
    }
    
    public ref T GetEntityComponentValue<T>(int id)
        where T : struct, IStructComponent
    {
        ref var entity  = ref entities[id];
        var heap        = (StructHeap<T>)archetypes[entity.archIndex].heapMap[StructHeap<T>.StructIndex];
        return ref heap.chunks[entity.compIndex / ChunkSize].components[entity.compIndex % ChunkSize];
    }
    
    public bool AddEntityComponent<T>(int id, in T component)
        where T : struct, IStructComponent
    {
        ref var entity      = ref entities[id];
        var archetype       = archetypes[entity.archIndex];
        var result          = AddComponent(id, ref archetype, ref entity.compIndex, component);
        entity.archIndex    = archetype.archIndex;
        return result;
    }
    
    public bool RemoveEntityComponent<T>(int id)
        where T : struct, IStructComponent
    {
        ref var entity      = ref entities[id];
        var archetype       = archetypes[entity.archIndex];
        var result          = RemoveComponent<T>(id, ref archetype, ref entity.compIndex);
        entity.archIndex    = archetype.archIndex;
        return result;
    }
}
