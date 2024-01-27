// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal struct EntityChange
{
    internal            ComponentTypes                  componentTypes; // 32 
    internal            Tags                            tags;           // 32
}

internal readonly struct Playback
{
    // possible opportunity for optimization as Dictionary values are using 64 bytes
    internal readonly   Dictionary<int, EntityChange>   entityChanges;  //  8
    internal readonly   EntityStore                     store;          //  8
    
    internal Playback(EntityStore store) {
        this.store      = store;
        entityChanges   = new Dictionary<int, EntityChange>();
    }
}
