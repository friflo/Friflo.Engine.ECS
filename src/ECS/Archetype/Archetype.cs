// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
using static Friflo.Fliox.Engine.ECS.StructInfo;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed class Archetype
{
#region     public properties
    /// <summary>Number of entities stored in the <see cref="Archetype"/></summary>
    [Browse(Never)] public              int                 EntityCount     => entityCount;
    [Browse(Never)] public              int                 ComponentCount  => structCount;
                    public              int                 Capacity        => memory.capacity;

    /// <summary>The entity ids store in the <see cref="Archetype"/></summary>
                    public              ReadOnlySpan<int>   EntityIds       => new (entityIds, 0, entityCount);
    
                    public              EntityStoreBase     Store           => store;
                    public ref readonly ArchetypeStructs    Structs         => ref structs;
                    public ref readonly Tags                Tags            => ref tags;
#endregion

#region     internal properties
    [Browse(Never)] internal ReadOnlySpan<StructHeap>       Heaps           => structHeaps;
    [Browse(Never)] internal            int                 ChunkCount      // entity count: 0: 0   1:0   512:0   513:1, ...
                                                                            => entityCount / ChunkSize;
    [Browse(Never)] internal            int                 ChunkEnd        // entity count: 0:-1   1:0   512:0   513:1, ...
                                                                            => (entityCount + ChunkSize - 1) / ChunkSize - 1;
    [Browse(Never)] internal            int                 ChunkRest       => entityCount % ChunkSize;
                    public   override   string              ToString()      => GetString();
#endregion

#region     private / internal members
                    private  readonly   StructHeap[]        structHeaps;    //  8 + all archetype components (struct heaps * componentCount)
    /// Store the entity id for each component. 
    [Browse(Never)] internal            int[]               entityIds;      //  8 + ids - could use a StructHeap<int> if needed
    [Browse(Never)] private             int                 entityCount;    //  4       - number of entities in archetype
                    private             ChunkMemory         memory;         // 16       - count & length used to store components in chunks  
    // --- internal
    [Browse(Never)] internal readonly   int                 structCount;    //  4       - number of component types
    [Browse(Never)] internal readonly   ArchetypeStructs    structs;        // 32       - component types of archetype
    [Browse(Never)] internal readonly   Tags                tags;           // 32       - tags assigned to archetype
    [Browse(Never)] internal readonly   ArchetypeKey        key;            //  8 (+76)
    /// <remarks>Lookups on <see cref="heapMap"/>[] does not require a range check. See <see cref="ComponentSchema.CheckStructIndex"/></remarks>
    [Browse(Never)] internal readonly   StructHeap[]        heapMap;        //  8       - Length always = maxStructIndex. Used for heap lookup
    [Browse(Never)] internal readonly   EntityStoreBase     store;          //  8       - containing EntityStoreBase
    [Browse(Never)] internal readonly   EntityStore         entityStore;    //  8       - containing EntityStore
    [Browse(Never)] internal readonly   int                 archIndex;      //  4       - archetype index in EntityStore.archs[]
                    internal readonly   StandardComponents  std;            // 32       - heap references to std types: Position, Rotation, ...
    #endregion

#region initialize
    /// <summary>Create an instance of an <see cref="EntityStoreBase.defaultArchetype"/></summary>
    internal Archetype(in ArchetypeConfig config)
    {
        store           = config.store;
        entityStore     = store as EntityStore;
        archIndex       = EntityStoreBase.Static.DefaultArchIndex;
        structHeaps     = Array.Empty<StructHeap>();
        heapMap         = EntityStoreBase.Static.DefaultHeapMap; // all items are always null
        key             = new ArchetypeKey(this);
        // entityIds        = null      // stores no entities
        // entityCapacity   = 0         // stores no entities
        // shrinkThreshold  = 0         // stores no entities - will not shrink
        // componentCount   = 0         // has no components
        // structs          = default   // has no components
        // tags             = default   // has no tags
    }
    
    /// <summary>
    /// Note!: Ensure constructor cannot throw exceptions to eliminate <see cref="TypeInitializationException"/>'s
    /// </summary>
    private Archetype(in ArchetypeConfig config, StructHeap[] heaps, in Tags tags)
    {
        memory.capacity         = ChunkSize;
        memory.shrinkThreshold  = -1;
        memory.chunkCount       = 1;
        memory.chunkLength      = 1;
        store           = config.store;
        entityStore     = store as EntityStore;
        archIndex       = config.archetypeIndex;
        structCount     = heaps.Length;
        structHeaps     = heaps;
        entityIds       = new int [1];
        heapMap         = new StructHeap[config.maxStructIndex];
        structs         = new ArchetypeStructs(heaps);
        this.tags       = tags;
        key             = new ArchetypeKey(this);
        for (int pos = 0; pos < structCount; pos++)
        {
            var heap = heaps[pos];
            heap.SetArchetypeDebug(this);
            heapMap[heap.structIndex] = heap;
            SetStandardComponentHeaps(heap, ref std);
        }
    }
    
    private static void SetStandardComponentHeaps(StructHeap heap, ref StandardComponents std)
    {
        var type = heap.StructType;
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

    /// <remarks>Is called by methods using generic component type: T1, T2, T3, ...</remarks>
    internal static Archetype CreateWithSignatureTypes(in ArchetypeConfig config, in SignatureIndexes indexes, in Tags tags)
    {
        var length          = indexes.length;
        var componentHeaps  = new StructHeap[length];
        var components      = EntityStoreBase.Static.ComponentSchema.Components;
        for (int n = 0; n < length; n++) {
            var structIndex   = indexes.GetStructIndex(n);
            var structType    = components[structIndex];
            componentHeaps[n] = structType.CreateHeap();
        }
        return new Archetype(config, componentHeaps, tags);
    }
    
    /// <remarks>
    /// Is called by methods using a set of arbitrary struct <see cref="SchemaType"/>'s.<br/>
    /// Using a <see cref="List{T}"/> of types is okay. Method is only called for missing <see cref="Archetype"/>'s
    /// </remarks>
    internal static Archetype CreateWithStructTypes(in ArchetypeConfig config, List<ComponentType> structTypes, in Tags tags)
    {
        var length          = structTypes.Count;
        var componentHeaps  = new StructHeap[length];
        for (int n = 0; n < length; n++) {
            componentHeaps[n] = structTypes[n].CreateHeap();
        }
        return new Archetype(config, componentHeaps, tags);
    }
    #endregion

#region public methods
    // todo for solution using generic component type T. Therefore add method to StructHeap 
    public object GetEntityComponent(Entity entity, ComponentType componentType)
    {
        // => ref ((StructHeap<T>)archetype.heapMap[StructHeap<T>.StructIndex]).chunks[compIndex / ChunkSize].components[compIndex % ChunkSize];
        // heapMap[componentType.structIndex].chunks[entity.compIndex / ChunkSize].components[entity.compIndex % ChunkSize];
        return heapMap[componentType.structIndex].GetComponentDebug(entity.compIndex);
    }
    #endregion
    
#region component handling

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
            if (entityCount > memory.shrinkThreshold) {
                return;
            }
            CheckChunkCapacity();
            return;
        }
        // --- move components of last entity to the index where the entity is currently placed to avoid unused entries
        foreach (var heap in structHeaps) {
            heap.MoveComponent(lastIndex, newIndex);
        }
        var lastEntityId    = entityIds[lastIndex];
        store.UpdateEntityCompIndex(lastEntityId, newIndex); // set entity component index for new archetype
        
        entityIds[newIndex] = lastEntityId;
        entityCount--;      // remove last entity id
        if (entityCount > memory.shrinkThreshold) {
            return;
        }
        CheckChunkCapacity();
    }
    
    internal int AddEntity(int id)
    {
        var index = entityCount++;
        if (index == entityIds.Length) {
            Utils.Resize(ref entityIds, 2 * entityIds.Length);
        }
        entityIds[index] = id;  // add entity id
        if (index < memory.capacity) {
            return index;
        }
        CheckChunkCapacity();
        return index;
    }
    
    private void CheckChunkCapacity()
    {
        //  newChunkCount(entityCount)  [   0,  512] -> 1
        //                              [ 513, 1024] -> 2
        //                              [1025, 1536] -> 3
        var newChunkCount   = (entityCount - 1) / ChunkSize + 1;
        var chunkCount      = memory.chunkCount;
        if (newChunkCount > chunkCount)
        {
            int newChunkLength = memory.chunkLength;
            // --- double Length of chunks array if needed
            if (newChunkCount > memory.chunkLength) {
                newChunkLength *= 2;
            }
            SetChunkCapacity(newChunkCount, chunkCount, newChunkLength);
            return;
        }
        if (newChunkCount < chunkCount)
        {
            // --- halve Length of chunks array if newChunkCount is significant less than (1/4) of current chunks length 
            if (newChunkCount <=  memory.chunkLength / 4)
            {
                int newChunkLength  = memory.chunkLength / 2;
                // Create new chunks array with half the Length of the current one.
                // Copy newChunkLength component buffers from current to new one.
                SetChunkCapacity(newChunkLength, newChunkLength, newChunkLength);
            }
        }
    }
    
    private void SetChunkCapacity(int newChunkCount, int chunkCount, int newChunkLength) {
        foreach (var heap in structHeaps) {
            heap.SetChunkCapacity(newChunkCount, chunkCount, newChunkLength, memory.chunkLength);
        }
        memory.chunkCount       = newChunkCount;
        memory.chunkLength      = newChunkLength;
        memory.capacity         = newChunkCount * ChunkSize;        // 512, 1024, 1536, 2048, ...
        memory.shrinkThreshold  = memory.capacity - ChunkSize * 2;  // -512, 0, 512, 1024, ...
    }
    #endregion
    
#region internal methods
    internal static int GetEntityCount(ReadOnlySpan<Archetype> archetypes)
    {
        int count = 0;
        foreach (var archetype in archetypes) {
            count += archetype.entityCount;
        }
        return count;
    }
    
    private string GetString() {
        var sb          = new StringBuilder();
        var hasTypes    = false;
        sb.Append('[');
        foreach (var heap in structHeaps) {
            sb.Append(heap.StructType.Name);
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
