// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Fliox.Engine.ECS.EntityStore.Static;
    
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed partial class EntityStore
{
    public Archetype GetArchetype<T>()
        where T : struct
    {
        var hash = typeof(T).Handle();
        if (TryGetArchetype(hash, out var archetype)) {
            return archetype;
        }
        var config  = GetArchetypeConfig();
        archetype   = Archetype.Create<T>(config);
        AddArchetype(archetype);
        return archetype;
    }
    
    public Archetype GetArchetype<T1, T2>()
        where T1 : struct
        where T2 : struct
    {
        ReadOnlySpan<long> guids = stackalloc long[] {
            typeof(T1).Handle(), typeof(T2).Handle()
        };
        var hash = GetHash(guids);
        if (TryGetArchetype(hash, out var archetype)) {
            return archetype;
        }
        var heaps = new StructHeap[] {
            StructHeap<T1>.Create(DefaultCapacity, typeStore),
            StructHeap<T2>.Create(DefaultCapacity, typeStore)
        };
        var config  = GetArchetypeConfig();
        archetype   = Archetype.CreateWithHeaps(config, heaps);
        AddArchetype(archetype);
        return archetype;
    }
    
    public Archetype GetArchetype<T1, T2, T3>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        ReadOnlySpan<long> guids = stackalloc long[] {
            typeof(T1).Handle(), typeof(T2).Handle(), typeof(T3).Handle()
        };
        var hash = GetHash(guids);
        if (TryGetArchetype(hash, out var archetype)) {
            return archetype;
        }
        var heaps = new StructHeap[] {
            StructHeap<T1>.Create(DefaultCapacity, typeStore),
            StructHeap<T2>.Create(DefaultCapacity, typeStore),
            StructHeap<T3>.Create(DefaultCapacity, typeStore )
        };
        var config  = GetArchetypeConfig();
        archetype   = Archetype.CreateWithHeaps(config, heaps);
        AddArchetype(archetype);
        return archetype;
    }
    
    public Archetype GetArchetype<T1, T2, T3, T4>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        ReadOnlySpan<long> guids = stackalloc long[] {
            typeof(T1).Handle(), typeof(T2).Handle(), typeof(T3).Handle(), typeof(T4).Handle()
        };
        var hash = GetHash(guids);
        if (TryGetArchetype(hash, out var archetype)) {
            return archetype;
        }
        var heaps = new StructHeap[] {
            StructHeap<T1>.Create(DefaultCapacity, typeStore),
            StructHeap<T2>.Create(DefaultCapacity, typeStore),
            StructHeap<T3>.Create(DefaultCapacity, typeStore),
            StructHeap<T4>.Create(DefaultCapacity, typeStore)
        };
        var config  = GetArchetypeConfig();
        archetype   = Archetype.CreateWithHeaps(config, heaps);
        AddArchetype(archetype);
        return archetype;
    }
    
    private ArchetypeConfig GetArchetypeConfig()
    {
        return new ArchetypeConfig {
            store           = this,
            archetypeIndex  = archetypesCount,
            maxStructIndex  = maxStructIndex,
            capacity        = DefaultCapacity,
            typeStore       = typeStore
        };
    }
}
