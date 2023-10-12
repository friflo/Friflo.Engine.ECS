// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.Client;
using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Mapper;
using static System.Diagnostics.DebuggerBrowsableState;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;
using static Friflo.Fliox.Engine.ECS.TreeMembership;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// Hard rule: this file/section MUST NOT use GameEntity instances

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertConstructorToMemberInitializers
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// The <see cref="EntityStore"/> provide the features listed below
/// <list type="bullet">
///   <item>
///   Store a map (container) of entities in linear memory.<br/>
///   Entity data can retrieved by entity <b>id</b> using the property <see cref="Nodes"/>.<br/>
///   <see cref="GameEntity"/>'s have the states below:<br/>
///   <list type="bullet">
///     <item>
///       <b><see cref="StoreOwnership"/>:</b> <see cref="attached"/> / <see cref="detached"/><br/>
///       if <see cref="detached"/> - <see cref="NullReferenceException"/> are thrown by <see cref="GameEntity"/> methods.
///     </item>
///     <item>
///       <b><see cref="TreeMembership"/>:</b> <see cref="treeNode"/> / <see cref="floating"/> node (not part of the store tree).<br/>
///       All children of a <see cref="treeNode"/> are <see cref="treeNode"/>'s themselves.
///     </item>
///     </list>
///   </item>
///   <item>Manage a tree of entities which starts with the <see cref="Root"/> entity to build up a scene</item>
///   <item>Store the values of <b>struct</b> components - attributed with <see cref="StructComponentAttribute"/> - in linear memory</item>
/// </list>
/// </summary>
/// <remarks>
/// <i>GameEntity free usage</i><br/>
/// <see cref="EntityStore"/> implementation is prepared for <see cref="GameEntity"/> free usage.<br/>
/// The focus of the this usage type is performance.<br/>
/// The key is to reduce heap consumption and GC costs caused by <see cref="GameEntity"/> instances.<br/>
/// In this case entities are only stored in the <see cref="EntityStore"/> and <see cref="Archetype"/> containers.<br/>
/// <br/>
/// The downside of this approach are:<br/>
/// - Entities can be created only programmatically but not within the editor which requires (managed) <see cref="GameEntity"/>'s.<br/>
/// - API to access / query / mutate <see cref="EntityNode"/>'s is less convenient.<br/>
/// </remarks>
public sealed partial class EntityStore
{
#region public properties
    /// <summary>Number of all entities stored in the <see cref="EntityStore"/></summary>
                    public              int                         EntityCount     => nodeCount;
    
    /// <summary>Array of <see cref="Archetype"/>'s utilized by the <see cref="EntityStore"/></summary>
    /// <remarks>Each <see cref="Archetype"/> contains all entities of a specific combination of <b>struct</b> components.</remarks>
                    public              ReadOnlySpan<Archetype>     Archetypes      => new (archetypes, 0, archetypesCount);
    /// <summary>Enables access to <see cref="EntityNode"/>'s by <see cref="EntityNode.id"/>.</summary>
    /// <returns>a node array that can contain unused nodes. So its length is <see cref="EntityCount"/> + number of unused nodes</returns>
                    public              ReadOnlySpan<EntityNode>    Nodes           => new (nodes);
                    public              int                         NodeMaxId       => nodeMaxId;
    [Browse(Never)] private             bool                        HasRoot         => rootId   >= Static.MinNodeId;
    
                    public  static      ComponentSchema             GetComponentSchema()    => Static.ComponentSchema;
                    public  override    string                      ToString()              => $"Count: {nodeCount}";
    #endregion
    
#region private / internal fields
    [Browse(Never)] private             Archetype[]                 archetypes;         //  8 + archetypes      - array of all archetypes. never null
    [Browse(Never)] internal readonly   HashSet<ArchetypeKey>       archetypeSet;       //  8 + Set<Key>'s      - Set<> to get archetypes by key
    [Browse(Never)] internal            int                         archetypesCount;    //  8                   - number of archetypes
    [Browse(Never)] internal readonly   Archetype                   defaultArchetype;   //  8                   - default archetype without components
    [Browse(Never)] private             int                         rootId;             //  4                   - id of root node
    
    // --- node access
    [Browse(Never)] private  readonly   PidType                     pidType;            //  4                   - pid != id  /  pid == id
    [Browse(Never)] private             Random                      randPid;            //  8                   - null if using pid == id
                    private  readonly   Dictionary<long, int>       pid2Id;             //  8 + Map<pid,id>     - null if using pid == id
    [Browse(Never)] internal            EntityNode[]                nodes;              //  8 + all nodes       - acts also id2pid
    [Browse(Never)] private             int                         nodeMaxId;          //  4                   - highest entity id
    [Browse(Never)] private             int                         nodeCount;          //  4                   - number of all entities
    
    [Browse(Never)] private  readonly LocalEntities<long, DataNode> clientNodes;        //  8 Map<pid,DataNode> - client used to persist entities
    [Browse(Never)] private  readonly   ArchetypeKey                searchKey;          //  8 (+76)             - key buffer to find archetypes by key
    #endregion
    
#region static fields
    public static class Static {
        internal static readonly    int[]           EmptyChildNodes = null;
        internal static readonly    TypeStore       TypeStore       = new TypeStore();
        internal static readonly    ComponentSchema ComponentSchema = ComponentUtils.RegisterComponentTypes(TypeStore);
        
        internal const              int             DefaultCapacity =  1;
        /// <summary>to avoid accidental entity access by id using (default value) 0 </summary>
        internal const              int             MinNodeId       =  1;
        public   const              int             NoParentId      =  0;
        public   const              int             RootId          = -1;
    }
    #endregion
    
#region initialize
    public EntityStore(PidType pidType = PidType.RandomPids, SceneClient client = null) {
        this.pidType        = pidType;
        sequenceId          = Static.MinNodeId;
        rootId              = Static.NoParentId;
        archetypes          = new Archetype[2];
        archetypeSet        = new HashSet<ArchetypeKey>(ArchetypeIdEqualityComparer.Instance);
        if (pidType == PidType.RandomPids) {
            pid2Id  = new Dictionary<long, int>();
            randPid = new Random();
        }
        nodes               = Array.Empty<EntityNode>();
        EnsureNodesLength(2);
        var config          = GetArchetypeConfig();
        defaultArchetype    = Archetype.CreateWithSignatureTypes(config, new StructIndexes(0), default);
        clientNodes         = client?.nodes.Local;
        searchKey           = new ArchetypeKey();
        AddArchetype(defaultArchetype);
    }
    #endregion
    
#region access by pid
    /// <remarks>
    /// Avoid using this method if store is initialized with <see cref="PidType.RandomPids"/>.<br/>
    /// Instead use <see cref="EntityNode.Id"/> instead of <see cref="EntityNode.Pid"/> if possible
    /// as this method performs an expensive <see cref="Dictionary{TKey,TValue}"/> lookup.
    /// </remarks>
    public  int             PidToId(int pid) => pid2Id != null ? pid2Id[pid] : pid;
    
    /// <remarks>
    /// Avoid using this method if store is initialized with <see cref="PidType.RandomPids"/>.<br/>
    /// Instead use <see cref="Nodes"/> if possible as this method performs an expensive <see cref="Dictionary{TKey,TValue}"/> lookup.
    /// </remarks>
    public  ref EntityNode  GetNodeByPid(int pid) {
        if (pid2Id != null) {
            return ref nodes[pid2Id[pid]];
        }
        return ref nodes[pid];
    }
    #endregion
    
#region exceptions
    internal static Exception InvalidStoreException(string parameterName) {
        return new ArgumentException("entity is owned by a different store", parameterName);
    }
    
    private static Exception InvalidEntityIdException(int id, string parameterName) {
        return new ArgumentException($"Invalid node id <= 0. id: {id}", parameterName);
    }
    
    private static Exception IdAlreadyInUseException(int id) {
        return new InvalidOperationException($"id already in use in EntityStore. id: {id}");
    }
    
    private static Exception EntityAlreadyHasParent(int child, int curParent, int newParent) {
        var msg = $"child has already a parent. child: {child} current parent: {curParent}, new parent: {newParent}";
        return new InvalidOperationException(msg);
    }
    #endregion
}
