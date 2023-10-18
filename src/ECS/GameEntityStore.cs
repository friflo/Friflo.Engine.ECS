// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.Client;
using static System.Diagnostics.DebuggerBrowsableState;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;
using static Friflo.Fliox.Engine.ECS.TreeMembership;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
using LocalEntities = Friflo.Json.Fliox.Hub.Client.LocalEntities<long, Friflo.Fliox.Engine.Client.DataNode>;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// The <see cref="GameEntityStore"/> provide the features listed below
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
///       <b><see cref="TreeMembership"/>:</b> <see cref="rootTreeNode"/> / <see cref="floating"/> node (not part of the store <see cref="Root"/> tree).<br/>
///       All children of a <see cref="rootTreeNode"/> are <see cref="rootTreeNode"/>'s themselves.
///     </item>
///     </list>
///   </item>
///   <item>Manage a tree of entities which starts with the <see cref="Root"/> entity to build up a scene</item>
///   <item>Store the values of <b>struct</b> components - attributed with <see cref="StructComponentAttribute"/> - in linear memory</item>
/// </list>
/// </summary>
[CLSCompliant(true)]
public sealed partial class GameEntityStore : EntityStore
{
#region public properties
    /// <summary>Enables access to <see cref="EntityNode"/>'s by <see cref="EntityNode.id"/>.</summary>
    /// <returns>A node array that can contain unused nodes. So its length is <see cref="EntityStore.EntityCount"/> + number of unused nodes</returns>
                    public              ReadOnlySpan<EntityNode>    Nodes           => new (nodes);
    [Browse(Never)] private             bool                        HasRoot         => rootId   >= Static.MinNodeId;
                    public              GameEntity                  Root            => nodes[rootId].entity;    // null if no root set
    #endregion
    
#region internal fields
    // --- nodes
    [Browse(Never)] private             EntityNode[]            nodes;              //  8 + all nodes       - acts also id2pid
    [Browse(Never)] private  readonly   PidType                 pidType;            //  4                   - pid != id  /  pid == id
    [Browse(Never)] private             Random                  randPid;            //  8                   - null if using pid == id
                    private  readonly   Dictionary<long, int>   pid2Id;             //  8 + Map<pid,id>     - null if using pid == id
    [Browse(Never)] private             int                     rootId;             //  4                   - id of root node. 0 = NoParentId
    // --- misc
    [Browse(Never)] private  readonly   LocalEntities           clientNodes;        //  8 Map<pid,DataNode> - client used to persist entities
    #endregion
    
#region initialize
    public GameEntityStore(PidType pidType = PidType.RandomPids, SceneClient client = null)
    {
        this.pidType        = pidType;
        nodes               = Array.Empty<EntityNode>();
        EnsureNodesLength(2);
        rootId              = Static.NoParentId;
        if (pidType == PidType.RandomPids) {
            pid2Id  = new Dictionary<long, int>();
            randPid = new Random();
        }
        clientNodes = client?.nodes.Local;
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
}