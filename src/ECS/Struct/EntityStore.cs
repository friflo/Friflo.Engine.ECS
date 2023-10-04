// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using static Friflo.Fliox.Engine.ECS.EntityStore.Static;
    
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed partial class EntityStore
{
#region get archetype
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
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle();
        if (TryGetArchetype(hash, out var archetype)) {
            return archetype;
        }
        var config  = GetArchetypeConfig();
        var heaps = new StructHeap[] {
            StructHeap<T1>.Create(config),
            StructHeap<T2>.Create(config)
        };
        archetype   = Archetype.CreateWithHeaps(config, heaps);
        AddArchetype(archetype);
        return archetype;
    }
    
    public Archetype GetArchetype<T1, T2, T3>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle() ^
                   typeof(T3).Handle();
        if (TryGetArchetype(hash, out var archetype)) {
            return archetype;
        }
        var config  = GetArchetypeConfig();
        var heaps = new StructHeap[] {
            StructHeap<T1>.Create(config),
            StructHeap<T2>.Create(config),
            StructHeap<T3>.Create(config)
        };
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
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle() ^
                   typeof(T3).Handle() ^
                   typeof(T4).Handle();
        if (TryGetArchetype(hash, out var archetype)) {
            return archetype;
        }
        var config  = GetArchetypeConfig();
        var heaps = new StructHeap[] {
            StructHeap<T1>.Create(config),
            StructHeap<T2>.Create(config),
            StructHeap<T3>.Create(config),
            StructHeap<T4>.Create(config)
        };
        archetype   = Archetype.CreateWithHeaps(config, heaps);
        AddArchetype(archetype);
        return archetype;
    }
    
    internal ArchetypeConfig GetArchetypeConfig() {
        return new ArchetypeConfig (this, archetypesCount, maxStructIndex, DefaultCapacity, typeStore);
    }
    #endregion
    
    // -------------------------------------- archetype query --------------------------------------
#region archetype query
    private readonly Dictionary<long, ArchetypeQuery> queries = new Dictionary<long, ArchetypeQuery>();
    
    public ArchetypeQuery Query<T> ()
        where T : struct
    {
        var hash = typeof(T).Handle(); // could StructHeap<T>.StructIndex to improve performance 
        if (queries.TryGetValue(hash, out var query)) {
            return query;
        }
        ReadOnlySpan<int> structIndices = stackalloc int[] {
            StructHeap<T>.StructIndex
        };
        query = new ArchetypeQuery(this, structIndices);
        queries.Add(hash, query);
        return query;
    }
    
    public ArchetypeQuery Query<T1, T2> ()
        where T1 : struct
        where T2 : struct
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle();
        if (queries.TryGetValue(hash, out var query)) {
            return query;
        }
        ReadOnlySpan<int> structIndices = stackalloc int[] {
            StructHeap<T1>.StructIndex,
            StructHeap<T2>.StructIndex
        };
        query = new ArchetypeQuery(this, structIndices);
        queries.Add(hash, query);
        return query;
    }
    
    public ArchetypeQuery Query<T1, T2, T3> ()
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle() ^
                   typeof(T3).Handle();
        if (queries.TryGetValue(hash, out var query)) {
            return query;
        }
        ReadOnlySpan<int> structIndices = stackalloc int[] {
            StructHeap<T1>.StructIndex,
            StructHeap<T2>.StructIndex,
            StructHeap<T3>.StructIndex
        };
        query = new ArchetypeQuery(this, structIndices);
        queries.Add(hash, query);
        return query;
    }
    
    public ArchetypeQuery Query<T1, T2, T3, T4> ()
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle() ^
                   typeof(T3).Handle() ^
                   typeof(T4).Handle();
        if (queries.TryGetValue(hash, out var query)) {
            return query;
        }
        ReadOnlySpan<int> structIndices = stackalloc int[] {
            StructHeap<T1>.StructIndex,
            StructHeap<T2>.StructIndex,
            StructHeap<T3>.StructIndex,
            StructHeap<T4>.StructIndex
        };
        query = new ArchetypeQuery(this, structIndices);
        queries.Add(hash, query);
        return query;
    }
    #endregion
}
