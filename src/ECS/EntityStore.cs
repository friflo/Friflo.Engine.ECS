// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Serialize;
using static System.Diagnostics.DebuggerBrowsableState;
using static Friflo.Engine.ECS.StoreOwnership;
using static Friflo.Engine.ECS.TreeMembership;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
namespace Friflo.Engine.ECS;

/// <summary>
/// An <see cref="EntityStore"/> is a container for <see cref="Entity"/>'s their components, scripts, tags
/// and the tree structure.
/// </summary>
/// <remarks>
/// The <see cref="EntityStore"/> provide the features listed below
/// <list type="bullet">
///   <item>
///   Store a map (container) of entities in linear memory.<br/>
///   Entity data can retrieved by entity <b>id</b> using the property <see cref="GetEntityById"/>.<br/>
///   <see cref="Entity"/>'s have the states below:<br/>
///   <list type="bullet">
///     <item>
///       <see cref="StoreOwnership"/>: <see cref="attached"/> / <see cref="detached"/><br/>
///       if <see cref="detached"/> - <see cref="NullReferenceException"/> are thrown by <see cref="Entity"/> properties and methods.
///     </item>
///     <item>
///       <see cref="TreeMembership"/>: <see cref="treeNode"/> / <see cref="floating"/> node (not part of the <see cref="EntityStore"/> tree graph).<br/>
///       All children of a <see cref="treeNode"/> are <see cref="treeNode"/>'s themselves.
///     </item>
///     </list>
///   </item>
///   <item>Manage a tree graph of entities which starts with the <see cref="StoreRoot"/> entity to build up a scene graph.</item>
///   <item>Store the data of <see cref="IComponent"/>'s and <see cref="Script"/>'s.</item>
/// </list>
/// </remarks>
[CLSCompliant(true)]
public sealed partial class EntityStore : EntityStoreBase
{
#region public properties
    /// <summary> Return the root <see cref="Entity"/> of the store.</summary>
    public              Entity          StoreRoot               => storeRoot; // null if no graph origin set
    
    /// <summary> Return all <see cref="Script"/>'s added to <see cref="Entity"/>'s in the <see cref="EntityStore"/>. </summary>
    public ReadOnlySpan<EntityScripts>  EntityScripts           => new (entityScripts, 1, entityScriptCount - 1);
    
    /// <summary> Return all <see cref="Entity"/>'s stored in the <see cref="EntityStore"/>.</summary>
    /// <remarks>Property is mainly used for debugging.<br/>
    /// For efficient access to entity <see cref="IComponent"/>'s use one of the generic <b><c>EntityStore.Query()</c></b> methods. </remarks>
    public              QueryEntities   Entities                => GetEntities();
    #endregion
    
#region events
    /// <summary>Add / remove an event handler for <see cref="ECS.ChildEntitiesChanged"/> events triggered by:<br/>
    /// <see cref="Entity.AddChild"/> <br/> <see cref="Entity.InsertChild"/> <br/> <see cref="Entity.RemoveChild"/>.</summary>
    public  event   Action<ChildEntitiesChanged>    OnChildEntitiesChanged  { add => intern.childEntitiesChanged+= value;   remove => intern.childEntitiesChanged -= value; }
    
    /// <summary>Add / remove an event handler for <see cref="ECS.ScriptChanged"/> events triggered by:<br/>
    /// <see cref="Entity.AddScript{T}"/>.</summary>
    public  event   Action<ScriptChanged>           OnScriptAdded           { add => intern.scriptAdded         += value;   remove => intern.scriptAdded    -= value; }
    
    /// <summary>Add / remove an event handler for <see cref="ECS.ScriptChanged"/> events triggered by:<br/>
    /// <see cref="Entity.RemoveScript{T}"/> .</summary>
    public  event   Action<ScriptChanged>           OnScriptRemoved         { add => intern.scriptRemoved       += value;   remove => intern.scriptRemoved  -= value; }
    
    /// <summary> Fire events in case an <see cref="Entity"/> changed. </summary>
    public  event   EventHandler<EntitiesChanged>   OnEntitiesChanged       { add => intern.entitiesChanged     += value;   remove => intern.entitiesChanged-= value; }
    
    public  void    CastEntitiesChanged(object sender, EntitiesChanged args) => intern.entitiesChanged?.Invoke(sender, args);
    #endregion
    
#region internal fields
    // --- Note: all fields must stay private to limit the scope of mutations
    [Browse(Never)] internal            EntityNode[]            nodes;              //  8   - acts also id2pid
    [Browse(Never)] private             Entity                  storeRoot;          // 16   - origin of the tree graph. null if no origin assigned
    /// <summary>Contains implicit all entities with one or more <see cref="Script"/>'s to minimize iteration cost for <see cref="Script.Update"/>.</summary>
    [Browse(Never)] private             EntityScripts[]         entityScripts;      //  8   - invariant: entityScripts[0] = 0
    /// <summary>Count of entities with one or more <see cref="Script"/>'s</summary>
    [Browse(Never)] private             int                     entityScriptCount;  //  4   - invariant: > 0  and  <= entityScripts.Length
    // --- buffers
    [Browse(Never)] private             int[]                   idBuffer;           //  8
    [Browse(Never)] private readonly    HashSet<int>            idBufferSet;        //  8
    [Browse(Never)] private readonly    DataEntity              dataBuffer;         //  8

                    private             Intern                  intern;             // 88
    /// <summary>Contains state of <see cref="EntityStore"/> not relevant for application development.</summary>
    /// <remarks>Declaring internal state fields in this struct remove noise in debugger.</remarks>
    // MUST be private by all means 
    private struct Intern {
                        internal readonly   PidType                 pidType;                //  4   - pid != id  /  pid == id
                        internal            Random                  randPid;                //  8   - null if using pid == id
                        internal readonly   Dictionary<long, int>   pid2Id;                 //  8   - null if using pid == id

                        internal            int                     sequenceId;             //  4   - incrementing id used for next new entity
        // --- delegates
        internal    Action                <ChildEntitiesChanged>    childEntitiesChanged;   // 8   - fires event on add, insert, remove or delete an Entity
        internal    Dictionary<int, Action<ChildEntitiesChanged>>   entityChildEntitiesChanged;//  8
        //
        internal    Action                <ScriptChanged>           scriptAdded;            //  8   - fires event on add script
        internal    Action                <ScriptChanged>           scriptRemoved;          //  8   - fires event on remove script
        internal    Dictionary<int, Action<ScriptChanged>>          entityScriptChanged;    //  8   - entity event handlers for add/remove script
        //
        internal    SignalHandler[]                                 signalHandlerMap;       //  8
        internal    List<SignalHandler>                             signalHandlers;         //  8 
        //
        internal    EventHandler          <EntitiesChanged>         entitiesChanged;        //  8   - fires event to notify changes of multiple entities
        //
        internal    ArchetypeQuery                                  entityQuery;            //  8
                    
        internal Intern(PidType pidType)
        {
            this.pidType    = pidType;
            sequenceId      = Static.MinNodeId;
            if (pidType == PidType.RandomPids) {
                pid2Id  = new Dictionary<long, int>();
                randPid = new Random();
            }
            signalHandlerMap = Array.Empty<SignalHandler>();
        }
    }
    #endregion
    
#region initialize
    public EntityStore() : this (PidType.RandomPids) { }
    
    public EntityStore(PidType pidType)
    {
        intern              = new Intern(pidType);
        nodes               = Array.Empty<EntityNode>();
        EnsureNodesLength(2);
        entityScripts       = new EntityScripts[1]; // invariant: entityScripts[0] = 0
        entityScriptCount   = 1;
        idBuffer            = new int[1];
        idBufferSet         = new HashSet<int>();
        dataBuffer          = new DataEntity();
    }
    #endregion
    

#region id / pid conversion
    /// <remarks>
    /// Avoid using this method if store is initialized with <see cref="PidType.RandomPids"/>.<br/>
    /// Instead use <see cref="Entity.Id"/> instead of <see cref="Entity.Pid"/> if possible
    /// as this method performs a <see cref="Dictionary{TKey,TValue}"/> lookup.
    /// </remarks>
    public  int             PidToId(long pid)   => intern.pid2Id != null ? intern.pid2Id[pid] : (int)pid;
        
    public  long            IdToPid(int id)     => nodes[id].pid;
    #endregion
    
#region get EntityNode by id

    public  ref readonly  EntityNode  GetEntityNode(int id) {
        return ref nodes[id];
    }
    #endregion

#region get Entity by id / pid

    public  Entity  GetEntityById(int id) {
        return new Entity(this, id);
    }
    
    public  Entity  GetEntityByPid(long pid) {
        if (intern.pid2Id != null) {
            return new Entity(this, intern.pid2Id[pid]);
        }
        return new Entity(this, (int)pid);
    }
    
    public  bool  TryGetEntityByPid(long pid, out Entity value) {
        if (intern.pid2Id != null) {
            if (intern.pid2Id.TryGetValue(pid,out int id)) {
                value = new Entity(this, id);
                return true;
            }
            value = default;
            return false;
        }
        if (0 < pid && pid <= nodesMaxId) {
            var id = (int)pid;
            if (nodes[id].Is(NodeFlags.Created)) {
                value = new Entity(this, id);
                return true;
            }
        }
        value = default;
        return false;
    }
    #endregion
}