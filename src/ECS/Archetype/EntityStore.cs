// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Utils;
using Friflo.Json.Fliox.Mapper;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable once CheckNamespace
// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Engine.ECS;

/// <summary>
/// Store the <see cref="IComponent"/>s and <see cref="ITag"/> for the <see cref="Entity"/>'s of an <see cref="EntityStore"/>.  
/// </summary>
/// <remarks>
/// <see cref="EntityStoreBase"/> is separated as a base from <see cref="EntityStore"/> as is can be used for<br/>
/// different entity store implementations like the <see cref="RawEntityStore"/>.
/// </remarks>
[CLSCompliant(true)]
public abstract partial class EntityStoreBase
{
#region public properties
    /// <summary>Number of all entities stored in the entity store</summary>
    [Browse(Never)] public              int                     Count           => entityCount;
    
    /// <summary> Obsolete. Renamed to <see cref="Count"/>. </summary>
    [Obsolete($"Renamed to {nameof(Count)}")]
    [Browse(Never)] public              int                     EntityCount     => entityCount;
    
    /// <summary> Initialize a default <see cref="ParallelJobRunner"/> used for <see cref="QueryJob"/>'s. </summary>
    [Browse(Never)] public              ParallelJobRunner       JobRunner       { get; init; }

    /// <summary>Array of <see cref="Archetype"/>'s utilized by the entity store</summary>
    /// <remarks>Each <see cref="Archetype"/> contains all entities of a specific combination of <b>struct</b> components.</remarks>
                    public ReadOnlySpan<Archetype>              Archetypes      => new (archs, 0, archsCount);
    
    /// <summary> Returns the current number of <see cref="Archetypes"/> managed by the entity store. </summary>
    [Browse(Never)] public              int                     ArchetypeCount  => archsCount;
    
    /// <summary>
    /// Shrink ratio threshold: <c> Sum of all Archetype capacities / EntityStore entity count</c>. Default: 10<br/>   
    /// If the current ratio is > <see cref="ShrinkRatioThreshold"/> archetype capacities are shrinked. 
    /// </summary>
    [Browse(Never)] public              double                  ShrinkRatioThreshold { get => internBase.shrinkRatio; set => SetShrinkRatio(value); }
    
    /// <summary> Return the sum of all Archetype capacities. </summary>
    [Browse(Never)] public              long                    CapacitySumArchetypes => internBase.archetypesCapacity;
    
    /// <summary>Return all <see cref="UniqueEntity"/>'s in the entity store </summary>
                    public              QueryEntities           UniqueEntities  => GetUniqueEntities();
    
    [Browse(Never)] internal     Action<ComponentChanged>       ComponentAdded  => internBase.componentAdded;
    [Browse(Never)] internal     Action<ComponentChanged>       ComponentRemoved=> internBase.componentRemoved;
    [Browse(Never)] internal     Action<TagsChanged>            TagsChanged     => internBase.tagsChanged;

                    public   override   string                  ToString()      => $"entities: {entityCount}";
    #endregion

#region events
    /// <summary>Add / remove an event handler for <see cref="ECS.TagsChanged"/> events triggered by:<br/>
    /// <see cref="Entity.AddTag{T}"/> <br/> <see cref="Entity.AddTags"/> <br/> <see cref="Entity.RemoveTag{T}"/> <br/> <see cref="Entity.RemoveTags"/>.</summary>
    public event    Action<TagsChanged>       OnTagsChanged      { add => internBase.tagsChanged      += value; remove => internBase.tagsChanged      -= value; }
    
    /// <summary>Add / remove an event handler for <see cref="ECS.ComponentChanged"/> events triggered by: <br/>
    /// <see cref="Entity.AddComponent{T}()"/>.</summary>
    public event    Action<ComponentChanged>  OnComponentAdded   { add => internBase.componentAdded   += value; remove => internBase.componentAdded   -= value; }
    
    /// <summary>Add / remove an event handler for <see cref="ECS.ComponentChanged"/> events triggered by: <br/>
    /// <see cref="Entity.RemoveComponent{T}()"/>.</summary>
    public event    Action<ComponentChanged>  OnComponentRemoved { add => internBase.componentRemoved += value; remove => internBase.componentRemoved -= value; }
    #endregion
    
#region private / internal fields
    // --- archetypes
    [Browse(Never)] internal            Archetype[]             archs;              //  8   - array of all archetypes. never null
    [Browse(Never)] private             int                     archsCount;         //  4   - number of archetypes
    [Browse(Never)] private  readonly   HashSet<ArchetypeKey>   archSet;            //  8   - Set<> to get archetypes by key
    /// <summary>The default <see cref="Archetype"/> has no <see cref="Archetype.ComponentTypes"/> and <see cref="Archetype.Tags"/>.<br/>
    /// Its <see cref="Archetype"/>.<see cref="Archetype.archIndex"/> is always 0 (<see cref="Static.DefaultArchIndex"/>).</summary>
    [Browse(Never)] internal readonly   Archetype               defaultArchetype;   //  8   - default archetype. has no components & tags
    // --- nodes
    [Browse(Never)] internal            int                     entityCount;        //  4   - number of all entities
    // --- misc
    [Browse(Never)] private  readonly   ArchetypeKey            searchKey;          //  8   - key buffer to find archetypes by key
    [Browse(Never)] internal readonly   int[]                   singleIds;          //  8
    [Browse(Never)] internal            int                     singleIndex;        //  4
    [Browse(Never)] internal            bool                    shrinkArchetypes;   //  1
    
                    internal            InternBase              internBase;         // 88
    /// <summary>Contains state of <see cref="EntityStoreBase"/> not relevant for application development.</summary>
    /// <remarks>Declaring internal state fields in this struct remove noise in debugger.</remarks>
    internal struct InternBase {
        internal        long                                        archetypesCapacity;     // 16   - sum of all Archetype capacities
        internal        double                                      shrinkRatio;            //  8
        // --- delegates
        internal        Action                <TagsChanged>         tagsChanged;            //  8   - fires event if entity Tags are changed
        internal        Dictionary<int, Action<TagsChanged>>        entityTagsChanged;      //  8   - entity event handlers for add/remove Tags
        //
        internal        Action                <ComponentChanged>    componentAdded;         //  8   - fires event on add component
        internal        Action                <ComponentChanged>    componentRemoved;       //  8   - fires event on remove component
        internal        Dictionary<int, Action<ComponentChanged>>   entityComponentChanged; //  8   - entity event handlers for add/remove component
        //
        ///  reused query for <see cref="EntityStoreBase.GetUniqueEntity"/>
        internal        ArchetypeQuery<UniqueEntity>                uniqueEntityQuery;      //  8
        internal        StackArray<EntityBatch>                     entityBatches;          //  8
        internal        StackArray<CreateEntityBatch>               createEntityBatches;    //  8
        internal        StackArray<EntityList>                      entityLists;            //  8
    }
    #endregion
    
#region static fields
    // use nested class to minimize noise in debugger
    internal static class Static
    {
        internal static readonly    TypeStore       TypeStore       = new TypeStore();
        internal static readonly    EntitySchema    EntitySchema    = SchemaUtils.RegisterSchemaTypes(TypeStore);
        /// <summary>All items in the <see cref="DefaultHeapMap"/> are always null</summary>
        internal static readonly    StructHeap[]    DefaultHeapMap  = new StructHeap[EntitySchema.maxStructIndex];
        
        /// <summary>The index of the <see cref="EntityStoreBase.defaultArchetype"/> - index is always 0</summary>
        internal const              int             DefaultArchIndex        =  0;
        
        /// <summary>to avoid accidental entity access by id using (default value) 0 </summary>
        internal const              int             MinNodeId               =  1;
    //  /// <summary>
    //  /// A <see cref="EntityNode"/> with <see cref="Entity.Parent"/> == null
    //  /// is declared as <see cref="TreeMembership.floating"/>.
    //  /// </summary>
        internal const              int             NoParentId              =  0;
    //  internal const              int             StoreRootParentId       = -1;
    
        internal const              int             SingleMax               = 32;
    }
    #endregion
    
#region initialize
    protected EntityStoreBase()
    {
        archs               = new Archetype[2];
        archSet             = new HashSet<ArchetypeKey>(ArchetypeKeyEqualityComparer.Instance);
        var config          = GetArchetypeConfig(this);
        defaultArchetype    = new Archetype(config);
        searchKey           = new ArchetypeKey();
        AddArchetype(this, defaultArchetype);
        internBase.shrinkRatio          = 10;
        internBase.entityBatches        = new StackArray<EntityBatch>       (Array.Empty<EntityBatch>());
        internBase.createEntityBatches  = new StackArray<CreateEntityBatch> (Array.Empty<CreateEntityBatch>());
        internBase.entityLists          = new StackArray<EntityList>        (Array.Empty<EntityList>());
        singleIds                       = new int[Static.SingleMax];
    }
    #endregion
    
    protected internal abstract void    UpdateEntityCompIndex(int id, int compIndex);
    
#region exceptions
    /// <summary>
    /// Note: Should be called only in case the entity null check is negligible compared to total method execution.
    /// </summary>
    internal static NullReferenceException EntityNullException(Entity entity) {
        return new NullReferenceException($"entity is null. id: {entity.Id}");
    }
    
    internal static ArgumentException EntityArgumentNullException(Entity entity, string param) {
        return ExceptionUtils.ArgumentNullException($"entity is null. id: {entity.Id}", param);
    }
    
    internal static Exception   InvalidStoreException(string parameterName) {
        return ExceptionUtils.ArgumentException("entity is owned by a different store", parameterName);
    }
        
    internal static Exception   InvalidEntityIdException(int id, string parameterName) {
        return ExceptionUtils.ArgumentException($"invalid entity id <= 0. was: {id}", parameterName);
    }
        
    internal static Exception   IdAlreadyInUseException(int id, string parameterName) {
        return ExceptionUtils.ArgumentException($"id already in use in EntityStore. id: {id}", parameterName);
    }
    
    internal static Exception   PidOutOfRangeException(long pid, string parameterName) {
        var msg = $"pid must be in range [1, 2147483647] when using {nameof(PidType)}.{nameof(PidType.UsePidAsId)}. was: {pid}";
        return ExceptionUtils.ArgumentException(msg, parameterName);
    }
    
    internal static ArgumentException IdOutOfRangeException(EntityStore store, int id) {
        return new ArgumentException($"id: {id}. expect in [0, current max id: {store.nodes.Length - 1}]");
    }
    
    internal static ArgumentException AddRelationException(int id, int structIndex) {
        var componentType   = Static.EntitySchema.components[structIndex];
        var type            = componentType.Name;
        return new ArgumentException($"relation component must be added with:  entity.{nameof(RelationExtensions.AddRelation)}(new {type}());  id: {id}");
    }
    
    internal static ArgumentException RemoveRelationException(int id, int structIndex) {
        var componentType   = Static.EntitySchema.components[structIndex];
        var type            = componentType.Name;
        var keyType         = componentType.RelationKeyType.Name;
        return new ArgumentException($"relation component must be removed with:  entity.{nameof(RelationExtensions.RemoveRelation)}<{type},{keyType}>(key);  id: {id}");
    }
    #endregion
}

public static partial class EntityStoreExtensions
{
    /// <summary>
    /// Safe alternative for unsafe variant using <see cref="System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan{T}"/>
    /// to create a span of a single item. <br/>
    /// The general problem of this approach, the item is typically on the stack. <br/>
    /// So it's easy to create code with access violation that reference the span item that is not on the stack anymore.  
    /// </summary>
    internal static ReadOnlySpan<int> GetSpanId(this EntityStoreBase store, int id)
    {
        var ids             = store.singleIds;
        var index           = store.singleIndex;
        store.singleIndex   = (index + 1) % EntityStoreBase.Static.SingleMax;
        ids[index]  = id;
        return new ReadOnlySpan<int>(ids, index, 1);
    }
}
