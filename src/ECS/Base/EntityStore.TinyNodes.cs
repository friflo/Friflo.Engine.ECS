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
        var pid = GeneratePid(id);
        CreateTinyNodeInternal(id, pid);
        return id;
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage] // assert invariant
    private void AssertIdInTinyNodes(int id) {
        if (id < tinyNodes.Length) {
            return;
        }
        throw new InvalidOperationException("expect id < tinyNodes.length");
    }
    
    /// <summary>expect <see cref="tinyNodes"/> Length > id</summary> 
    private void CreateTinyNodeInternal(int id, long pid)
    {
        AssertIdInTinyNodes(id);
        ref var node = ref tinyNodes[id];
        if (node.Is(Created)) {
            AssertPid(node.pid, pid);
            return;
        }
        nodeCount++;
        if (nodeMaxId < id) {
            nodeMaxId = id;
        }
        AssertPid0(node.pid, pid);
        node.pid        = pid;
        // node.parentId   = Static.NoParentId;     // Is not set. A previous parent node has .parentId already set.
        node.flags      = Created;
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
