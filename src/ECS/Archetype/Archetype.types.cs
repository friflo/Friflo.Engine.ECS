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

internal struct ArchetypeMemory
{
    internal const  int     MinCapacity = 512;
    /// <summary> 512, 1024, 2048, 4096, ... </summary>
    internal        int     capacity;
    /// <summary>  -1,  512, 1024, 2048, ... </summary>
    internal        int     shrinkThreshold;
}