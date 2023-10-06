// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;


internal struct StructIndexes
{
    internal int T1;
    internal int T2;
    internal int T3;
    internal int T4;
    internal int T5;
}

public abstract class ArchetypeQuery
{
#region private fields
                    private  readonly   EntityStore     store;
                    private  readonly   ArchetypeMask   mask;
    [Browse(Never)] internal readonly   Signature       signature;
    [Browse(Never)] internal readonly   StructIndexes   structIndexes;
    //
    [Browse(Never)] private             Archetype[]     archetypes;
    [Browse(Never)] private             int             archetypeCount;
                    private             int             lastArchetypeCount;
    #endregion
    
    internal ArchetypeQuery(EntityStore store, Signature signature) {
        this.store          = store;
        archetypes          = new Archetype[1];
        mask                = new ArchetypeMask(signature);
        this.signature      = signature; 
        lastArchetypeCount  = 1;
        var componentTypes  = signature.componentTypes;
        switch (componentTypes.Length) {
            case 1:
                structIndexes.T1 = componentTypes[0].index;
                break;
            case 2:
                structIndexes.T1 = componentTypes[0].index;
                structIndexes.T2 = componentTypes[1].index;
                break;
            case 3:
                structIndexes.T1 = componentTypes[0].index;
                structIndexes.T2 = componentTypes[1].index;
                structIndexes.T3 = componentTypes[2].index;
                break;
            case 4:
                structIndexes.T1 = componentTypes[0].index;
                structIndexes.T2 = componentTypes[1].index;
                structIndexes.T3 = componentTypes[2].index;
                structIndexes.T4 = componentTypes[3].index;
                break;
            case 5:
                structIndexes.T1 = componentTypes[0].index;
                structIndexes.T2 = componentTypes[1].index;
                structIndexes.T3 = componentTypes[2].index;
                structIndexes.T4 = componentTypes[3].index;
                structIndexes.T5 = componentTypes[4].index;
                break;
            default:
                throw new NotImplementedException();
        }
    }
    
    public ReadOnlySpan<Archetype> Archetypes {
        get
        {
            if (store.archetypesCount == lastArchetypeCount) {
                return new ReadOnlySpan<Archetype>(archetypes, 0, archetypeCount);
            }
            var storeArchetypes = store.Archetypes;
            var newCount        = storeArchetypes.Length;
            for (int n = lastArchetypeCount; n < newCount; n++) {
                var archetype = storeArchetypes[n];
                if (!mask.Has(archetype.mask)) {
                    continue;
                }
                if (archetypeCount == archetypes.Length) {
                    Utils.Resize(ref archetypes, 2 * archetypeCount);
                }
                archetypes[archetypeCount++] = archetype;
            }
            lastArchetypeCount = newCount;
            return new ReadOnlySpan<Archetype>(archetypes, 0, archetypeCount);
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
    internal bool readOnlyT1;
    internal bool readOnlyT2;
    
    internal ArchetypeQuery(EntityStore store, Signature<T1, T2> signature)
        : base(store, signature) {
    }
    
    public ArchetypeQuery<T1, T2> RO<T>() where T : struct
    {
        readOnlyT1 |= typeof(T1) == typeof(T);
        readOnlyT2 |= typeof(T2) == typeof(T);
        return this;
    }
    
    public ArchetypeChunks  <T1,T2> Chunks                                      => new (this);
    public QueryEnumerator  <T1,T2> GetEnumerator()                             => new (this);
    public ForEachQuery     <T1,T2> ForEach(Action<Ref<T1>, Ref<T2>> lambda)    => new (this, lambda);
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
