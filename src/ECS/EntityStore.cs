// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
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
/// and the tree structure.<br/>
/// See <a href="https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-General#entitystore">Example.</a>
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
                    public              Entity              StoreRoot       => storeRoot; // null if no graph origin set
    
    /// <summary> Return all <see cref="Script"/>'s added to <see cref="Entity"/>'s in the <see cref="EntityStore"/>. </summary>
                    public ReadOnlySpan<EntityScripts>      EntityScripts   => new (entityScripts, 1, entityScriptCount - 1);
    
    /// <summary> Return all <see cref="Entity"/>'s stored in the <see cref="EntityStore"/>.</summary>
    /// <remarks>Property is mainly used for debugging.<br/>
    /// For efficient access to entity <see cref="IComponent"/>'s use one of the generic <b><c>EntityStore.Query()</c></b> methods. </remarks>
                    public              QueryEntities       Entities        => GetEntities();
    
    /// <summary>
    /// Record adding/removing of components/tags to/from entities if <see cref="ECS.EventRecorder.Enabled"/> is true.<br/>
    /// It is required to filter these events using an <see cref="EventFilter"/>.
    /// </summary>
    [Browse(Never)] public              EventRecorder       EventRecorder   => GetEventRecorder();
    
    /// <summary> Get the number of internally reserved entities. </summary>
    [Browse(Never)] public              int                 Capacity        => nodes.Length;
    
    /// <summary> Return store information used for debugging and optimization. </summary>
    // ReSharper disable once InconsistentNaming
    [Browse(Never)] public readonly     EntityStoreInfo     Info;
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
    
    
    /// <summary>Add / remove an event handler for <see cref="EntityCreate"/> events triggered by <see cref="EntityStore.CreateEntity()"/>.</summary>
    public event    Action<EntityCreate>            OnEntityCreate          { add => intern.entityCreate        += value; remove => intern.entityCreate     -= value; }
    
    /// <summary>Add / remove an event handler for <see cref="EntityDelete"/> events triggered by <see cref="Entity.DeleteEntity()"/>.</summary>
    public event    Action<EntityDelete>            OnEntityDelete          { add => intern.entityDelete        += value; remove => intern.entityDelete     -= value; }
    
    public  void    CastEntitiesChanged(object sender, EntitiesChanged args) => intern.entitiesChanged?.Invoke(sender, args);
    #endregion
    
#region internal fields
    // --- Note: all fields must stay private to limit the scope of mutations
    [Browse(Never)] internal            EntityNode[]            nodes;              //  8   - acts also id2pid
    [Browse(Never)] private             Entity                  storeRoot;          // 16   - origin of the tree graph. null if no origin assigned
    /// <summary>Contains implicit all entities with one or more <see cref="Script"/>'s to minimize iteration cost for <see cref="Script.Update"/>.</summary>
    [Browse(Never)] private             EntityScripts[]         entityScripts;      //  8   - invariant: entityScripts[0] = 0
    /// <summary>Contains the <see cref="entityScripts"/> index (value) of an entity id (key)</summary>
                    internal readonly   Dictionary<int, int>    scriptMap;          //  8   - invariant: entityScripts[0] = 0
    
    /// <summary>Count of entities with one or more <see cref="Script"/>'s</summary>
    [Browse(Never)] private             int                     entityScriptCount;  //  4   - invariant: > 0  and  <= entityScripts.Length
    // --- buffers
    [Browse(Never)] private             int[]                   idBuffer;           //  8
    [Browse(Never)] private  readonly   HashSet<int>            idBufferSet;        //  8
    [Browse(Never)] private  readonly   DataEntity              dataBuffer;         //  8
                    internal            Internals               internals;          //  8
                    private             Intern                  intern;             // 88

    internal struct Internals {
        internal readonly                   Dictionary<int, int>    parentMap;      //  8   - store the parent (value) of an entity (key)
        internal                            Random                  randPid;        //  8   - generate random pid's                       - null if UsePidAsId
        internal readonly                   Dictionary<long, int>   pid2Id;         //  8   - store the id (value) of a pid (key)         - null if UsePidAsId
        internal readonly                   Dictionary<int, long>   id2Pid;         //  8   - store the pid (value) of an entity id (key) - null if UsePidAsId
        
        internal Internals(PidType pidType)
        {
           parentMap = new Dictionary<int, int>();
           if (pidType == PidType.RandomPids) {
               randPid  = new Random();
               pid2Id   = new Dictionary<long, int>();
               id2Pid   = new Dictionary<int, long>();
           }
        }
    }
    
    /// <summary>Contains state of <see cref="EntityStore"/> not relevant for application development.</summary>
    /// <remarks>Declaring internal state fields in this struct remove noise in debugger.</remarks>
    // MUST be private by all means.
    private struct Intern {
        internal readonly                   PidType                 pidType;                //  4   - pid != id  /  pid == id
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
        internal    Action                <EntityCreate>            entityCreate;           //  8   - fires event on create entity
        internal    Action                <EntityDelete>            entityDelete;           //  8   - fires event on delete entity
        //
        internal    EventHandler          <EntitiesChanged>         entitiesChanged;        //  8   - fires event to notify changes of multiple entities
        //
        internal    ArchetypeQuery                                  entityQuery;            //  8
        //
        internal    Stack<CommandBuffer>                            commandBufferPool;      //  8
        internal    Playback                                        playback;               // 16
        internal    EventRecorder                                   eventRecorder;          //  8

                    
        internal Intern(PidType pidType)
        {
            this.pidType        = pidType;
            sequenceId          = Static.MinNodeId - 1;
            signalHandlerMap    = Array.Empty<SignalHandler>();
        }
    }
    #endregion
    
#region initialize
    public EntityStore() : this (PidType.UsePidAsId) { }
    
    public EntityStore(PidType pidType)
    {
        intern              = new Intern(pidType);
        internals           = new Internals(pidType);
        scriptMap           = new Dictionary<int, int>();
        nodes               = Array.Empty<EntityNode>();
        EnsureNodesLength(2);
        entityScripts       = new EntityScripts[1]; // invariant: entityScripts[0] = 0
        entityScriptCount   = 1;
        idBuffer            = new int[1];
        idBufferSet         = new HashSet<int>();
        dataBuffer          = new DataEntity();
        Info                = new EntityStoreInfo(this);
    }
    #endregion
    

#region id / pid conversion
    /// <summary>
    /// Return the <see cref="Entity.Id"/> for the passed entity <paramref name="pid"/>.
    /// </summary>
    /// <remarks>
    /// Avoid using this method if store is initialized with <see cref="PidType.RandomPids"/>.<br/>
    /// Instead use <see cref="Entity.Id"/> instead of <see cref="Entity.Pid"/> if possible
    /// as this method performs a <see cref="Dictionary{TKey,TValue}"/> lookup.
    /// </remarks>
    public  int             PidToId(long pid)   => internals.pid2Id != null ? internals.pid2Id[pid] : (int)pid;

    /// <summary>
    /// Return the <see cref="Entity.Pid"/> for the passed entity <paramref name="id"/>.
    /// </summary>
    public  long            IdToPid(int id)     => internals.id2Pid != null ? internals.id2Pid[id] : id;
    #endregion
    
#region get EntityNode by id
    /// <summary>
    /// Return the internal node for the passed entity <paramref name="id"/>. 
    /// </summary>
    public  ref readonly  EntityNode  GetEntityNode(int id) {
        return ref nodes[id];
    }
    #endregion

#region get Entity by id / pid

    /// <summary>
    /// Returns the <see cref="Entity"/> with the passed <paramref name="id"/>.<br/>
    /// The returned entity can be null (<see cref="Entity.IsNull"/> == true).
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"> In case passed <paramref name="id"/> invalid (id >= <see cref="Capacity"/>). </exception>
    public  Entity  GetEntityById(int id) {
        if (0 <= id && id < nodes.Length) {
            return new Entity(this, id);
        }
        throw new IndexOutOfRangeException();
    }
    
    /// <summary>
    /// Get the <see cref="Entity"/> associated with the passed <paramref name="id"/>.<br/>
    /// Returns true if passed <paramref name="id"/> is valid (id &lt; <see cref="Capacity"/>).<br/>
    /// The returned entity can be null (<see cref="Entity.IsNull"/> == true).
    /// </summary>
    public  bool TryGetEntityById(int id, out Entity entity)
    {
        var localNodes = nodes;
        if (0 <= id && id < localNodes.Length) {
            entity = new Entity(this, id);
            return localNodes[id].archetype != null;
        }
        entity = default;
        return false;
    }
    
    /// <summary>
    /// Return the <see cref="Entity"/> with the passed entity <paramref name="pid"/>.
    /// </summary>
    public  Entity  GetEntityByPid(long pid)
    {
        var pid2Id = internals.pid2Id;
        if (pid2Id != null) {
            return new Entity(this, pid2Id[pid]);
        }
        return new Entity(this, (int)pid);
    }
    
    /// <summary>
    /// Try to return the <see cref="Entity"/> with the passed entity <paramref name="pid"/>.<br/>
    /// </summary>
    public  bool  TryGetEntityByPid(long pid, out Entity value)
    {
        var pid2Id = internals.pid2Id;
        if (pid2Id != null) {
            if (pid2Id.TryGetValue(pid,out int id)) {
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