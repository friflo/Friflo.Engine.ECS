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
    // --- non blittable types
                    private  readonly   EntityStore         store;
    [Browse(Never)] private             Archetype[]         archetypes;
    [Browse(Never)] internal readonly   SignatureTypeSet    types;
    // --- blittable types
    [Browse(Never)] private  readonly   ArchetypeStructs    structs;
    [Browse(Never)] internal readonly   StructIndexes       structIndexes;
    [Browse(Never)] internal readonly   Tags                tags;
    [Browse(Never)] private             int                 archetypeCount;
                    private             int                 lastArchetypeCount;
    #endregion

                    public override     string              ToString() => types.GetString("Query: ");

    internal ArchetypeQuery(
        EntityStore         store,
        in ArchetypeStructs structs,
        in SignatureTypeSet types,
        in Tags             tags)
    {
        this.store          = store;
        archetypes          = new Archetype[1];
        this.structs        = structs;
        this.types          = types;
        this.tags           = tags;
        lastArchetypeCount  = 1;
        var componentTypes  = types;
        switch (componentTypes.Length) {
            case 1:
                structIndexes.T1 = componentTypes.T1.structIndex;
                break;
            case 2:
                structIndexes.T1 = componentTypes.T1.structIndex;
                structIndexes.T2 = componentTypes.T2.structIndex;
                break;
            case 3:
                structIndexes.T1 = componentTypes.T1.structIndex;
                structIndexes.T2 = componentTypes.T2.structIndex;
                structIndexes.T3 = componentTypes.T3.structIndex;
                break;
            case 4:
                structIndexes.T1 = componentTypes.T1.structIndex;
                structIndexes.T2 = componentTypes.T2.structIndex;
                structIndexes.T3 = componentTypes.T3.structIndex;
                structIndexes.T4 = componentTypes.T4.structIndex;
                break;
            case 5:
                structIndexes.T1 = componentTypes.T1.structIndex;
                structIndexes.T2 = componentTypes.T2.structIndex;
                structIndexes.T3 = componentTypes.T3.structIndex;
                structIndexes.T4 = componentTypes.T4.structIndex;
                structIndexes.T5 = componentTypes.T5.structIndex;
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
                if (!structs.Has(archetype.structs)) {
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
    where T : struct, IStructComponent
{
    internal ArchetypeQuery(EntityStore store, in Signature<T> signature, in Tags tags)
        : base(store, signature.structs, signature.types, tags) {
    }
}

public sealed class ArchetypeQuery<T1, T2> : ArchetypeQuery // : IEnumerable <>  // <- not implemented to avoid boxing
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
{
    internal    T1[]    copyT1;
    internal    T2[]    copyT2;
    
    internal ArchetypeQuery(EntityStore store, in Signature<T1, T2> signature, in Tags tags)
        : base(store, signature.structs, signature.types, tags) {
    }
    
    public ArchetypeQuery<T1, T2> ReadOnly<T>()
        where T : struct, IStructComponent
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
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
{
    internal ArchetypeQuery(EntityStore store, in Signature<T1, T2, T3> signature, in Tags tags)
        : base(store, signature.structs, signature.types, tags) {
    }
}

public sealed class ArchetypeQuery<T1, T2, T3, T4> : ArchetypeQuery
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
    where T4 : struct, IStructComponent
{
    internal ArchetypeQuery(EntityStore store, in Signature<T1, T2, T3, T4> signature, in Tags tags)
        : base(store, signature.structs, signature.types, tags) {
    }
}

public sealed class ArchetypeQuery<T1, T2, T3, T4, T5> : ArchetypeQuery
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
    where T4 : struct, IStructComponent
    where T5 : struct, IStructComponent
{
    internal ArchetypeQuery(EntityStore store, in Signature<T1, T2, T3, T4, T5> signature, in Tags tags)
        : base(store, signature.structs, signature.types, tags) {
    }
}
