// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public class ArchetypeQuery
{
#region private fields
    private readonly    EntityStore     store;
    private readonly    ArchetypeMask   mask;
    private             int             lastArchetypeCount;
    private             Archetype[]     archetypes;
    private             int             archetypeCount;
    #endregion
    
    internal ArchetypeQuery(EntityStore store, ReadOnlySpan<int> structIndices) {
        this.store          = store;
        archetypes          = new Archetype[1];
        mask                = new ArchetypeMask(structIndices);
        lastArchetypeCount  = 1;
    }
    
    public ReadOnlySpan<Archetype> Archetypes {
        get {
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