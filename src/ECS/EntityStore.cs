// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS.Serialize;
using static System.Diagnostics.DebuggerBrowsableState;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;
using static Friflo.Fliox.Engine.ECS.TreeMembership;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// The <see cref="EntityStore"/> provide the features listed below
/// <list type="bullet">
///   <item>
///   Store a map (container) of entities in linear memory.<br/>
///   Entity data can retrieved by entity <b>id</b> using the property <see cref="GetEntityById"/>.<br/>
///   <see cref="Entity"/>'s have the states below:<br/>
///   <list type="bullet">
///     <item>
///       <b><see cref="StoreOwnership"/>:</b> <see cref="attached"/> / <see cref="detached"/><br/>
///       if <see cref="detached"/> - <see cref="NullReferenceException"/> are thrown by <see cref="Entity"/> methods.
///     </item>
///     <item>
///       <b><see cref="TreeMembership"/>:</b> <see cref="treeNode"/> / <see cref="floating"/> node (not part of the <see cref="EntityStore"/> tree graph).<br/>
///       All children of a <see cref="treeNode"/> are <see cref="treeNode"/>'s themselves.
///     </item>
///     </list>
///   </item>
///   <item>Manage a tree graph of entities which starts with the <see cref="StoreRoot"/> entity to build up a scene graph</item>
///   <item>Store the data of <see cref="IComponent"/>'s and <see cref="Script"/>'s.</item>
/// </list>
/// </summary>
[CLSCompliant(true)]
public sealed partial class EntityStore : EntityStoreBase
{
#region public properties
    /// <summary>Enables access to <see cref="EntityNode"/>'s by <see cref="EntityNode.id"/>.</summary>
    /// <returns>A node array that can contain unused nodes. So its length is <see cref="EntityStore.EntityCount"/> + number of unused nodes</returns>
    public ReadOnlySpan<EntityNode>                 Nodes               => new (nodes);
    public              Entity                      StoreRoot           => storeRoot; // null if no graph origin set
    public ReadOnlySpan<EntityScripts>              EntityScripts       => new (entityScripts, 1, entityScriptCount - 1);
    #endregion
    
#region event handler
    /// <summary>Set or clear a <see cref="ChildEntitiesChangedHandler"/> to get events on add, insert, remove or delete <see cref="Entity"/>'s.</summary>
    /// <remarks>Event handlers previously added with <see cref="ChildEntitiesChanged"/> are removed.</remarks>
    public  ChildEntitiesChangedHandler ChildEntitiesChanged{ get => childEntitiesChanged;  set => childEntitiesChanged = value; }
    
    // --- script:   added / removed
    public  ScriptChangedHandler        ScriptAdded         { get => scriptAdded;           set => scriptAdded          = value; }
    public  ScriptChangedHandler        ScriptRemoved       { get => scriptRemoved;         set => scriptRemoved        = value; }
    
    public  EntitiesChangedHandler      EntitiesChanged     { get => entitiesChanged;       set => entitiesChanged      = value; }
    
    #endregion
    
#region internal fields
    // --- Note: all fields must stay private to limit the scope of mutations
    [Browse(Never)] internal            EntityNode[]            nodes;              //  8 + all nodes   - acts also id2pid
    [Browse(Never)] private  readonly   PidType                 pidType;            //  4               - pid != id  /  pid == id
    [Browse(Never)] private             Random                  randPid;            //  8               - null if using pid == id
                    private  readonly   Dictionary<long, int>   pid2Id;             //  8 + Map<pid,id> - null if using pid == id
    [Browse(Never)] private             Entity                  storeRoot;          // 16               - origin of the tree graph. null if no origin assigned
    /// <summary>Contains implicit all entities with one or more <see cref="Script"/>'s to minimize iteration cost for <see cref="Script.Update"/>.</summary>
    [Browse(Never)] private             EntityScripts[]         entityScripts;      //  8               - invariant: entityScripts[0] = 0
    /// <summary>Count of entities with one or more <see cref="Script"/>'s</summary>
    [Browse(Never)] private             int                     entityScriptCount;  //  4               - invariant: > 0  and  <= entityScripts.Length

    // --- buffers
    [Browse(Never)] private             int[]                   idBuffer;           //  8
    [Browse(Never)] private readonly    HashSet<int>            idBufferSet;        //  8
    [Browse(Never)] private readonly    DataEntity              dataBuffer;         //  8

    // --- delegates
    [Browse(Never)] private         ChildEntitiesChangedHandler childEntitiesChanged;// 8               - fire events on add, insert, remove or delete an Entity
    //
    [Browse(Never)] private         ScriptChangedHandler        scriptAdded;        //  8
    [Browse(Never)] private         ScriptChangedHandler        scriptRemoved;      //  8
    //
    [Browse(Never)] private         EntitiesChangedHandler      entitiesChanged;    //  8
    
    #endregion
    
#region initialize
    public EntityStore() : this (PidType.RandomPids) { }
    
    public EntityStore(PidType pidType)
    {
        this.pidType        = pidType;
        nodes               = Array.Empty<EntityNode>();
        EnsureNodesLength(2);
        if (pidType == PidType.RandomPids) {
            pid2Id  = new Dictionary<long, int>();
            randPid = new Random();
        }
        entityScripts       = new EntityScripts[1]; // invariant: entityScripts[0] = 0
        entityScriptCount   = 1;
        idBuffer            = new int[1];
        idBufferSet         = new HashSet<int>();
        dataBuffer          = new DataEntity();
    }
    #endregion
    
#region get EntityNode by id
    public  ref readonly  EntityNode  GetEntityNode(int id) {
        return ref nodes[id];
    }
    #endregion

#region get Entity by id / pid
    /// <remarks>
    /// Avoid using this method if store is initialized with <see cref="PidType.RandomPids"/>.<br/>
    /// Instead use <see cref="Entity.Id"/> instead of <see cref="Entity.Pid"/> if possible
    /// as this method performs a <see cref="Dictionary{TKey,TValue}"/> lookup.
    /// </remarks>
    public  int             PidToId(long pid)   => pid2Id != null ? pid2Id[pid] : (int)pid;
    
    public  long            IdToPid(int id)     => nodes[id].pid;
    
    /*
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
    } */
    
    public  Entity  GetEntityByPid(long pid) {
        if (pid2Id != null) {
            return new Entity(pid2Id[pid], this);
        }
        return new Entity((int)pid, this);
    }
    
    public  bool  TryGetEntityByPid(long pid, out Entity value) {
        if (pid2Id != null) {
            if (pid2Id.TryGetValue(pid,out int id)) {
                value = new Entity(id, this);
                return true;
            }
            value = default;
            return false;
        }
        if (0 < pid && pid <= nodesMaxId) {
            var id = (int)pid;
            if (nodes[id].Is(NodeFlags.Created)) {
                value = new Entity(id, this);
                return true;
            }
        }
        value = default;
        return false;
    }
    
    public  Entity  GetEntityById(int id) {
        return new Entity(id, this);
    }
    #endregion
}