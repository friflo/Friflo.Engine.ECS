// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Friflo.Engine.ECS.Relations;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable UseCollectionExpression
// ReSharper disable InlineTemporaryVariable
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// An <see cref="Archetype"/> store entities with a specific set of <see cref="IComponent"/> and <see cref="ITag"/> types.<br/>
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/general#archetype">Example.</a>
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
    [Browse(Never)] public              int                 Count           => entityCount;
    
    /// <summary> Obsolete. Renamed to <see cref="Count"/>. </summary>
    [Obsolete($"Renamed to {nameof(Count)}")]
    [Browse(Never)]
    public                              int                 EntityCount     => entityCount;
    
    [Browse(Never)] public              string              Name            => GetName();
    
    /// <summary>Number of <see cref="ComponentTypes"/> managed by the archetype.</summary>
    [Browse(Never)] public              int                 ComponentCount  => componentCount;
    
    /// <summary>The current capacity reserved to store entity components.</summary>
                    public              int                 Capacity        => memory.capacity;

    /// <summary>Return the entity ids stored in the archetype.</summary>
                    public              ReadOnlySpan<int>   EntityIds       => new (entityIds, 0, entityCount);
    
    /// <summary>Return the components of the specified <typeparamref name="TComponent"/> type stored in the archetype.</summary>
                    public              Span<TComponent>    Components<TComponent>() where TComponent : struct, IComponent
                        => new (((StructHeap<TComponent>)heapMap[StructInfo<TComponent>.Index]).components, 0, entityCount);
    
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
    [Browse(Never)] internal readonly   StructHeap[]        structHeaps;    //  8   - never null. archetype components
    /// Store the entity id for each component.
    [Browse(Never)] internal            int[]               entityIds;      //  8   - could use a StructHeap<int> if needed
    [Browse(Never)] internal            int                 entityCount;    //  4   - number of entities in archetype
    [Browse(Never)] private             ArchetypeMemory     memory;         // 16   - count & length used to store components in chunks  
    // --- internal
    [Browse(Never)] internal readonly   int                 componentCount; //  4   - number of component types
    [Browse(Never)] internal readonly   ComponentTypes      componentTypes; // 32   - component types of archetype
    [Browse(Never)] internal readonly   Tags                tags;           // 32   - tags assigned to archetype
    [Browse(Never)] internal readonly   ArchetypeKey        key;            //  8
    /// <remarks>Lookups on <see cref="heapMap"/>[] does not require a range check. See <see cref="EntitySchema.CheckStructIndex"/></remarks>
    [Browse(Never)] internal readonly   StructHeap[]        heapMap;        //  8   - never null. Length always = maxStructIndex. Used for heap lookup
    [Browse(Never)] internal readonly   EntityStoreBase     store;          //  8   - containing EntityStoreBase
    [Browse(Never)] internal readonly   EntityStore         entityStore;    //  8   - containing EntityStore
    [Browse(Never)] internal readonly   int                 archIndex;      //  4   - archetype index in EntityStore.archs[]
    [Browse(Never)] internal readonly   StandardComponents  std;            // 32   - heap references to std types: Position, Rotation, ...
    [Browse(Never)] private             ArchetypeQuery      query;          //  8   - return the entities of this archetype
    [Browse(Never)] internal            TypeCache           componentAdd;   // 24
    [Browse(Never)] internal            TypeCache           componentRemove;// 24
    #endregion
    
#region public methods
    /// <summary>
    /// Create an <see cref="Entity"/> with the <see cref="ComponentTypes"/> and <see cref="Tags"/> managed by the archetype.
    /// </summary>
    public Entity CreateEntity()
    {
        var localStore  = entityStore;
        var id          = localStore.NewId();
        var compIndex   = localStore.CreateEntityInternal(this, id, out var revision);
        foreach (var heap in structHeaps) {
            heap.SetComponentDefault(compIndex);
        }
        var entity = new Entity(localStore, id, revision);
        
        // Send event. See: SEND_EVENT notes
        localStore.CreateEntityEvent(entity);
        return entity;
    }
    
    public Entity CreateEntity(int id)
    {
        var localStore  = entityStore;
        localStore.CheckEntityId(id);
        var compIndex   = localStore.CreateEntityInternal(this, id, out var revision);
        foreach (var heap in structHeaps) {
            heap.SetComponentDefault(compIndex);
        }
        var entity = new Entity(localStore, id, revision);
        
        // Send event. See: SEND_EVENT notes
        localStore.CreateEntityEvent(entity);
        return entity;
    }
    
    public Entities CreateEntities(int count)
    {
        var localStore      = entityStore;
        int compIndexStart  = entityCount;
        localStore.CreateEntityNodes(this, count);
        
        foreach (var heap in structHeaps) {
            heap.SetComponentsDefault(compIndexStart, count);
        }
        // Send event. See: SEND_EVENT notes
        var entities = new Entities(localStore, entityIds, compIndexStart, count);
        localStore.CreateEntityEvents(entities);
        return entities;
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
        config.store.AddArchetypeCapacity(ArchetypeUtils.MinCapacity);
        memory.capacity         = ArchetypeUtils.MinCapacity;
        memory.shrinkThreshold  = -1;
        store           = config.store;
        entityStore     = store as EntityStore;
        archIndex       = EntityStoreBase.Static.DefaultArchIndex;
        structHeaps     = Array.Empty<StructHeap>();
        entityIds       = new int [memory.capacity];
        heapMap         = EntityStoreBase.Static.DefaultHeapMap; // all items are always null
        key             = new ArchetypeKey(this);
        // componentCount   = 0         // has no components
        // componentTypes   = default   // has no components
        // tags             = default   // has no tags
    }
    
    /// <summary> used by <see cref="EntityRelations"/> </summary>
    internal Archetype(in ArchetypeConfig config, StructHeap heap)
    {
        config.store.AddArchetypeCapacity(ArchetypeUtils.MinCapacity);
        memory.capacity = ArchetypeUtils.MinCapacity;
        memory.shrinkThreshold  = -1;
        store           = config.store;
        entityStore     = store as EntityStore;
        archIndex       = config.archetypeIndex;
        componentCount  = 1;
        structHeaps     = new [] { heap };
        entityIds       = new int [memory.capacity];
        heapMap         = new StructHeap[config.schema.maxStructIndex];
        componentTypes  = new ComponentTypes(structHeaps);
        key             = new ArchetypeKey(this);
        heapMap[heap.structIndex] = heap;
        heap.SetArchetypeDebug(this);
    }
    
    /// <summary>
    /// Note!: Ensure constructor cannot throw exceptions to eliminate <see cref="TypeInitializationException"/>'s
    /// </summary>
    private Archetype(in ArchetypeConfig config, StructHeap[] heaps, in Tags tags)
    {
        config.store.AddArchetypeCapacity(ArchetypeUtils.MinCapacity);
        memory.capacity = ArchetypeUtils.MinCapacity;
        memory.shrinkThreshold  = -1;
        store           = config.store;
        entityStore     = store as EntityStore;
        archIndex       = config.archetypeIndex;
        componentCount  = heaps.Length;
        structHeaps     = heaps;
        entityIds       = new int [memory.capacity];
        heapMap         = new StructHeap[config.schema.maxStructIndex];
        componentTypes  = new ComponentTypes(heaps);
        this.tags       = tags;
        key             = new ArchetypeKey(this);
        for (int pos = 0; pos < componentCount; pos++)
        {
            var heap    = heaps[pos];
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

    internal static Archetype CreateWithComponentTypes(
        in ArchetypeConfig  config,
        in ComponentTypes   componentTypes,
        in Tags             tags)
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

    /// <returns> the component index in the <paramref name="targetArch"/> </returns>
    internal static int MoveEntityTo(Archetype sourceArch, int id, int sourceIndex, Archetype targetArch)
    {
        if (sourceArch == targetArch) {
            return sourceIndex;
        }
        // --- copy entity components to components of targetArch
        var targetIndex     = AddEntity(targetArch, id);
        var sourceHeapMap   = sourceArch.heapMap;
        foreach (var targetHeap in targetArch.structHeaps)
        {
            var sourceHeap = sourceHeapMap[targetHeap.structIndex];
            if (sourceHeap != null) {
                // case: sourceArch and targetArch contain component type   => copy component to targetHeap.
                sourceHeap.CopyComponentTo(sourceIndex, targetHeap, targetIndex);
                continue;
            }
            // case: component type is no present in sourceArch     => set new component to default in targetHeap.
            // This is redundant for Entity.AddComponent() but other callers may not assign a component value.
            targetHeap.SetComponentDefault(targetIndex);
        }
        MoveLastComponentsTo(sourceArch, sourceIndex, true);
        return targetIndex;
    }
    
    internal static void MoveLastComponentsTo(Archetype arch, int newIndex, bool updateCompIndex)
    {
        var lastIndex   = arch.entityCount - 1;
        var store       = arch.store;
        if (lastIndex != newIndex) {
            // --- move components of last entity to the index where the entity is currently placed to avoid unused entries
            foreach (var heap in arch.structHeaps) {
                heap.MoveComponent(lastIndex, newIndex);
            }
            var entityIds       = arch.entityIds;
            var lastEntityId    = entityIds[lastIndex];
            if (updateCompIndex) {
                store.UpdateEntityCompIndex(lastEntityId, newIndex); // set entity component index for new archetype
            }
            entityIds[newIndex] = lastEntityId;
        }   // ReSharper disable once RedundantIfElseBlock
        else {
            // --- case: newIndex is already the last entity => only decrement entityCount
        }
        arch.entityCount = lastIndex;  // remove last entity id
        if (!store.shrinkArchetypes || lastIndex > arch.memory.shrinkThreshold) {
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
    
    /// <returns> the component index in <paramref name="arch"/> </returns>
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
        arch.store.AddArchetypeCapacity(capacity - arch.memory.capacity);
        
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
        query ??= new ArchetypeQuery(this);
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
    
    private string GetName() {
        var sb = new StringBuilder();
        AppendString(sb);
        return sb.ToString();
    }
    
    private string GetString() {
        var sb = new StringBuilder();
        AppendString(sb);
        sb.Append("  entities: ");
        sb.Append(entityCount);
        return sb.ToString();
    }

    internal void AppendString(StringBuilder sb)
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
        }
        sb.Append(']');
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
