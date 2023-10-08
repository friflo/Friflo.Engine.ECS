// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// <see cref="ArchetypeQuery"/> an all its generic implementation are immutable and designed to reuse its instances.
/// </summary>
public abstract class ArchetypeQuery
{
#region private fields
                    private  readonly   EntityStore     store;
    [Browse(Never)] internal readonly   Signature       signature;
    /// <summary>redundant with <see cref="signature"/> mask but enables masking without dereferencing <see cref="signature"/></summary>
    [Browse(Never)] private  readonly   ArchetypeMask   mask;
    [Browse(Never)] internal readonly   StructIndexes   structIndexes;
    //
    [Browse(Never)] private             Archetype[]     archetypes;
    [Browse(Never)] private             int             archetypeCount;
                    private             int             lastArchetypeCount;
    #endregion

                    public override     string          ToString() => signature.types.GetString("Query: ");

    internal ArchetypeQuery(EntityStore store, Signature signature) {
        this.store          = store;
        this.signature      = signature;
        archetypes          = new Archetype[1];
        mask                = signature.mask;
        lastArchetypeCount  = 1;
        var componentTypes  = signature.types;
        switch (componentTypes.Length) {
            case 1:
                structIndexes.T1 = componentTypes.T1.index;
                break;
            case 2:
                structIndexes.T1 = componentTypes.T1.index;
                structIndexes.T2 = componentTypes.T2.index;
                break;
            case 3:
                structIndexes.T1 = componentTypes.T1.index;
                structIndexes.T2 = componentTypes.T2.index;
                structIndexes.T3 = componentTypes.T3.index;
                break;
            case 4:
                structIndexes.T1 = componentTypes.T1.index;
                structIndexes.T2 = componentTypes.T2.index;
                structIndexes.T3 = componentTypes.T3.index;
                structIndexes.T4 = componentTypes.T4.index;
                break;
            case 5:
                structIndexes.T1 = componentTypes.T1.index;
                structIndexes.T2 = componentTypes.T2.index;
                structIndexes.T3 = componentTypes.T3.index;
                structIndexes.T4 = componentTypes.T4.index;
                structIndexes.T5 = componentTypes.T5.index;
                break;
            default:
                throw new NotImplementedException();
        }
        // store.AddQuery(this);
    }
    
    // private  readonly    List<ArchetypeQuery>    queries;            // only for debugging
    // internal void        AddQuery(ArchetypeQuery query) { queries.Add(query); }
    
    public ReadOnlySpan<Archetype> Archetypes {
        get
        {
            if (store.archetypesCount == lastArchetypeCount) {
                return new ReadOnlySpan<Archetype>(archetypes, 0, archetypeCount);
            }
            // --- update archetypes / archetypesCount: Add matching archetypes newly added to the store
            var storeArchetypes = store.Archetypes;
            var newStoreLength  = storeArchetypes.Length;
            var nextArchetypes  = archetypes;
            var nextCount       = archetypeCount;
            for (int n = lastArchetypeCount; n < newStoreLength; n++) {
                var archetype = storeArchetypes[n];
                if (!mask.Has(archetype.mask)) {
                    continue;
                }
                if (nextCount == nextArchetypes.Length) {
                    Utils.Resize(ref nextArchetypes, 2 * nextCount);
                }
                nextArchetypes[nextCount++] = archetype;
            }
            // --- order matters in case of parallel execution
            archetypes          = nextArchetypes;   // using changed (added) archetypes with old archetypeCount         => OK
            archetypeCount      = nextCount;        // archetypes already changed                                       => OK
            lastArchetypeCount  = newStoreLength;   // using old lastArchetypeCount result only in a redundant update   => OK
            return new ReadOnlySpan<Archetype>(nextArchetypes, 0, nextCount);
        }
    }
}


public sealed class ArchetypeQuery<T> : ArchetypeQuery
    where T : struct
{
    internal ArchetypeQuery(EntityStore store, Signature<T> signature)
        : base(store, signature) {
    }
}

public sealed class ArchetypeQuery<T1, T2> : ArchetypeQuery // : IEnumerable <>  // <- not implemented to avoid boxing
    where T1 : struct
    where T2 : struct
{
    internal    T1[]    copyT1;
    internal    T2[]    copyT2;
    
    internal ArchetypeQuery(EntityStore store, Signature<T1, T2> signature)
        : base(store, signature) {
    }
    
    public ArchetypeQuery<T1, T2> ReadOnly<T>() where T : struct
    {
        if (typeof(T1) == typeof(T)) copyT1 = new T1[StructUtils.ChunkSize];
        if (typeof(T2) == typeof(T)) copyT2 = new T2[StructUtils.ChunkSize];
        return this;
    }
    
    public QueryChunks      <T1,T2> Chunks                                      => new (this);
    public QueryEnumerator  <T1,T2> GetEnumerator()                             => new (this);
    public QueryForEach     <T1,T2> ForEach(Action<Ref<T1>, Ref<T2>> lambda)    => new (this, lambda);
}

public sealed class ArchetypeQuery<T1, T2, T3> : ArchetypeQuery
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    internal ArchetypeQuery(EntityStore store, Signature<T1, T2, T3> signature)
        : base(store, signature) {
    }
}

public sealed class ArchetypeQuery<T1, T2, T3, T4> : ArchetypeQuery
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
{
    internal ArchetypeQuery(EntityStore store, Signature<T1, T2, T3, T4> signature)
        : base(store, signature) {
    }
}

public sealed class ArchetypeQuery<T1, T2, T3, T4, T5> : ArchetypeQuery
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct
{
    internal ArchetypeQuery(EntityStore store, Signature<T1, T2, T3, T4, T5> signature)
        : base(store, signature) {
    }
}
