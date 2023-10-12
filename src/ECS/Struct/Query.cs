// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// <see cref="ArchetypeQuery"/> an all its generic implementation are immutable and designed to reuse its instances.
/// </summary>
public class ArchetypeQuery
{
#region private / internal fields
    // --- non blittable types
                    private  readonly   EntityStore         store;              //  8
    [Browse(Never)] private             Archetype[]         archetypes;         //  8   current list of matching archetypes, can grow
    // --- blittable types
    [Browse(Never)] private             int                 archetypeCount;     //  4   current number archetypes 
                    private             int                 lastArchetypeCount; //  4   number of archetypes the EntityStore had on last check
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   // 24   ordered struct indices of struct component types: T1,T2,T3,T4,T5
    [Browse(Never)] private             Tags                requiredTags;       // 32   entity tags an Archetype must have
                    
                    public override     string              ToString() => GetString();
    #endregion

#region methods
    public ArchetypeQuery   AllTags(in Tags tags) { SetRequiredTags(tags); return this; }

//  public QueryChunks      Chunks                                      => new (this);
    public QueryEnumerator  GetEnumerator()                             => new (this);
//  public QueryForEach     ForEach(Action<Ref<T1>, Ref<T2>> lambda)    => new (this, lambda);

    internal ArchetypeQuery(
        EntityStore         store,
        in SignatureIndexes indexes)
    {
        this.store          = store;
        archetypes          = Array.Empty<Archetype>();
        lastArchetypeCount  = 1;
        signatureIndexes    = indexes;
        // store.AddQuery(this);
    }
    
    /// <remarks>
    /// Reset <see cref="lastArchetypeCount"/> to force update of <see cref="archetypes"/> on subsequent call to <see cref="Archetypes"/>
    /// </remarks>
    internal void SetRequiredTags(in Tags tags) {
        requiredTags        = tags;
        lastArchetypeCount  = 1;
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
            var requiredStructs = new ArchetypeStructs(signatureIndexes);
            
            for (int n = lastArchetypeCount; n < newStoreLength; n++)
            {
                var archetype         = storeArchetypes[n];
                var hasRequiredTypes  = archetype.structs.HasAll(requiredStructs) &&
                                        archetype.tags.   HasAll(requiredTags);
                if (!hasRequiredTypes) {
                    continue;
                }
                if (nextCount == nextArchetypes.Length) {
                    var length = Math.Max(4, 2 * nextCount);
                    Utils.Resize(ref nextArchetypes, length);
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

    private string GetString() {
        var sb          = new StringBuilder();
        var hasTypes    = false;
        sb.Append("Query: [");
        var structs = EntityStore.Static.ComponentSchema.Structs;
        for (int n = 0; n < signatureIndexes.length; n++)
        {
            var structIndex = signatureIndexes.GetIndex(n);
            var structType = structs[structIndex];
            sb.Append(structType.type.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        foreach (var tag in requiredTags) {
            sb.Append('#');
            sb.Append(tag.type.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        if (hasTypes) {
            sb.Length -= 2;
            sb.Append(']');
        }
        return sb.ToString();
    }
    #endregion
}

public sealed class ArchetypeQuery<T> : ArchetypeQuery
    where T : struct, IStructComponent
{
    public new ArchetypeQuery<T> AllTags (in Tags tags) { SetRequiredTags(tags); return this; }
    
    internal ArchetypeQuery(EntityStore store, in Signature<T> signature)
        : base(store, signature.signatureIndexes) {
    }
}

public sealed class ArchetypeQuery<T1, T2> : ArchetypeQuery // : IEnumerable <>  // <- not implemented to avoid boxing
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
{
    internal    T1[]    copyT1;
    internal    T2[]    copyT2;
    
     public new ArchetypeQuery<T1, T2> AllTags (in Tags tags) { SetRequiredTags(tags); return this; }
    
    internal ArchetypeQuery(EntityStore store, in Signature<T1, T2> signature)
        : base(store, signature.signatureIndexes) {
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
    public new ArchetypeQuery<T1, T2, T3> AllTags (in Tags tags) { SetRequiredTags(tags); return this; }
    
    internal ArchetypeQuery(EntityStore store, in Signature<T1, T2, T3> signature)
        : base(store, signature.signatureIndexes) {
    }
}

public sealed class ArchetypeQuery<T1, T2, T3, T4> : ArchetypeQuery
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
    where T4 : struct, IStructComponent
{
    public new ArchetypeQuery<T1, T2, T3, T4> AllTags (in Tags tags) { SetRequiredTags(tags); return this; }
    
    internal ArchetypeQuery(EntityStore store, in Signature<T1, T2, T3, T4> signature)
        : base(store, signature.signatureIndexes) {
    }
}

public sealed class ArchetypeQuery<T1, T2, T3, T4, T5> : ArchetypeQuery
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
    where T4 : struct, IStructComponent
    where T5 : struct, IStructComponent
{
    public new ArchetypeQuery<T1, T2, T3, T4, T5> AllTags (in Tags tags) { SetRequiredTags(tags); return this; }
    
    internal ArchetypeQuery(EntityStore store, in Signature<T1, T2, T3, T4, T5> signature)
        : base(store, signature.signatureIndexes) {
    }
}
