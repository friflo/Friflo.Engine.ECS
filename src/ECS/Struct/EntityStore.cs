// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using static Friflo.Fliox.Engine.ECS.EntityStore.Static;

// ReSharper disable ArrangeTrailingCommaInMultilineLists
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
        var config      = GetArchetypeConfig();
        var compTypes   = Static.ComponentTypes;
        var types       = new ComponentType[] {
            compTypes.GetStructType(StructHeap<T>.StructIndex, config.maxStructIndex, typeof(T))
        };
        archetype = Archetype.CreateWithStructTypes(config, types);
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
        var config      = GetArchetypeConfig();
        var compTypes   = Static.ComponentTypes;
        var types       = new ComponentType[] {
            compTypes.GetStructType(StructHeap<T1>.StructIndex, config.maxStructIndex, typeof(T1)),
            compTypes.GetStructType(StructHeap<T2>.StructIndex, config.maxStructIndex, typeof(T2)),
        };
        archetype = Archetype.CreateWithStructTypes(config, types);
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
        var config      = GetArchetypeConfig();
        var compTypes   = Static.ComponentTypes;
        var types       = new ComponentType[] {
            compTypes.GetStructType(StructHeap<T1>.StructIndex, config.maxStructIndex, typeof(T1)),
            compTypes.GetStructType(StructHeap<T2>.StructIndex, config.maxStructIndex, typeof(T2)),
            compTypes.GetStructType(StructHeap<T3>.StructIndex, config.maxStructIndex, typeof(T3)),
        };
        archetype = Archetype.CreateWithStructTypes(config, types);
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
        var config      = GetArchetypeConfig();
        var compTypes   = Static.ComponentTypes;
        var types       = new ComponentType[] {
            compTypes.GetStructType(StructHeap<T1>.StructIndex, config.maxStructIndex, typeof(T1)),
            compTypes.GetStructType(StructHeap<T2>.StructIndex, config.maxStructIndex, typeof(T2)),
            compTypes.GetStructType(StructHeap<T3>.StructIndex, config.maxStructIndex, typeof(T3)),
            compTypes.GetStructType(StructHeap<T4>.StructIndex, config.maxStructIndex, typeof(T4)),
        };
        archetype = Archetype.CreateWithStructTypes(config, types);
        AddArchetype(archetype);
        return archetype;
    }
    
    internal ArchetypeConfig GetArchetypeConfig() {
        return new ArchetypeConfig (this, archetypesCount, maxStructIndex, DefaultCapacity, typeStore);
    }
    #endregion
    
    // -------------------------------------- archetype query --------------------------------------
#region archetype query

    // ----------------------------------- query via generic Signature -----------------------------------
    private readonly ArchetypeQuery[] queriesSig = new ArchetypeQuery[100];
    
    public ArchetypeQuery<T> Query<T> (Signature<T> signature)
        where T : struct
    {
        var query = queriesSig[signature.index];
        if (query != null) {
            return (ArchetypeQuery<T>)query;
        }
        var newQuery = new ArchetypeQuery<T>(this, signature); 
        queriesSig[signature.index] = newQuery;
        return newQuery;
    }
    
    public ArchetypeQuery<T1, T2> Query<T1, T2> (Signature<T1, T2> signature)
        where T1: struct
        where T2: struct
    {
        var query = queriesSig[signature.index];
        if (query != null) {
            return (ArchetypeQuery<T1, T2>)query;
        }
        var newQuery = new ArchetypeQuery<T1, T2>(this, signature); 
        queriesSig[signature.index] = newQuery;
        return newQuery;
    }
    
    public ArchetypeQuery<T1, T2, T3> Query<T1, T2, T3> (Signature<T1, T2, T3> signature)
        where T1: struct
        where T2: struct
        where T3: struct
    {
        var query = queriesSig[signature.index];
        if (query != null) {
            return (ArchetypeQuery<T1, T2, T3>)query;
        }
        var newQuery = new ArchetypeQuery<T1, T2, T3>(this, signature); 
        queriesSig[signature.index] = newQuery;
        return newQuery;
    }
    
    public ArchetypeQuery<T1, T2, T3, T4> Query<T1, T2, T3, T4> (Signature<T1, T2, T3, T4> signature)
        where T1: struct
        where T2: struct
        where T3: struct
        where T4: struct
    {
        var query = queriesSig[signature.index];
        if (query != null) {
            return (ArchetypeQuery<T1, T2, T3, T4>)query;
        }
        var newQuery = new ArchetypeQuery<T1, T2, T3, T4>(this, signature); 
        queriesSig[signature.index] = newQuery;
        return newQuery;
    }
    
    public ArchetypeQuery<T1, T2, T3, T4, T5> Query<T1, T2, T3, T4, T5> (Signature<T1, T2, T3, T4, T5> signature)
        where T1: struct
        where T2: struct
        where T3: struct
        where T4: struct
        where T5: struct
    {
        var query = queriesSig[signature.index];
        if (query != null) {
            return (ArchetypeQuery<T1, T2, T3, T4, T5>)query;
        }
        var newQuery = new ArchetypeQuery<T1, T2, T3, T4, T5>(this, signature); 
        queriesSig[signature.index] = newQuery;
        return newQuery;
    }
    
    
    // ----------------------------------- query via generic parameters -----------------------------------
    private readonly Dictionary<long, ArchetypeQuery> queries = new Dictionary<long, ArchetypeQuery>();
    
    public ArchetypeQuery<T> Query<T> ()
        where T : struct
    {
        var hash = typeof(T).Handle(); // could StructHeap<T>.StructIndex to improve performance 
        if (queries.TryGetValue(hash, out var query)) {
            return (ArchetypeQuery<T>)query;
        }
        ReadOnlySpan<int> structIndices = stackalloc int[] {
            StructHeap<T>.StructIndex
        };
        var newQuery = new ArchetypeQuery<T>(this, structIndices);
        queries.Add(hash, newQuery);
        return newQuery;
    }
    
    public ArchetypeQuery<T1, T2> Query<T1, T2> ()
        where T1 : struct
        where T2 : struct
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle();
        if (queries.TryGetValue(hash, out var query)) {
            return (ArchetypeQuery<T1, T2>)query;
        }
        ReadOnlySpan<int> structIndices = stackalloc int[] {
            StructHeap<T1>.StructIndex,
            StructHeap<T2>.StructIndex,
        };
        var newQuery = new ArchetypeQuery<T1, T2>(this, structIndices);
        queries.Add(hash, newQuery);
        return newQuery;
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
            StructHeap<T3>.StructIndex,
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
            StructHeap<T4>.StructIndex,
        };
        query = new ArchetypeQuery(this, structIndices);
        queries.Add(hash, query);
        return query;
    }
    
    public ArchetypeQuery Query<T1, T2, T3, T4, T5> ()
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle() ^
                   typeof(T3).Handle() ^
                   typeof(T4).Handle() ^
                   typeof(T5).Handle();
        if (queries.TryGetValue(hash, out var query)) {
            return query;
        }
        ReadOnlySpan<int> structIndices = stackalloc int[] {
            StructHeap<T1>.StructIndex,
            StructHeap<T2>.StructIndex,
            StructHeap<T3>.StructIndex,
            StructHeap<T4>.StructIndex,
            StructHeap<T5>.StructIndex,
        };
        query = new ArchetypeQuery(this, structIndices);
        queries.Add(hash, query);
        return query;
    }
    
    public ArchetypeQuery Query<T1, T2, T3, T4, T5, T6> ()
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle() ^
                   typeof(T3).Handle() ^
                   typeof(T4).Handle() ^
                   typeof(T5).Handle() ^
                   typeof(T6).Handle();
        if (queries.TryGetValue(hash, out var query)) {
            return query;
        }
        ReadOnlySpan<int> structIndices = stackalloc int[] {
            StructHeap<T1>.StructIndex,
            StructHeap<T2>.StructIndex,
            StructHeap<T3>.StructIndex,
            StructHeap<T4>.StructIndex,
            StructHeap<T5>.StructIndex,
            StructHeap<T6>.StructIndex,
        };
        query = new ArchetypeQuery(this, structIndices);
        queries.Add(hash, query);
        return query;
    }
    #endregion
}
