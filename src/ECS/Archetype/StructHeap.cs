// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal abstract class StructHeap
{
    // --- internal fields
    internal readonly   int         structIndex;    //  4
#if DEBUG
    internal            Archetype   archetype;      // only used for debugging
#endif

    internal  abstract  Type        StructType          { get; }
    protected abstract  void        DebugInfo           (out int count, out int length);
    internal  abstract  void        SetChunkCapacity    (int newChunkCount, int chunkCount, int newChunkLength, int chunkLength);
    internal  abstract  void        MoveComponent       (int from, int to);
    internal  abstract  void        CopyComponentTo     (int sourcePos, StructHeap target, int targetPos);
    internal  abstract  object      GetComponentDebug   (int compIndex);
    internal  abstract  Bytes       Write               (ObjectWriter writer, int compIndex);
    internal  abstract  void        Read                (ObjectReader reader, int compIndex, JsonValue json);

    internal StructHeap(int structIndex) {
        this.structIndex    = structIndex;
    }

    internal void SetArchetype(Archetype archetype) {
#if DEBUG
        this.archetype = archetype;
#endif
    }
    
    public override string ToString() {
        DebugInfo(out int count, out int length);
        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(StructType.Name);
        sb.Append("] chunks - Count: ");
        sb.Append(count);
        sb.Append(", Length: ");
        sb.Append(length);
#if DEBUG
        sb.Append(", EntityCount: ");
        sb.Append(archetype.EntityCount);
#endif
        return sb.ToString();
    }
}
