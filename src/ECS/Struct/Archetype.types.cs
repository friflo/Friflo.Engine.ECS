// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace

using Friflo.Json.Fliox.Mapper;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal struct StandardComponents
{
    internal    StructHeap<Position>    position;
    internal    StructHeap<Rotation>    rotation;
    internal    StructHeap<Scale3>      scale3;
    internal    StructHeap<EntityName>  name;
}

internal readonly struct ArchetypeConfig
{
    internal readonly   EntityStore store;
    internal readonly   int         archetypeIndex;
    internal readonly   int         maxStructIndex;
    internal readonly   int         capacity;
    internal readonly   TypeStore   typeStore;
    
    internal ArchetypeConfig(
        EntityStore store,
        int         archetypeIndex,
        int         maxStructIndex,
        int         capacity,
        TypeStore   typeStore)
    {
        this.store          = store;
        this.archetypeIndex = archetypeIndex;
        this.maxStructIndex = maxStructIndex;
        this.capacity       = capacity;
        this.typeStore      = typeStore;
    }
}