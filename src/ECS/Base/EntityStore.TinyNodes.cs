// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using static Friflo.Fliox.Engine.ECS.StructUtils;

// Hard rule: this file/section MUST NOT use GameEntity instances

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed partial class EntityStore
{
    public int CreateTineNode() {
        var id      = sequenceId++;
        EnsureNodesLength(id + 1);
        var pid = GeneratePid(id);
        CreateEntityNode(id, pid, NodeBinding.None);
        return id;
    }
    
    public int GetEntityComponentCount(int id) {
        return archetypes[nodes[id].archIndex].componentCount;
    }
    
    public ref T GetEntityComponentValue<T>(int id)
        where T : struct, IStructComponent
    {
        ref var node    = ref nodes[id];
        var heap        = (StructHeap<T>)archetypes[node.archIndex].heapMap[StructHeap<T>.StructIndex];
        return ref heap.chunks[node.compIndex / ChunkSize].components[node.compIndex % ChunkSize];
    }
    
    public bool AddEntityComponent<T>(int id, in T component)
        where T : struct, IStructComponent
    {
        ref var node    = ref nodes[id];
        var archetype   = archetypes[node.archIndex];
        var compIndex   = node.compIndex; 
        return AddComponent(node.id, ref archetype, ref compIndex, component);
    }
    
    
    public bool RemoveEntityComponent<T>(int id)
        where T : struct, IStructComponent
    {
        ref var node    = ref nodes[id];
        var archetype   = archetypes[node.archIndex];
        var compIndex   = node.compIndex; 
        return RemoveComponent<T>(node.id, ref archetype, ref compIndex);
    }
}
