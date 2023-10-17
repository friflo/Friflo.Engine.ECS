// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
using ReadOnlyHeaps = System.ReadOnlySpan<Friflo.Fliox.Engine.ECS.StructHeap>;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed class Archetype
{
#region public properties
    /// <summary>Number of entities stored in the <see cref="Archetype"/></summary>
    [Browse(Never)] public              int                 EntityCount     => entityCount;
    [Browse(Never)] public              int                 ChunkEnd        // entity count: 0: 0, 1: 0, 512: 0, 513: 1, ...
                                                                            => (entityCount - 1) / StructUtils.ChunkSize;
    
    /// <summary>The entity ids store in the <see cref="Archetype"/></summary>
                    public              ReadOnlySpan<int>   EntityIds       => new (entityIds, 0, entityCount);
    
                    public              EntityStore         Store           => store;
                    public ref readonly ArchetypeStructs    Structs         => ref structs;
                    public ref readonly Tags                Tags            => ref tags;
    #endregion
    
#region private / internal members
                    private  readonly   StructHeap[]        structHeaps;    //  8 + all archetype components (struct heaps * componentCount)
    /// Store the entity id for each component. 
    [Browse(Never)] internal            int[]               entityIds;      //  8 + ids - could use a StructHeap<int> if needed
    [Browse(Never)] private             int                 entityCount;    //  4       - number of entities in archetype
                    private             int                 capacity;       //  4
    // --- internal
    [Browse(Never)] internal readonly   ArchetypeStructs    structs;        // 32       - struct component types of archetype
    [Browse(Never)] internal readonly   Tags                tags;           // 32       - tags assigned to archetype
    [Browse(Never)] internal readonly   ArchetypeKey        key;            //  8 (+76)
    /// <remarks>Lookups on <see cref="heapMap"/>[] does not require a range check. See <see cref="ComponentSchema.GetStructType"/></remarks>
    [Browse(Never)] internal readonly   StructHeap[]        heapMap;        //  8       - Length always = maxStructIndex. Used for heap lookup
    [Browse(Never)] internal readonly   EntityStore         store;          //  8       - containing EntityStore
    [Browse(Never)] internal readonly   GameEntityStore     gameEntityStore;//  8       - containing EntityStore
    [Browse(Never)] internal readonly   int                 archIndex;      //  4       - index in EntityStore
    [Browse(Never)] internal readonly   int                 componentCount; //  4       - number of component types
                    internal readonly   StandardComponents  std;            // 32       - heap references to std types: Position, Rotation, ...
    
    [Browse(Never)] internal            ReadOnlyHeaps       Heaps           => structHeaps;
                    public   override   string              ToString()      => GetString();
    #endregion
    
#region initialize
    /// <summary>Create an instance of an <see cref="EntityStore.defaultArchetype"/></summary>
    internal Archetype(in ArchetypeConfig config)
    {
        store           = config.store;
        gameEntityStore = store as GameEntityStore;
        archIndex       = EntityStore.Static.DefaultArchIndex;
        structHeaps     = Array.Empty<StructHeap>();
        heapMap         = EntityStore.Static.DefaultHeapMap; // all items are always null
        key             = new ArchetypeKey(this);
        // entityIds        = null      // stores no entities
        // capacity         = 0         // stores no entities
        // componentCount   = 0         // has no struct components
        // structs          = default   // has no struct components
        // tags             = default   // has no tags
    }
    
    /// <summary>
    /// Note!: Ensure constructor cannot throw exceptions to eliminate <see cref="TypeInitializationException"/>'s
    /// </summary>
    private Archetype(in ArchetypeConfig config, StructHeap[] heaps, in Tags tags)
    {
        store           = config.store;
        gameEntityStore = store as GameEntityStore;
        archIndex       = config.archetypeIndex;
        capacity        = config.capacity;
        componentCount  = heaps.Length;
        structHeaps     = heaps;
        entityIds       = new int [1];
        heapMap         = new StructHeap[config.maxStructIndex];
        structs         = new ArchetypeStructs(heaps);
        this.tags       = tags;
        key             = new ArchetypeKey(this);
        for (int pos = 0; pos < componentCount; pos++)
        {
            var heap = heaps[pos];
            heap.SetArchetype(this);
            heapMap[heap.structIndex] = heap;
            SetStandardComponentHeaps(heap, ref std);
        }
    }
    
    private static void SetStandardComponentHeaps(StructHeap heap, ref StandardComponents std)
    {
        var type = heap.type;
        if        (type == typeof(Position)) {
            std.position    = (StructHeap<Position>)    heap;
        } else if (type == typeof(Rotation)) {
            std.rotation    = (StructHeap<Rotation>)    heap;
        } else if (type == typeof(Scale3)) {
            std.scale3      = (StructHeap<Scale3>)      heap;
        } else if (type == typeof(EntityName)) {
            std.name        = (StructHeap<EntityName>)  heap;
        }
    }

    /// <remarks>Is called by methods using generic struct component type: T1, T2, T3, ...</remarks>
    internal static Archetype CreateWithSignatureTypes(in ArchetypeConfig config, in SignatureIndexes indexes, in Tags tags)
    {
        var length          = indexes.length;
        var componentHeaps  = new StructHeap[length];
        var structs         = EntityStore.Static.ComponentSchema.Structs;
        for (int n = 0; n < length; n++) {
            var structIndex   = indexes.GetStructIndex(n);
            var structType    = structs[structIndex];
            componentHeaps[n] = structType.CreateHeap(config.capacity);
        }
        return new Archetype(config, componentHeaps, tags);
    }
    
    /// <remarks>
    /// Is called by methods using a set of arbitrary struct <see cref="ComponentType"/>'s.<br/>
    /// Using a <see cref="List{T}"/> of types is okay. Method is only called for missing <see cref="Archetype"/>'s
    /// </remarks>
    internal static Archetype CreateWithStructTypes(in ArchetypeConfig config, List<ComponentType> types, in Tags tags)
    {
        var length          = types.Count;
        var componentHeaps  = new StructHeap[length];
        for (int n = 0; n < length; n++) {
            componentHeaps[n] = types[n].CreateHeap(config.capacity);
        }
        return new Archetype(config, componentHeaps, tags);
    }
    #endregion
    
#region struct component handling

    internal int MoveEntityTo(int id, int sourceIndex, Archetype newArchetype)
    {
        // --- copy entity components to components of new newArchetype
        var targetIndex = newArchetype.AddEntity(id);
        foreach (var sourceHeap in structHeaps)
        {
            var targetHeap = newArchetype.heapMap[sourceHeap.structIndex];
            if (targetHeap == null) {
                continue;
            }
            sourceHeap.CopyComponentTo(sourceIndex, targetHeap, targetIndex);
        }
        MoveLastComponentsTo(sourceIndex);
        return targetIndex;
    }
    
    internal void MoveLastComponentsTo(int newIndex)
    {
        var lastIndex = entityCount - 1;
        // --- decrement entityCount if the newIndex is already the last entity id
        if (lastIndex == newIndex) {
            entityCount = newIndex;
            return;
        }
        // --- move components of last entity to the index where the entity is currently placed to avoid unused entries
        foreach (var heap in structHeaps) {
            heap.MoveComponent(lastIndex, newIndex);
        }
        var lastEntityId    = entityIds[lastIndex];
        if (gameEntityStore != null) {
            gameEntityStore.nodes[lastEntityId].entity.compIndex = newIndex; // set component index for new archetype
        }
        entityIds[newIndex] = lastEntityId;
        entityCount--;      // remove last entity id
    }
    
    internal int AddEntity(int id)
    {
        var index = entityCount;
        if (index == entityIds.Length) {
            Utils.Resize(ref entityIds, 2 * entityIds.Length);
        }
        entityIds[entityCount++] = id;  // add entity id
        EnsureCapacity(index + 1);
        return index;
    }
    
    private void  EnsureCapacity(int newCapacity) {
        if (capacity >= newCapacity) {
            return;
        }
        capacity = newCapacity;
        foreach (var heap in structHeaps) {
            heap.SetCapacity(newCapacity);
        }
    }
    
    private string GetString() {
        var sb          = new StringBuilder();
        var hasTypes    = false;
        sb.Append('[');
        foreach (var heap in structHeaps) {
            sb.Append(heap.type.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        foreach (var tag in tags) {
            sb.Append('#');
            sb.Append(tag.type.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        if (hasTypes) {
            sb.Length -= 2;
            sb.Append("]  Count: ");
            sb.Append(entityCount);
            return sb.ToString();
        }
        return "[]";
    }
    #endregion
}
