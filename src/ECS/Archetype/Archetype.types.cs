// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

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
    internal readonly   EntitySchema    schema;
    
    internal ArchetypeConfig(EntityStoreBase store, int archetypeIndex, EntitySchema schema)
    {
        this.store          = store;
        this.archetypeIndex = archetypeIndex;
        this.schema         = schema;
    }
}

internal struct ArchetypeMemory
{
    /// <summary> 512, 1024, 2048, 4096, ... </summary>
    internal        int     capacity;
    /// <summary>  -1,  512, 1024, 2048, ... </summary>
    internal        int     shrinkThreshold;
}

internal static class ArchetypeExtensions
{
    internal static ReadOnlySpan<StructHeap>   Heaps       (this Archetype archetype)  => archetype.structHeaps;
     
    /*
    internal static                    int     ChunkCount  (this Archetype archetype)  // entity count: 0: 0   1:0 ... 512:0     513:1 ...
                                               => archetype.entityCount / ChunkSize;

    internal static                    int     ChunkEnd    (this Archetype archetype)  // entity count: 0:-1   1:0 ... 512:0     513:1 ...
                                               => (archetype.entityCount + ChunkSize - 1) / ChunkSize - 1;

    internal static                    int     ChunkRest(this Archetype archetype)  => archetype.entityCount % ChunkSize;
    */

    /*
    /// <summary> return remaining length in range [1, <see cref="ChunkSize"/>] </summary>
    internal static                    int     ChunkRest   (this Archetype archetype)  // entity count: 0:0    1:1 ... 512:512   513:1   514:2 ...
                                               => (archetype.entityCount - 1) % ChunkSize + 1; */
}