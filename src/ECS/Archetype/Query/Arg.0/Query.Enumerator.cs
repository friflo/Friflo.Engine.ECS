// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public ref struct QueryEnumerator
{
    private             int                     entityPos;
    private             int                     componentLen;
    private             int[]                   entityIds;
    
    private  readonly   ReadOnlySpan<Archetype> archetypes;
    private             int                     archetypePos;
    
    internal QueryEnumerator(ArchetypeQuery query)
    {
        archetypes      = query.GetArchetypes();
        archetypePos    = 0;
        var archetype   = archetypes[0];
        entityIds       = archetype.entityIds;
        entityPos       = -1;
        componentLen    = archetype.EntityCount - 1;
    }
    
    public readonly int Current   => entityIds[entityPos];
    
    // --- IEnumerator
    public bool MoveNext() {
        if (entityPos < componentLen) {
            entityPos++;
            return true;
        }
        if (archetypePos < archetypes.Length - 1) {
            var archetype   = archetypes[++archetypePos];
            entityPos       = 0;
            entityIds       = archetype.entityIds;
            componentLen    = archetype.EntityCount - 1;
            return true;
        }
        return false;  
    }
}
