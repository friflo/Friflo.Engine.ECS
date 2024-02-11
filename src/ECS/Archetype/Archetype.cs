// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// An <see cref="Archetype"/> store entities with a specific set of <see cref="IComponent"/> and <see cref="ITag"/> types.
/// </summary>
/// <remarks>
/// E.g. all entities with a <see cref="Position"/> and <see cref="Rotation"/> component are store in the same archetype.<br/>
/// In case of removing one of these components or adding a new one from / to an <see cref="Entity"/> the entity is moved to a different archetype.<br/>
/// <br/>
/// This is the basic pattern for an archetype base ECS. This approach enables efficient entity / component queries.<br/>
/// A query result is simply the union of all archetypes having the requested components.<br/>
/// <br/>
/// Queries can be created via generic <see cref="EntityStoreBase"/>.<c>Query()</c> methods.<br/>
/// </remarks>
public sealed class Archetype
{
#region     public properties
    /// <summary>Number of entities / components stored in the <see cref="Archetype"/></summary>
    [Browse(Never)] public              int                 EntityCount     => entityCount;
    
    /// <summary>Number of <see cref="ComponentTypes"/> managed by the archetype.</summary>
    [Browse(Never)] public              int                 ComponentCount  => componentCount;
    
    /// <summary>The current capacity reserved to store entity components.</summary>
                    public              int                 Capacity        => memory.capacity;

    /// <summary>The list of entity ids stored in the archetype.</summary>
                    public              ReadOnlySpan<int>   EntityIds       => new (entityIds, 0, entityCount);
    
    /// <summary>The <see cref="EntityStore"/> owning the archetype.</summary>
                    public              EntityStoreBase     Store           => store;
    
    /// <summary>The <see cref="IComponent"/> types managed by the archetype.</summary>
                    public ref readonly ComponentTypes      ComponentTypes  => ref componentTypes;
    
    /// <summary>The <see cref="ITag"/> types managed by the archetype.</summary>
                    public ref readonly Tags                Tags            => ref tags;
    
    /// <summary>Return all <see cref="Entity"/>'s stored in the <see cref="Archetype"/>.</summary>
    /// <remarks>Property is mainly used for debugging.<br/>
    /// For efficient access to entity <see cref="IComponent"/>'s use one of the generic <b><c>EntityStore.Query()</c></b> methods. </remarks>
                    public              QueryEntities       Entities        => GetEntities();
                    
                    public   override   string              ToString()      => GetString();
#endregion

#region     private / internal members
                    internal readonly   StructHeap[]        structHeaps;    //  8 + all archetype components (struct heaps * componentCount)
    /// Store the entity id for each component.
    [Browse(Never)] internal            int[]               entityIds;      //  8 + ids - could use a StructHeap<int> if needed
    [Browse(Never)] internal            int                 entityCount;    //  4       - number of entities in archetype
    [Browse(Never)] private             ArchetypeMemory     memory;         // 16       - count & length used to store components in chunks  
    // --- internal
    [Browse(Never)] internal readonly   int                 componentCount; //  4       - number of component types
    [Browse(Never)] internal readonly   ComponentTypes      componentTypes; // 32       - component types of archetype
    [Browse(Never)] internal readonly   Tags                tags;           // 32       - tags assigned to archetype
    [Browse(Never)] internal readonly   ArchetypeKey        key;            //  8 (+76)
    /// <remarks>Lookups on <see cref="heapMap"/>[] does not require a range check. See <see cref="EntitySchema.CheckStructIndex"/></remarks>
    [Browse(Never)] internal readonly   StructHeap[]        heapMap;        //  8       - Length always = maxStructIndex. Used for heap lookup
    [Browse(Never)] internal readonly   EntityStoreBase     store;          //  8       - containing EntityStoreBase
    [Browse(Never)] internal readonly   EntityStore         entityStore;    //  8       - containing EntityStore
    [Browse(Never)] internal readonly   int                 archIndex;      //  4       - archetype index in EntityStore.archs[]
    [Browse(Never)] internal readonly   StandardComponents  std;            // 32       - heap references to std types: Position, Rotation, ...
    [Browse(Never)] private             ArchetypeQuery      query;          //  8
    #endregion
    
#region public methods
    /// <summary>
    /// Create an <see cref="Entity"/> with the <see cref="ComponentTypes"/> and <see cref="Tags"/> managed by the archetype.
    /// </summary>
    public Entity CreateEntity()
    {
        var entity          = entityStore.CreateEntity();
        entity.refArchetype = this;
        entity.refCompIndex = AddEntity(this, entity.Id);
        return entity;
    }
    
    /// <summary>
    /// Allocates memory for entity components in the archetype to enable adding entity components without reallocation.
    /// </summary>
    /// <returns>The number of entities that can be added without reallocation. </returns>
    public int EnsureCapacity(int capacity) {
        var available = memory.capacity - entityCount;
        if (capacity <= available) {
            return available;
        }
        var newCapacity = GetUpperPowerOfTwo(entityCount + capacity);
        Resize(this, newCapacity);
        return memory.capacity - entityCount;
    }
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
        // componentTypes   = default   // has no components
        // tags             = default   // has no tags
    }
    
    /// <summary>
    /// Note!: Ensure constructor cannot throw exceptions to eliminate <see cref="TypeInitializationException"/>'s
    /// </summary>
    private Archetype(in ArchetypeConfig config, StructHeap[] heaps, in Tags tags)
    {
        memory.capacity         = ArchetypeUtils.MinCapacity;
        memory.shrinkThreshold  = -1;
        store           = config.store;
        entityStore     = store as EntityStore;
        archIndex       = config.archetypeIndex;
        componentCount  = heaps.Length;
        structHeaps     = heaps;
        entityIds       = new int [memory.capacity];
        heapMap         = new StructHeap[config.maxStructIndex];
        componentTypes  = new ComponentTypes(heaps);
        this.tags       = tags;
        key             = new ArchetypeKey(this);
        for (int pos = 0; pos < componentCount; pos++)
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
        var components      = EntityStoreBase.Static.EntitySchema.components;
        for (int n = 0; n < length; n++) {
            var structIndex   = indexes.GetStructIndex(n);
            var componentType = components[structIndex];
            componentHeaps[n] = componentType.CreateHeap();
        }
        return new Archetype(config, componentHeaps, tags);
    }
    
    /// <remarks>
    /// Is called by methods using a set of arbitrary struct <see cref="ComponentType"/>'s.<br/>
    /// Using a <see cref="List{T}"/> of types is okay. Method is only called for missing <see cref="Archetype"/>'s
    /// </remarks>
    internal static Archetype CreateWithComponentTypeList(in ArchetypeConfig config, List<ComponentType> componentTypes, in Tags tags)
    {
        var length          = componentTypes.Count;
        var componentHeaps  = new StructHeap[length];
        for (int n = 0; n < length; n++) {
            componentHeaps[n] = componentTypes[n].CreateHeap();
        }
        return new Archetype(config, componentHeaps, tags);
    }
    
    internal static Archetype CreateWithComponentTypes(in ArchetypeConfig config, in ComponentTypes componentTypes, in Tags tags)
    {
        var length          = componentTypes.Count;
        var componentHeaps  = new StructHeap[length];
        int n = 0;
        foreach (var componentType in componentTypes) {
            componentHeaps[n++] = componentType.CreateHeap();
        }
        return new Archetype(config, componentHeaps, tags);
    }
    #endregion

    
#region component handling

    /// <remarks> the component index in the <paramref name="newArchetype"/> </remarks>
    internal static int MoveEntityTo(Archetype arch, int id, int sourceIndex, Archetype newArchetype)
    {
        // --- copy entity components to components of new newArchetype
        var targetIndex = AddEntity(newArchetype, id);
        foreach (var sourceHeap in arch.structHeaps)
        {
            var targetHeap = newArchetype.heapMap[sourceHeap.structIndex];
            if (targetHeap == null) {
                continue;
            }
            sourceHeap.CopyComponentTo(sourceIndex, targetHeap, targetIndex);
        }
        MoveLastComponentsTo(arch, sourceIndex);
        return targetIndex;
    }
    
    internal static void MoveLastComponentsTo(Archetype arch, int newIndex)
    {
        var lastIndex = arch.entityCount - 1;
        if (lastIndex != newIndex) {
            // --- move components of last entity to the index where the entity is currently placed to avoid unused entries
            foreach (var heap in arch.structHeaps) {
                heap.MoveComponent(lastIndex, newIndex);
            }
            var lastEntityId = arch.entityIds[lastIndex];
            arch.store.UpdateEntityCompIndex(lastEntityId, newIndex); // set entity component index for new archetype
        
            arch.entityIds[newIndex] = lastEntityId;
        }   // ReSharper disable once RedundantIfElseBlock
        else {
            // --- case: newIndex is already the last entity => only decrement entityCount
        }
        arch.entityCount = lastIndex;  // remove last entity id
        if (lastIndex > arch.memory.shrinkThreshold) {
            return;
        }
        ResizeShrink(arch);
    }
    
    /// <remarks>Must be used only on case all <see cref="ComponentTypes"/> are <see cref="ComponentType.IsBlittable"/></remarks>
    internal static void CopyComponents(Archetype arch, int sourceIndex, int targetIndex)
    {
        foreach (var sourceHeap in arch.structHeaps) {
            sourceHeap.CopyComponent(sourceIndex, targetIndex);
        }
    }
    
    /// <returns> the component index in this <see cref="Archetype"/> </returns>
    internal static int AddEntity(Archetype arch, int id)
    {
        var index =  arch.entityCount;
        if (index == arch.memory.capacity) {
            ResizeGrow(arch);
        }
        arch.entityCount = index + 1;
        arch.entityIds[index] = id;  // add entity id
        return index;
    }
    
    private static void ResizeGrow  (Archetype arch) => Resize(arch, 2 * arch.memory.capacity);
    private static void ResizeShrink(Archetype arch) => Resize(arch, 2 * arch.memory.shrinkThreshold);
    
    private static void Resize(Archetype arch, int capacity)
    {
        AssertCapacity(capacity);
        int shrinkThreshold = capacity / 4;
        if (shrinkThreshold < ArchetypeUtils.MinCapacity) {
            shrinkThreshold = -1;
        }
        arch.memory.shrinkThreshold = shrinkThreshold;
        arch.memory.capacity        = capacity;
        
        var count = arch.entityCount;
        ArrayUtils.Resize(ref arch.entityIds, capacity, count);
        foreach (var heap in arch.structHeaps) {
            heap.ResizeComponents(capacity, count);
        }
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage]
    private static void AssertCapacity(int capacity) {
        var multiple = capacity / ArchetypeUtils.MinCapacity;
        if (multiple * ArchetypeUtils.MinCapacity != capacity) {
            throw new InvalidOperationException($"invalid capacity. Expect multiple of: {ArchetypeUtils.MinCapacity} - was: {capacity}");
        }
    }
    
    #endregion
    
#region internal methods
    private QueryEntities GetEntities() {
        query ??= new ArchetypeQuery(store, componentTypes);
        return query.Entities;
    }
    
    private static int GetUpperPowerOfTwo(int value)
    {
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        value++;
        return value;
    }

    internal static int GetEntityCount(ReadOnlySpan<Archetype> archetypes)
    {
        int count = 0;
        foreach (var archetype in archetypes) {
            count += archetype.entityCount;
        }
        return count;
    }
    
    internal static int GetChunkCount(ReadOnlySpan<Archetype> archetypes)
    {
        int count = 0;
        foreach (var archetype in archetypes) {
            if (archetype.entityCount > 0) {
                count ++;    
            }
        }
        return count;
    }
    
    private string GetString() => AppendString(new StringBuilder()).ToString();
    
    internal StringBuilder AppendString(StringBuilder sb)
    {
        var hasTypes    = false;
        sb.Append('[');
        foreach (var heap in structHeaps) {
            sb.Append(heap.StructType.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        foreach (var tag in tags) {
            sb.Append('#');
            sb.Append(tag.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        if (hasTypes) {
            sb.Length -= 2;
            sb.Append(']');
            sb.Append("  entities: ");
            sb.Append(entityCount);
            return sb;
        }
        sb.Append(']');
        return sb;
    }
    #endregion
}

public static class ArchetypeUtils
{
    /// <summary> Minimum: 64 see <see cref="MaxComponentMultiple"/> to support padding for vectorization.</summary>
    /// <remarks> Could be less than 64 if using <see cref="ComponentType{T}.ByteSize"/> for <see cref="StructHeap{T}.components"/> </remarks>
    public   const  int     MinCapacity             = 512;
    
    /// <summary> Maximum number of components  </summary>
    internal const  int     MaxComponentMultiple     = 64;
}
