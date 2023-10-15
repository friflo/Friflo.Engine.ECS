// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static Friflo.Fliox.Engine.ECS.StructUtils;
using static Friflo.Fliox.Engine.ECS.NodeFlags;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// Hard rule: this file/section MUST NOT use GameEntity instances

// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <remarks>
/// <i>Usage type:</i> <b>TinyNodes</b><br/>
/// This approach enables using the <see cref="EntityStore"/> without <see cref="GameEntity"/>'s.<br/>
/// The focus of the this usage type is performance.<br/>
/// The key is to reduce heap consumption and GC costs caused by <see cref="GameEntity"/> instances.<br/>
/// In this case entities are stored only as <see cref="EntityNode"/>'s without <see cref="GameEntity"/> instances
/// in the <see cref="EntityStore"/>.<br/>
/// <br/>
/// The downside of this approach are:<br/>
/// <list type="bullet">
///   <item>Entities can be created only programmatically but not within the editor which requires (managed) <see cref="GameEntity"/>'s.</item>
///   <item>The API to access / query / mutate <see cref="EntityNode"/>'s is less convenient.<br/>
///     It requires always two parameters - <see cref="EntityStore"/> + entity <c>id</c> - instead of a single <see cref="GameEntity"/> reference.
///   </item>
/// </list>
/// </remarks>
public sealed class TinyEntityStore : EntityStore
{
                    public             ReadOnlySpan<TinyEntity> Entities           => new (entities);
                    
    [Browse(Never)] private            TinyEntity[]             entities;          //  8 + all tiny entities

    public TinyEntityStore()
    {
        entities = Array.Empty<TinyEntity>();
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
        for (int n = curLength; n < length; n++) {
            entities[n] = new TinyEntity (n);
        }
    }
    
    public int CreateEntity() {
        var id      = sequenceId++;
        CreateEntity(id);
        return id;
    }
    
    public int CreateEntity(int id) {
        EnsureEntitiesLength(id + 1);
        
        ref var entity = ref entities[id];
        if (entity.Is(Created)) {
            return id;
        }
        nodeCount++;
        if (nodeMaxId < id) {
            nodeMaxId = id;
        }
        entity.flags      = Created;
        return id;
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage] // assert invariant
    private void AssertIdInTinyNodes(int id) {
        if (id < entities.Length) {
            return;
        }
        throw new InvalidOperationException("expect id < tinyNodes.length");
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
        var result          = AddComponent(entity.id, ref archetype, ref entity.compIndex, component);
        entity.archIndex    = (short)archetype.archIndex;
        return result;
    }
    
    public bool RemoveEntityComponent<T>(int id)
        where T : struct, IStructComponent
    {
        ref var entity      = ref entities[id];
        var archetype       = archetypes[entity.archIndex];
        var result          =  RemoveComponent<T>(entity.id, ref archetype, ref entity.compIndex);
        entity.archIndex    = (short)archetype.archIndex;
        return result;
    }
}
