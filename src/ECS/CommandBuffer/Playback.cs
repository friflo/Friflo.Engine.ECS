// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable UseCollectionExpression
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal struct EntityChange
{
    internal            ComponentTypes                  componentTypes; // 32 
    internal            Tags                            tags;           // 32
    internal            Archetype                       oldArchetype;   //  8
    internal            int                             entityId;       //  4
}

internal class Playback
{
    /// <summary>
    /// Store indexes into <see cref="entityChanges"/> instead the <see cref="EntityChange"/> value directly.<br/>
    /// Size of <see cref="EntityChange"/> is too big (96 bytes) which degrade performance when rehashing Dictionary.
    /// </summary> 
    internal readonly   Dictionary<int, int>            entityChangesIndexes;   //  8
    internal            EntityChange[]                  entityChanges;          //  8
    internal            int                             entityChangesCount;     //  4
    internal readonly   EntityStore                     store;                  //  8
    
    internal Playback(EntityStore store) {
        this.store              = store;
        entityChangesIndexes    = new Dictionary<int, int>();
        entityChanges           = Array.Empty<EntityChange>();
    }
    
    public EntityChange[] ResizeChanges() {
        return ArrayUtils.Resize(ref entityChanges, Math.Max(8, 2 * entityChanges.Length));
    }
}
