// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal abstract class StructHeap
{
    // --- internal
    internal readonly   string      structKey;
    internal readonly   Bytes       keyBytes;
    internal readonly   Type        type;
    internal readonly   int         structIndex;
    internal readonly   long        hash;
#if DEBUG
    internal            Archetype   archetype; // only used provide debug info.
#endif

    public   override   string      ToString() => GetString();
    
    internal abstract   void        SetCapacity         (int capacity);
    internal abstract   void        MoveComponent       (int from, int to);
    internal abstract   void        CopyComponentTo     (int sourcePos, StructHeap target, int targetPos);
    internal abstract   object      GetComponentDebug   (int compIndex);
    internal abstract   Bytes       Write               (ObjectWriter writer, int compIndex);
    internal abstract   void        Read                (ObjectReader reader, int compIndex, JsonValue json);

    internal StructHeap(int structIndex, string structKey, Type type) {
        this.structIndex    = structIndex;
        this.structKey      = structKey;
        keyBytes            = new Bytes(structKey);
        this.type           = type;
        hash                = type.Handle();
    }

    internal string GetString() {
#if DEBUG
        return $"[{type.Name}] heap - Count: {archetype.EntityCount}";
#else
        return $"[{type.Name}] heap";
#endif
    }

    [Conditional("DEBUG")]
    internal void SetArchetype(Archetype archetype) {
#if DEBUG
        this.archetype = archetype;
#endif
    }
}
