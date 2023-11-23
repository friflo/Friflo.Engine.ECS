// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Fliox.Engine.ECS.StructInfo;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// Hard rule: this file MUST NOT access Entity's

// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="RawEntityStore"/> enables using an entity store without <see cref="Entity"/>'s.<br/>
/// <br/>
/// The focus of the this entity store implementation is performance.<br/>
/// The key is to eliminate heap consumption and GC costs caused by <see cref="Entity"/> instances.<br/>
/// A <see cref="RawEntityStore"/> stores only an array of blittable <see cref="RawEntity"/>'s -
/// structs having no reference type fields<br/>
/// </summary>
/// <remarks>
/// The downside of this approach are:<br/>
/// <list type="bullet">
///   <item>Entities can be created only programmatically but not within the editor which requires (managed) <see cref="Entity"/>'s.</item>
///   <item>The API to access / query / mutate <see cref="RawEntity"/>'s is less convenient.<br/>
///     It requires always two parameters - a <see cref="RawEntityStore"/> + entity <c>id</c> - instead of a single <see cref="Entity"/> reference.
///   </item>
/// </list>
/// </remarks>
public sealed class RawEntityStore : EntityStoreBase
{
    private            RawEntity[]             entities;          //  8 + all raw entities

    public RawEntityStore()
    {
        entities = Array.Empty<RawEntity>();
    }

#region entity create
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
    
    public Archetype GetEntityArchetype(int id) {
        return archs[entities[id].archIndex];
    }
    
    /// <summary>
    /// Creates a new entity with the components and tags of the given <paramref name="archetype"/>
    /// </summary>
    public int CreateEntity(Archetype archetype)
    {
        if (this != archetype.store) {
            throw InvalidStoreException(nameof(archetype));
        }
        var id              = CreateEntity();
        ref var entity      = ref entities[id]; 
        entity.archIndex    = archetype.archIndex;
        entity.compIndex    = archetype.AddEntity(id);
        return id;
    }
    
    public int CreateEntity() {
        var id      = sequenceId++;
        CreateEntity(id);
        return id;
    }
    
    public int CreateEntity(int id) {
        EnsureEntitiesLength(id + 1);
        nodesCount++;
        if (nodesMaxId < id) {
            nodesMaxId = id;
        }
        return id;
    }
    
    protected internal override void UpdateEntityCompIndex(int id, int compIndex) {
        entities[id].compIndex = compIndex;
    }
    
    public void DeleteEntity(int id)
    {
        ref var entity  = ref entities[id]; 
        var archetype   = archs[entity.archIndex];
        if (archetype == defaultArchetype) {
            return;
        }
        archetype.MoveLastComponentsTo(entity.compIndex);
        entity.archIndex = 0;
        entity.compIndex = 0;
        nodesCount--;
    }

    #endregion

#region components
    public int GetEntityComponentCount(int id) {
        return archs[entities[id].archIndex].structCount;
    }
    
    public ref T GetEntityComponent<T>(int id)
        where T : struct, IComponent
    {
        ref var entity  = ref entities[id];
        var heap        = (StructHeap<T>)archs[entity.archIndex].heapMap[StructHeap<T>.StructIndex];
        return ref heap.chunks[entity.compIndex / ChunkSize].components[entity.compIndex % ChunkSize];
    }
    
    public bool AddEntityComponent<T>(int id, in T component)
        where T : struct, IComponent
    {
        ref var entity      = ref entities[id];
        var archetype       = archs[entity.archIndex];
        var result          = AddComponent(id, ref archetype, ref entity.compIndex, component);
        entity.archIndex    = archetype.archIndex;
        return result;
    }
    
    public bool RemoveEntityComponent<T>(int id)
        where T : struct, IComponent
    {
        ref var entity      = ref entities[id];
        var archetype       = archs[entity.archIndex];
        var result          = RemoveComponent(id, ref archetype, ref entity.compIndex, StructHeap<T>.StructIndex);
        entity.archIndex    = archetype.archIndex;
        return result;
    }
    #endregion
    
#region tags
    public  ref readonly Tags    GetEntityTags(int id) {
        return ref archs[entities[id].archIndex].tags;
    }

    public  bool AddEntityTags(int id, in Tags tags)
    {
        ref var entity      = ref entities[id];
        var archetype       = archs[entity.archIndex];
        var result          = AddTags(tags, id, ref archetype, ref entity.compIndex);
        entity.archIndex    = archetype.archIndex; 
        return result;
    }
        
    public  bool RemoveEntityTags(int id, in Tags tags)
    {
        ref var entity      = ref entities[id];
        var archetype       = archs[entity.archIndex];
        var result          = RemoveTags(tags, id, ref archetype, ref entity.compIndex);
        entity.archIndex    = archetype.archIndex;
        return result;
    }
    #endregion
}
