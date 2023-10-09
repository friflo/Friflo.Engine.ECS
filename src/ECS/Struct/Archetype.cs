// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed class Archetype
{
#region public properties / fields
    /// <summary>Number of entities stored in the <see cref="Archetype"/></summary>
    [Browse(Never)] public              int                         EntityCount     => entityCount;
    
    /// <summary>The entity ids store in the <see cref="Archetype"/></summary>
                    public              ReadOnlySpan<int>           EntityIds       => new (entityIds, 0, entityCount);
    
                    public              EntityStore                 Store           => store;
                    
    [Browse(Never)] public readonly     ArchetypeMask               mask;
    #endregion
    
#region internal members
                    private   readonly  StructHeap[]                structHeaps;    // Length = number of component types
    /// Store the entity id for each component. 
    [Browse(Never)] private             int[]                       entityIds;      // could use a StructHeap<int> if needed
    [Browse(Never)] private             int                         entityCount;
                    private             int                         capacity;
    
    // --- internal
    /// <remarks>
    /// Lookups on <see cref="heapMap"/> with <see cref="StructHeap.structIndex"/> or <see cref="StructHeap{T}.StructIndex"/>
    /// does not require a range check. This is already ensured at <see cref="ComponentSchema.GetStructType"/>
    /// </remarks>
    [Browse(Never)] internal readonly   StructHeap[]                heapMap;
    [Browse(Never)] internal readonly   EntityStore                 store;
    [Browse(Never)] internal readonly   int                         archIndex;
    [Browse(Never)] internal readonly   int                         componentCount; // number of component types
    [Browse(Never)] internal readonly   long                        typeHash;
                    internal readonly   StandardComponents          std;
    
    [Browse(Never)] internal readonly   Tags                        tags;
    
    [Browse(Never)] internal            ReadOnlySpan<StructHeap>    Heaps           => structHeaps;
    
                    public override     string                      ToString()      => GetString();
    #endregion
    
#region initialize
    /// <summary>
    /// Note!: Ensure constructor cannot throw exceptions to eliminate <see cref="TypeInitializationException"/>'s
    /// </summary>
    private Archetype(in ArchetypeConfig config, StructHeap[] heaps, in Tags tags)
    {
        store           = config.store;
        archIndex       = config.archetypeIndex;
        capacity        = config.capacity;
        componentCount  = heaps.Length;
        typeHash        = EntityStore.GetHash(heaps);
        structHeaps     = new StructHeap[componentCount];
        entityIds       = new int [1];
        heapMap         = new StructHeap[config.maxStructIndex];
        mask            = new ArchetypeMask(heaps);
        this.tags       = tags;
        foreach (var heap in heaps) {
            SetStandardComponentHeaps(heap, ref std);
        }
        int pos = 0;
        foreach (var heap in heaps)
        {
            heap.SetArchetype(this);
            structHeaps[pos++]          = heap;
            heapMap[heap.structIndex]   = heap;
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
    internal static Archetype CreateWithSignatureTypes(in ArchetypeConfig config, in SignatureTypeSet types, in Tags tags)
    {
        var length          = types.Length;
        var componentHeaps  = new StructHeap[length];
        for (int n = 0; n < length; n++) {
            componentHeaps[n] = types[n].CreateHeap(config.capacity);
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

    internal int MoveEntityTo(int id, int compIndex, Archetype newArchetype, ComponentUpdater updater)
    {
        var sourceIndex = compIndex;
        // --- copy entity components to components of new newArchetype
        var targetIndex = newArchetype.AddEntity(id);
        
        for (int n = 0; n < structHeaps.Length; n++) {
            var sourceHeap  = structHeaps[n];
            var targetHeap  = newArchetype.heapMap[sourceHeap.structIndex];
            if (targetHeap == null) {
                continue;
            }
            sourceHeap.CopyComponentTo(sourceIndex, targetHeap, targetIndex);
        }
        MoveLastComponentsTo(sourceIndex, updater);
        return targetIndex;
    }
    
    internal void MoveLastComponentsTo(int newIndex, ComponentUpdater updater)
    {
        var lastIndex = entityCount - 1;
        // --- clear entityMap if the entity is the only one in entityMap 
        if (lastIndex == 0) {
            entityCount = 0;
            return;
        }
        // --- move components of last entity to the index where the entity is currently placed to avoid unused entries
        foreach (var heap in structHeaps) {
            heap.MoveComponent(lastIndex, newIndex);
        }
        var lastEntityId    = entityIds[lastIndex];
        updater.UpdateComponentIndex(store, lastEntityId, newIndex);
        entityIds[newIndex]  = lastEntityId;
        entityCount--;     // remove last entity id
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
        var sb = new StringBuilder();
        if (structHeaps.Length == 0) {
            sb.Append("[]");    
        } else {
            sb.Append('[');
            foreach (var heap in structHeaps) {
                sb.Append(heap.type.Name);
                sb.Append(", ");
            }
            sb.Length -= 2;
            sb.Append("]  Count: ");
            sb.Append(entityCount);
        }
        return sb.ToString();
    }
    #endregion
}
