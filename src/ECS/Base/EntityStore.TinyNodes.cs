// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static Friflo.Fliox.Engine.ECS.StructUtils;
using static Friflo.Fliox.Engine.ECS.NodeFlags;

// Hard rule: this file/section MUST NOT use GameEntity instances

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed partial class EntityStore
{
    public void EnsureTinyNodeCapacity(int length) {
        EnsureTinyNodesLength(sequenceId + length);
    }

    private void EnsureTinyNodesLength(int length)
    {
        var curLength = tinyNodes.Length;
        if (length <= curLength) {
            return;
        }
        var newLength = Math.Max(length, 2 * tinyNodes.Length);
        Utils.Resize(ref tinyNodes, newLength);
        for (int n = curLength; n < length; n++) {
            tinyNodes[n] = new TinyNode (n);
        }
    }
    
    public int CreateTinyNode() {
        var id      = sequenceId++;
        EnsureTinyNodesLength(id + 1);
        
        ref var node = ref tinyNodes[id];
        if (node.Is(Created)) {
            return id;
        }
        nodeCount++;
        if (nodeMaxId < id) {
            nodeMaxId = id;
        }
        node.flags      = Created;
        return id;
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage] // assert invariant
    private void AssertIdInTinyNodes(int id) {
        if (id < tinyNodes.Length) {
            return;
        }
        throw new InvalidOperationException("expect id < tinyNodes.length");
    }
    
    public int GetEntityComponentCount(int id) {
        return archetypes[tinyNodes[id].archIndex].componentCount;
    }
    
    public ref T GetEntityComponentValue<T>(int id)
        where T : struct, IStructComponent
    {
        ref var node    = ref tinyNodes[id];
        var heap        = (StructHeap<T>)archetypes[node.archIndex].heapMap[StructHeap<T>.StructIndex];
        return ref heap.chunks[node.compIndex / ChunkSize].components[node.compIndex % ChunkSize];
    }
    
    public bool AddEntityComponent<T>(int id, in T component)
        where T : struct, IStructComponent
    {
        ref var node    = ref tinyNodes[id];
        var archetype   = archetypes[node.archIndex];
        var result      = AddComponent(node.id, ref archetype, ref node.compIndex, component);
        node.archIndex  = (short)archetype.archIndex;
        return result;
    }
    
    public bool RemoveEntityComponent<T>(int id)
        where T : struct, IStructComponent
    {
        ref var node    = ref tinyNodes[id];
        var archetype   = archetypes[node.archIndex];
        var result      =  RemoveComponent<T>(node.id, ref archetype, ref node.compIndex);
        node.archIndex  = (short)archetype.archIndex;
        return result;
    }
}
