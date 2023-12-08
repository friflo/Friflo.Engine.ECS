// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Json.Fliox.Mapper;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

[assembly: CLSCompliant(true)]

// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable once CheckNamespace
// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.ECS;

[CLSCompliant(true)]
public abstract partial class EntityStoreBase
{
#region public properties
    /// <summary>Number of all entities stored in the entity store</summary>
                    public              int                     EntityCount         => nodesCount;
                    public              int                     NodeMaxId           => nodesMaxId;

    /// <summary>Array of <see cref="Archetype"/>'s utilized by the entity store</summary>
    /// <remarks>Each <see cref="Archetype"/> contains all entities of a specific combination of <b>struct</b> components.</remarks>
                    public ReadOnlySpan<Archetype>              Archetypes          => new (archs, 0, archsCount);
    [Browse(Never)] public              int                     ArchetypeCount      => archsCount;
                    public   override   string                  ToString()          => $"Count: {nodesCount}";
    #endregion

#region event handler
    // --- tags: changed
    public  TagsChangedHandler      TagsChanged         { get => tagsChanged;       set => tagsChanged      = value; }
    
    // --- component: added / removed
    public  ComponentChangedHandler ComponentAdded      { get => componentAdded;    set => componentAdded   = value; }
    public  ComponentChangedHandler ComponentRemoved    { get => componentRemoved;  set => componentRemoved = value; }


    #endregion
    
#region private / internal fields
    // --- archetypes
    [Browse(Never)] protected           Archetype[]             archs;              //  8 + archetypes      - array of all archetypes. never null
    [Browse(Never)] private             int                     archsCount;         //  4                   - number of archetypes
    [Browse(Never)] private  readonly   HashSet<ArchetypeKey>   archSet;            //  8 + Set<Key>'s      - Set<> to get archetypes by key
    /// <summary>The default <see cref="Archetype"/> has no <see cref="Archetype.ComponentTypes"/> and <see cref="Archetype.Tags"/>.<br/>
    /// Its <see cref="Archetype"/>.<see cref="Archetype.archIndex"/> is always 0 (<see cref="Static.DefaultArchIndex"/>).</summary>
    [Browse(Never)] internal readonly   Archetype               defaultArchetype;   //  8                   - default archetype. has no components & tags
    // --- nodes
    [Browse(Never)] protected           int                     nodesMaxId;         //  4                   - highest entity id
    [Browse(Never)] protected           int                     nodesCount;         //  4                   - number of all entities
                    protected           int                     sequenceId;         //  4                   - incrementing id used for next new entity
    // --- delegates
    [Browse(Never)] private             TagsChangedHandler      tagsChanged;        //  8
    //
    [Browse(Never)] private             ComponentChangedHandler componentAdded;     //  8
    [Browse(Never)] private             ComponentChangedHandler componentRemoved;   //  8
    // --- misc
    [Browse(Never)] private   readonly  ArchetypeKey            searchKey;          //  8 (+76)             - key buffer to find archetypes by key
    #endregion
    
#region static fields
    public static class Static {
        internal static readonly    int[]           EmptyChildNodes = null;
        internal static readonly    TypeStore       TypeStore       = new TypeStore();
        internal static readonly    EntitySchema    EntitySchema    = SchemaUtils.RegisterSchemaTypes(TypeStore);
        /// <summary>All items in the <see cref="DefaultHeapMap"/> are always null</summary>
        internal static readonly    StructHeap[]    DefaultHeapMap  = new StructHeap[EntitySchema.maxStructIndex];
        
        /// <summary>The index of the <see cref="EntityStoreBase.defaultArchetype"/> - index is always 0</summary>
        internal const              int             DefaultArchIndex        =  0;
        
        /// <summary>to avoid accidental entity access by id using (default value) 0 </summary>
        internal const              int             MinNodeId               =  1;
        /// <summary>
        /// An <see cref="EntityNode"/> with <see cref="EntityNode.parentId"/> == <see cref="NoParentId"/>
        /// is declared as <see cref="TreeMembership.floating"/>.
        /// </summary>
        public   const              int             NoParentId              =  0;
        public   const              int             StoreRootParentId       = -1;
    }
    #endregion
    
#region initialize
    protected EntityStoreBase()
    {
        sequenceId          = Static.MinNodeId;
        archs               = new Archetype[2];
        archSet             = new HashSet<ArchetypeKey>(ArchetypeKeyEqualityComparer.Instance);
        var config          = GetArchetypeConfig();
        defaultArchetype    = new Archetype(config);
        searchKey           = new ArchetypeKey();
        AddArchetype(defaultArchetype);
    }
    
    protected internal abstract void UpdateEntityCompIndex(int id, int compIndex); 
    
    #endregion
    
#region exceptions
    public static Exception InvalidStoreException(string parameterName) {
        return new ArgumentException("entity is owned by a different store", parameterName);
    }
        
    internal static Exception InvalidEntityIdException(int id, string parameterName) {
        return new ArgumentException($"invalid entity id <= 0. was: {id}", parameterName);
    }
        
    internal static Exception IdAlreadyInUseException(int id, string parameterName) {
        return new ArgumentException($"id already in use in EntityStore. id: {id}", parameterName);
    }
    
    internal static Exception PidOutOfRangeException(long pid, string parameterName) {
        var msg = $"pid must be in range [1, 2147483647] when using {nameof(PidType)}.{nameof(PidType.UsePidAsId)}. was: {pid}";
        return new ArgumentException(msg, parameterName);
    }
    
    internal static Exception AddEntityAsChildToItselfException(int id) {
        return new InvalidOperationException($"Cannot add entity to itself as a child. id: {id}");
    }
    #endregion

}
