// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal abstract class StructHeap
{
    // --- internal
    internal readonly   string      keyName;
    internal readonly   Type        type;
    internal readonly   int         heapIndex;
    internal readonly   long        hash;
#if DEBUG
    internal            Archetype   archetype; // only used provide debug info.
#endif

    public   override   string      ToString() => GetString();
    
    internal abstract   StructHeap  CreateHeap          (int capacity, TypeStore typeStore);
    internal abstract   void        SetCapacity         (int capacity);
    internal abstract   void        MoveComponent       (int from, int to);
    internal abstract   void        CopyComponentTo     (int sourcePos, StructHeap target, int targetPos);
    internal abstract   object      GetComponentDebug   (int compIndex);
    internal abstract   void        Write               (ObjectWriter writer, int compIndex);

    internal StructHeap(int heapIndex, string keyName, Type type) {
        this.heapIndex  = heapIndex;
        this.keyName    = keyName;
        this.type       = type;
        hash            = type.Handle();
    }

    internal string GetString() {
#if DEBUG
        return $"[{type.Name}] heap - Count: {archetype.EntityCount}";
#else
        return $"[{type.Name}] heap";
#endif
    }
}
