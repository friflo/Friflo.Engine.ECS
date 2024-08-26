// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// A <see cref="RawEntityStore"/> enables using an entity store without using <see cref="Entity"/>'s.<br/>
/// </summary>
/// <remarks>
/// The focus of the this entity store implementation is performance.<br/>
/// The key is to minimize heap consumption required by <see cref="EntityNode"/>'s - 48 bytes<br/>
/// A <see cref="RawEntityStore"/> stores only an array of blittable <see cref="RawEntityNode"/>'s -
/// structs having no reference type fields.<br/>
/// <br/>
/// The downside of this approach are:<br/>
/// <list type="bullet">
///   <item>Entities can be created only programmatically but not within the editor which requires (managed) <see cref="Entity"/>'s.</item>
///   <item>The API to access / query / mutate <see cref="RawEntityNode"/>'s is less convenient.<br/>
///     It requires always two parameters - a <see cref="RawEntityStore"/> + entity <c>id</c> - instead of a single <see cref="Entity"/> reference.
///   </item>
/// </list>
/// </remarks>
public sealed class RawEntityStore : EntityStoreBase
{
    private         RawEntityNode[] entities;       //  8 + all raw entities
    private         int             sequenceId;     //  4               - incrementing id used for next new entity

    public RawEntityStore()
    {
        entities    = Array.Empty<RawEntityNode>();
        sequenceId  = Static.MinNodeId;
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
        ArrayUtils.Resize(ref entities, newLength);
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
        var id = NewId();
        CreateEntity(archetype, id);
        return id;
    }
    
    public int CreateEntity(int id)
    {
        CreateEntity(defaultArchetype, id);
        return id;
    }
    
    public int CreateEntity()
    {
        var id = NewId();
        CreateEntity(defaultArchetype, id);
        return id;
    }
    
    private void CreateEntity(Archetype archetype, int id)
    {
        EnsureEntitiesLength(id + 1);
        var index       = entityCount;
        entityCount     = index + 1;
        ref var node    = ref entities[id];
        node.compIndex  = Archetype.AddEntity(archetype, id);
        node.archIndex  = archetype.archIndex; // default archetype index
    }
    
    protected internal override void    UpdateEntityCompIndex(int id, int compIndex) {
        entities[id].compIndex = compIndex;
    }
    
    private int NewId() {
        return sequenceId++;
    }
    
    public bool DeleteEntity(int id)
    {
        ref var entity  = ref entities[id];
        var archIndex   = entity.archIndex;
        if (archIndex == 0) {
            return false;
        }
        var archetype   = archs[archIndex];
        Archetype.MoveLastComponentsTo(archetype, entity.compIndex, true);
        entity.archIndex = 0;
        entity.compIndex = 0;
        entityCount--;
        return true;
    }

    #endregion

#region components
    public int GetEntityComponentCount(int id) {
        return archs[entities[id].archIndex].componentCount;
    }
    
    public ref T GetEntityComponent<T>(int id)
        where T : struct, IComponent
    {
        ref var entity  = ref entities[id];
        var heap        = (StructHeap<T>)archs[entity.archIndex].heapMap[StructInfo<T>.Index];
        return ref heap.components[entity.compIndex];
    }
    
    [ExcludeFromCodeCoverage]
    public bool AddEntityComponent<T>(int id, in T component)
        where T : struct, IComponent
    {
        throw new NotImplementedException($"{nameof(RawEntityStore)} will be removed");
    }
    
    [ExcludeFromCodeCoverage]
    public bool RemoveEntityComponent<T>(int id)
        where T : struct, IComponent
    {
        throw new NotImplementedException($"{nameof(RawEntityStore)} will be removed");
    }
    #endregion
    
#region tags
    public  ref readonly Tags    GetEntityTags(int id) {
        return ref archs[entities[id].archIndex].tags;
    }

    public  bool AddEntityTags(int id, in Tags tags)
    {
        ref var entity  = ref entities[id];
        var archetype   = archs[entity.archIndex];
        return AddTags(this, tags, id, ref archetype, ref entity.compIndex, ref entity.archIndex);
    }
        
    public  bool RemoveEntityTags(int id, in Tags tags)
    {
        ref var entity  = ref entities[id];
        var archetype   = archs[entity.archIndex];
        return RemoveTags(this, tags, id, ref archetype, ref entity.compIndex, ref entity.archIndex);
    }
    #endregion
}
