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
#region private fields
    // --- non blittable types
                    private  readonly   EntityStore         store;              //  8
    [Browse(Never)] private             Archetype[]         archetypes;         //  8
    [Browse(Never)] internal readonly   SignatureTypeSet    types;              // 48 
    // --- blittable types
    [Browse(Never)] private  readonly   ArchetypeStructs    structs;            // 32 
    [Browse(Never)] internal readonly   StructIndexes       structIndexes;      // 20
    [Browse(Never)] internal            Tags                allTags;            // 32
    [Browse(Never)] private             int                 archetypeCount;     //  4
                    private             int                 lastArchetypeCount; //  4
                    
                    public              ArchetypeQuery      AllTags(in Tags tags) { allTags = tags; return this; }
                    
                    public override     string              ToString() => GetString();
    #endregion

//  public QueryChunks      Chunks                                      => new (this);
    public QueryEnumerator  GetEnumerator()                             => new (this);
//  public QueryForEach     ForEach(Action<Ref<T1>, Ref<T2>> lambda)    => new (this, lambda);

    internal ArchetypeQuery(
        EntityStore         store,
        in ArchetypeStructs structs,
        in SignatureTypeSet types)
    {
        this.store          = store;
        archetypes          = new Archetype[1];
        this.structs        = structs;
        this.types          = types;
        lastArchetypeCount  = 1;
        var componentTypes  = types;
        switch (componentTypes.Length) {
            case 0:
                break;
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
                var archetype       = storeArchetypes[n];
                bool hasAllTypes    = structs.Has(archetype.structs) && archetype.tags.HasAll(allTags);
                if (!hasAllTypes) {
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

    private string GetString() {
        var sb          = new StringBuilder();
        var hasTypes    = false;
        sb.Append("Query: [");
        foreach (var structType in structs) {
            sb.Append(structType.type.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        foreach (var tag in allTags) {
            sb.Append('#');
            sb.Append(tag.type.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        if (hasTypes) {
            sb.Length -= 2;
            sb.Append(']');
            return sb.ToString();
        }
        return "[]";
    }
}

public sealed class ArchetypeQuery<T> : ArchetypeQuery
    where T : struct, IStructComponent
{
    public new ArchetypeQuery<T> AllTags (in Tags tags) { allTags = tags; return this; }
    
    internal ArchetypeQuery(EntityStore store, in Signature<T> signature)
        : base(store, signature.structs, signature.types) {
    }
}

public sealed class ArchetypeQuery<T1, T2> : ArchetypeQuery // : IEnumerable <>  // <- not implemented to avoid boxing
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
{
    internal    T1[]    copyT1;
    internal    T2[]    copyT2;
    
     public new ArchetypeQuery<T1, T2> AllTags (in Tags tags) { allTags = tags; return this; }
    
    internal ArchetypeQuery(EntityStore store, in Signature<T1, T2> signature)
        : base(store, signature.structs, signature.types) {
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
    public new ArchetypeQuery<T1, T2, T3> AllTags (in Tags tags) { allTags = tags; return this; }
    
    internal ArchetypeQuery(EntityStore store, in Signature<T1, T2, T3> signature)
        : base(store, signature.structs, signature.types) {
    }
}

public sealed class ArchetypeQuery<T1, T2, T3, T4> : ArchetypeQuery
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
    where T4 : struct, IStructComponent
{
    public new ArchetypeQuery<T1, T2, T3, T4> AllTags (in Tags tags) { allTags = tags; return this; }
    
    internal ArchetypeQuery(EntityStore store, in Signature<T1, T2, T3, T4> signature)
        : base(store, signature.structs, signature.types) {
    }
}

public sealed class ArchetypeQuery<T1, T2, T3, T4, T5> : ArchetypeQuery
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
    where T4 : struct, IStructComponent
    where T5 : struct, IStructComponent
{
    public new ArchetypeQuery<T1, T2, T3, T4, T5> AllTags (in Tags tags) { allTags = tags; return this; }
    
    internal ArchetypeQuery(EntityStore store, in Signature<T1, T2, T3, T4, T5> signature)
        : base(store, signature.structs, signature.types) {
    }
}
