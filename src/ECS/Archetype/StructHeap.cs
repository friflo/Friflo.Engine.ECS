// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <remarks>
/// <b>Note:</b> Should not contain any other fields. Reasons:<br/>
/// - to enable maximum efficiency when GC iterate <see cref="Archetype.structHeaps"/> <see cref="Archetype.heapMap"/>
///   for collection.
/// </remarks>
internal abstract class StructHeap
{
    // Note: Should not contain any other field. See class <remarks>
    // --- internal fields
    internal readonly   int         structIndex;    //  4
#if DEBUG
    private             Archetype   archetype;      // only used for debugging
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

    internal void SetArchetypeDebug(Archetype archetype) {
#if DEBUG
        this.archetype = archetype;
#endif
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage]
    internal static void AssertChunksLength(int expect, int actual) {
        if (expect != actual) throw new InvalidOperationException($"expect chunk length: {expect}, was: {actual}");
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage]
    internal static void AssertChunkComponentsNull(object components) {
        if (components != null) throw new InvalidOperationException($"expect components == null");
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
