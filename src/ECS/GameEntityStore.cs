// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using static System.Diagnostics.DebuggerBrowsableState;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;
using static Friflo.Fliox.Engine.ECS.TreeMembership;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWhenPossible

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
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
///       <b><see cref="TreeMembership"/>:</b> <see cref="treeNode"/> / <see cref="floating"/> node (not part of the <see cref="EntityStore"/> tree graph).<br/>
///       All children of a <see cref="treeNode"/> are <see cref="treeNode"/>'s themselves.
///     </item>
///     </list>
///   </item>
///   <item>Manage a tree graph of entities which starts with the <see cref="StoreRoot"/> entity to build up a scene</item>
///   <item>Store the values of <b>struct</b> components - attributed with <see cref="ComponentAttribute"/> - in linear memory</item>
/// </list>
/// </summary>
[CLSCompliant(true)]
public sealed partial class EntityStore : EntityStoreBase
{
#region public properties
    /// <summary>Enables access to <see cref="EntityNode"/>'s by <see cref="EntityNode.id"/>.</summary>
    /// <returns>A node array that can contain unused nodes. So its length is <see cref="EntityStore.EntityCount"/> + number of unused nodes</returns>
    public ReadOnlySpan<EntityNode>                 Nodes               => new (nodes);
    public              GameEntity                  StoreRoot           => storeRoot; // null if no graph origin set
    public ReadOnlySpan<EntityScripts>              EntityScripts       => new (entityScripts, 0, entityScriptCount);
    /// <summary>Set or clear a <see cref="ECS.ChildNodesChangedHandler"/> to get events on add, insert, remove or delete <see cref="GameEntity"/>'s.</summary>
    /// <remarks>Event handlers previously added with <see cref="ChildNodesChanged"/> are removed.</remarks>
    public              ChildNodesChangedHandler    ChildNodesChangedHandler { get => childNodesChanged; set => childNodesChanged = value; }
    #endregion
    
#region internal fields
    // --- Note: all fields must stay private to limit the scope of mutations
    [Browse(Never)] private             EntityNode[]            nodes;              //  8 + all nodes   - acts also id2pid
    [Browse(Never)] private  readonly   PidType                 pidType;            //  4               - pid != id  /  pid == id
    [Browse(Never)] private             Random                  randPid;            //  8               - null if using pid == id
                    private  readonly   Dictionary<long, int>   pid2Id;             //  8 + Map<pid,id> - null if using pid == id
    [Browse(Never)] private             GameEntity              storeRoot;          //  8               - origin of the tree graph. null if no origin assigned
    /// <summary>Contains implicit all entities with one or more <see cref="Script"/>'s to minimize iteration cost for <see cref="Script.Update"/>.</summary>
    [Browse(Never)] private             EntityScripts[]         entityScripts;      //  8
    /// <summary>Count of entities with one or more <see cref="Script"/>'s</summary>
    [Browse(Never)] private             int                     entityScriptCount;  //  4               - >= 0  and  <= entityScripts.Length
    
    [Browse(Never)] private            ChildNodesChangedHandler childNodesChanged;  //  8               - fire events on add, insert, remove or delete a GameEntity
    [Browse(Never)] private             int[]                   idBuffer;           //  8
    [Browse(Never)] private readonly    HashSet<int>            idBufferSet;        //  8
    
    #endregion
    
#region initialize
    public EntityStore() : this (PidType.RandomPids) { }
    
    public EntityStore(PidType pidType)
    {
        this.pidType    = pidType;
        nodes           = Array.Empty<EntityNode>();
        EnsureNodesLength(2);
        if (pidType == PidType.RandomPids) {
            pid2Id  = new Dictionary<long, int>();
            randPid = new Random();
        }
        entityScripts   = Array.Empty<EntityScripts>();
        idBuffer        = new int[1];
        idBufferSet     = new HashSet<int>();
    }
    #endregion
    
#region access by pid
    /// <remarks>
    /// Avoid using this method if store is initialized with <see cref="PidType.RandomPids"/>.<br/>
    /// Instead use <see cref="EntityNode.Id"/> instead of <see cref="EntityNode.Pid"/> if possible
    /// as this method performs an expensive <see cref="Dictionary{TKey,TValue}"/> lookup.
    /// </remarks>
    public  int             PidToId(long pid) => pid2Id != null ? pid2Id[pid] : (int)pid;
    
    /// <remarks>
    /// Avoid using this method if store is initialized with <see cref="PidType.RandomPids"/>.<br/>
    /// Instead use <see cref="Nodes"/> if possible as this method performs an expensive <see cref="Dictionary{TKey,TValue}"/> lookup.
    /// </remarks>
    public  ref EntityNode  GetNodeByPid(long pid) {
        if (pid2Id != null) {
            return ref nodes[pid2Id[pid]];
        }
        return ref nodes[pid];
    }
    
    public  ref EntityNode  GetNodeById(int id) {
        return ref nodes[id];
    }
    #endregion
}