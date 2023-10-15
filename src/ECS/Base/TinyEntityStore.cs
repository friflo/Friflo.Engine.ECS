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
    [Browse(Never)] private            TinyNode[]              tinyNodes;          //  8 + all tiny nodes

    public TinyEntityStore()
    {
        tinyNodes = Array.Empty<TinyNode>();
    }
        
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
