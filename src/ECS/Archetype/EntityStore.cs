// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.Client;
using Friflo.Json.Fliox.Mapper;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
using LocalEntities = Friflo.Json.Fliox.Hub.Client.LocalEntities<long, Friflo.Fliox.Engine.Client.DataNode>;

[assembly: CLSCompliant(true)]

// Hard rule: this file MUST NOT access GameEntity's

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[CLSCompliant(true)]
public abstract partial class EntityStore
{
#region public properties
    /// <summary>Number of all entities stored in the <see cref="EntityStore"/></summary>
                    public              int                     EntityCount         => nodesCount;
                    public              int                     NodeMaxId           => nodesMaxId;

    /// <summary>Array of <see cref="Archetype"/>'s utilized by the <see cref="EntityStore"/></summary>
    /// <remarks>Each <see cref="Archetype"/> contains all entities of a specific combination of <b>struct</b> components.</remarks>
                    public              ReadOnlySpan<Archetype> Archetypes          => new (archs, 0, archsCount);
    
                    public  static      ComponentSchema         GetComponentSchema()=> Static.ComponentSchema;
                    public  override    string                  ToString()          => $"Count: {nodesCount}";
    #endregion
    
#region private / internal fields
    // --- archetypes
    [Browse(Never)] protected           Archetype[]             archs;              //  8 + archetypes      - array of all archetypes. never null
    [Browse(Never)] internal            int                     archsCount;         //  4                   - number of archetypes
    [Browse(Never)] internal readonly   HashSet<ArchetypeKey>   archSet;            //  8 + Set<Key>'s      - Set<> to get archetypes by key
    /// <summary>The default <see cref="Archetype"/> has no <see cref="Archetype.Structs"/> and <see cref="Archetype.Tags"/>.<br/>
    /// Its <see cref="Archetype"/>.<see cref="Archetype.archIndex"/> is always 0 (<see cref="Static.DefaultArchIndex"/>).</summary>
    [Browse(Never)] internal readonly   Archetype               defaultArchetype;   //  8                   - default archetype. has no struct components & tags
    // --- nodes
    [Browse(Never)] protected           int                     nodesMaxId;         //  4                   - highest entity id
    [Browse(Never)] protected           int                     nodesCount;         //  4                   - number of all entities
                    protected           int                     sequenceId;         //  4                   - incrementing id used for next new EntityNode
    // --- misc
    [Browse(Never)] protected readonly  LocalEntities           clientNodes;        //  8 Map<pid,DataNode> - client used to persist entities
    [Browse(Never)] private   readonly  ArchetypeKey            searchKey;          //  8 (+76)             - key buffer to find archetypes by key
    #endregion
    
#region static fields
    public static class Static {
        internal static readonly    int[]           EmptyChildNodes = null;
        internal static readonly    TypeStore       TypeStore       = new TypeStore();
        internal static readonly    ComponentSchema ComponentSchema = ComponentUtils.RegisterComponentTypes(TypeStore);
        /// <summary>All items in the <see cref="DefaultHeapMap"/> are always null</summary>
        internal static readonly    StructHeap[]    DefaultHeapMap  = new StructHeap[ComponentSchema.maxStructIndex];
        
        /// <summary>The index of the <see cref="EntityStore.defaultArchetype"/> - this index always 0</summary>
        internal const              int             DefaultArchIndex    =  0;
        
        internal const              int             DefaultCapacity     =  1;
        
        /// <summary>to avoid accidental entity access by id using (default value) 0 </summary>
        internal const              int             MinNodeId           =  1;
        /// <summary>
        /// An <see cref="EntityNode"/> with <see cref="EntityNode.parentId"/> == <see cref="NoParentId"/>
        /// is declared as <see cref="TreeMembership.floating"/>.
        /// </summary>
        public   const              int             NoParentId          =  0;
        public   const              int             RootId              = -1;
    }
    #endregion
    
#region initialize
    protected EntityStore(SceneClient client = null)
    {
        sequenceId          = Static.MinNodeId;
        archs               = new Archetype[2];
        archSet             = new HashSet<ArchetypeKey>(ArchetypeKeyEqualityComparer.Instance);
        var config          = GetArchetypeConfig();
        defaultArchetype    = new Archetype(config);
        clientNodes         = client?.nodes.Local;
        searchKey           = new ArchetypeKey();
        AddArchetype(defaultArchetype);
    }
    #endregion

#region exceptions
    internal static Exception InvalidStoreException(string parameterName) {
        return new ArgumentException("entity is owned by a different store", parameterName);
    }
        
    internal static Exception InvalidEntityIdException(int id, string parameterName) {
        return new ArgumentException($"invalid node id <= 0. was: {id}", parameterName);
    }
        
    internal static Exception IdAlreadyInUseException(int id, string parameterName) {
        return new ArgumentException($"id already in use in EntityStore. id: {id}", parameterName);
    }
    #endregion

}
