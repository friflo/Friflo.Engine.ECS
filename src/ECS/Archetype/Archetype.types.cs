// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal struct StandardComponents
{
    internal    StructHeap<Position>    position;   // 8
    internal    StructHeap<Rotation>    rotation;   // 8
    internal    StructHeap<Scale3>      scale3;     // 8
    internal    StructHeap<EntityName>  name;       // 8
}

internal readonly struct ArchetypeConfig
{
    internal readonly   EntityStore store;
    internal readonly   int         archetypeIndex;
    internal readonly   int         chunkSize;
    internal readonly   int         maxStructIndex;
    
    internal ArchetypeConfig(
        EntityStore store,
        int         archetypeIndex,
        int         chunkSize)
    {
        this.store          = store;
        this.archetypeIndex = archetypeIndex;
        this.chunkSize      = chunkSize;
        maxStructIndex      = EntityStore.Static.ComponentSchema.maxStructIndex;
    }
}

internal struct ChunkMemory
{
    /// <summary>Multiple of <see cref="StructUtils.ChunkSize"/> struct components / entities</summary>
    internal    int     capacity;           //  4       - multiple of chunk size entities
    internal    int     shrinkThreshold;    //  4       - multiple of chunk size entities
}