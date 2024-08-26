// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


/// <remarks>
/// <b>Note:</b> Should not contain any other fields. Reasons:<br/>
/// - to enable maximum efficiency when GC iterate <see cref="Archetype.structHeaps"/> <see cref="Archetype.heapMap"/>
///   for collection.
/// </remarks>
internal abstract class StructHeap : IComponentStash
{
    // Note: Should not contain any other field. See class <remarks>
    // --- internal fields
    internal readonly   int         structIndex;    //  4
#if DEBUG
    // ReSharper disable once NotAccessedField.Local
    private             Archetype   archetype;      // only used for debugging
#endif
    public    abstract  IComponent  GetStashDebug  ();

    internal  abstract  Type        StructType              { get; }
    internal  abstract  void        StashComponent          (int compIndex);
    internal  abstract  void        SetBatchComponent       (BatchComponent[] components, int compIndex);
    protected abstract  int         ComponentsLength        { get; }
    internal  abstract  void        ResizeComponents        (int capacity, int count);
    internal  abstract  void        MoveComponent           (int from, int to);
    internal  abstract  void        CopyComponentTo         (int sourcePos, StructHeap target, int targetPos);
    internal  abstract  void        CopyComponent           (int sourcePos, int targetPos);
    internal  abstract  void        SetComponentDefault     (int compIndex);
    internal  abstract  void        SetComponentsDefault    (int compIndexStart, int count);
    internal  abstract  IComponent  GetComponentDebug       (int compIndex);
    internal  abstract  Bytes       Write                   (ObjectWriter writer, int compIndex);
    internal  abstract  void        Read                    (ObjectReader reader, int compIndex, JsonValue json);

    internal StructHeap(int structIndex) {
        this.structIndex    = structIndex;
    }

    internal void SetArchetypeDebug(Archetype archetype) {
#if DEBUG
        this.archetype = archetype;
#endif
    }
    
    /*
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage]
    internal static void AssertChunksLength(int expect, int actual) {
        if (expect != actual) throw new InvalidOperationException($"expect chunk length: {expect}, was: {actual}");
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage]
    internal static void AssertChunkComponentsNull(object components) {
        if (components != null) throw new InvalidOperationException($"expect components == null");
    } */
    
    public override string ToString() {
        int length = ComponentsLength;
        var sb = new StringBuilder();
        sb.Append("StructHeap<");
        sb.Append(StructType.Name);
        sb.Append(">  Capacity: ");
        sb.Append(length);
        return sb.ToString();
    }
}
