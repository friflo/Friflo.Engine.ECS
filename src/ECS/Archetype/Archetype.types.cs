// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal struct StandardComponents
{
    internal    StructHeap<Position>    position;   // 8
    internal    StructHeap<Rotation>    rotation;   // 8
    internal    StructHeap<Scale3>      scale3;     // 8
    internal    StructHeap<EntityName>  name;       // 8
}

internal readonly struct ArchetypeConfig
{
    internal readonly   EntityStoreBase store;
    internal readonly   int             archetypeIndex;
    internal readonly   int             maxStructIndex;
    
    internal ArchetypeConfig(EntityStoreBase store, int archetypeIndex)
    {
        this.store          = store;
        this.archetypeIndex = archetypeIndex;
        maxStructIndex      = EntityStoreBase.Static.EntitySchema.maxStructIndex;
    }
}

internal struct ChunkMemory
{
    /// <summary>
    /// The number of <see cref="StructChunk{T}"/>'s (chunks with <see cref="StructChunk{T}"/>.<see cref="StructChunk{T}.components"/> != null)<br/>
    /// stored in the <see cref="StructHeap{T}"/>.<see cref="StructHeap{T}.chunks"/> array.
    /// </summary>
    internal    int     chunkCount;         //  4       - 1 <= chunkCount <= chunkLength
    /// <summary>
    /// The current array Length of <see cref="StructHeap{T}"/>.<see cref="StructHeap{T}.chunks"/>. Values: 1, 2, 4, 8, 16, ...
    /// </summary>
    internal    int     chunkLength;        //  4       - 1, 2, 4, 8, 16, ...
    // --- fields derived from chunkCount & chunkLength
    /// <summary>The sum of allocated components in <see cref="StructHeap{T}"/>.<see cref="StructHeap{T}.chunks"/>.</summary>
    internal    int     capacity;           //  4       - multiple of chunk size
    internal    int     shrinkThreshold;    //  4       - multiple of chunk size
}