// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
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
#region public properties
    /// <summary>Number of entities stored in the <see cref="Archetype"/></summary>
    [Browse(Never)] public              int                         EntityCount     => entityCount;
    
    /// <summary>The entity ids store in the <see cref="Archetype"/></summary>
                    public              ReadOnlySpan<int>           EntityIds       => new (entityIds, 0, entityCount);
    
                    public              EntityStore                 Store           => store;
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
    /// does not require a range check. This is already ensured at <see cref="ComponentTypes.GetStructType"/>
    /// </remarks>
    [Browse(Never)] internal readonly   StructHeap[]                heapMap;
    [Browse(Never)] internal readonly   EntityStore                 store;
    [Browse(Never)] internal readonly   int                         archIndex;
    [Browse(Never)] internal readonly   int                         componentCount; // number of component types
    [Browse(Never)] internal readonly   long                        typeHash;
                    internal readonly   StandardComponents          std;
                    internal readonly   ArchetypeMask               mask;
    
    [Browse(Never)] internal            ReadOnlySpan<StructHeap>    Heaps           => structHeaps;
    
                    public override     string                      ToString()      => GetString();
    #endregion
    
#region initialize
    private Archetype(in ArchetypeConfig config, StructHeap[] heaps, StructHeap newComp)
    {
        store           = config.store;
        archIndex       = config.archetypeIndex;
        capacity        = config.capacity;
        componentCount  = heaps.Length + (newComp != null ? 1 : 0);
        typeHash        = EntityStore.GetHash(heaps, newComp);
        structHeaps     = new StructHeap[componentCount];
        entityIds       = new int [1];
        heapMap         = new StructHeap[config.maxStructIndex];
        mask            = new ArchetypeMask(heaps, newComp);
        if (newComp != null) {
            SetStandardComponentHeaps(newComp, ref std);
        }
        foreach (var heap in heaps) {
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

    internal static Archetype CreateFromArchetype(in ArchetypeConfig config, Archetype current, StructHeap newHeap)
    {
        var heaps = new StructHeap[current.structHeaps.Length]; 
        for (int n = 0; n < heaps.Length; n++) {
            // create same components used in the passed Archetype
            var heap    = current.structHeaps[n].CreateHeap(config.capacity, config.typeStore);
            heaps[n]    = heap;
        }
        var archetype = new Archetype(config, heaps, newHeap);
        int pos = 0;
        foreach (var component in heaps) {
            archetype.AddStructHeap(pos++, component);
        }
        archetype.AddStructHeap(pos, newHeap);
        return archetype;
    }
    
    internal static Archetype CreateWithStructTypes(in ArchetypeConfig config, ComponentType[] types)
    {
        var length          = types.Length;
        var componentHeaps  = new StructHeap[length];
        for (int n = 0; n < length; n++) {
            componentHeaps[n] = types[n].CreateHeap(config.capacity);
        }
        var archetype   = new Archetype(config, componentHeaps, null);
        int pos         = 0;
        foreach (var component in componentHeaps) {
            archetype.AddStructHeap(pos++, component);
        }
        return archetype;
    }
    #endregion
    
#region struct component handling
    private void AddStructHeap(int pos, StructHeap heap) {
#if DEBUG
        heap.archetype              = this;
#endif
        structHeaps[pos]            = heap;
        heapMap[heap.structIndex]   = heap;
    }
   
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
        updater.UpdateComponentIndex(lastEntityId, newIndex);
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
