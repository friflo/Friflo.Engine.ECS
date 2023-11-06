// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Specialized;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;
using static Friflo.Fliox.Engine.ECS.StructInfo;
using static Friflo.Fliox.Engine.ECS.TreeMembership;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="GameEntity"/> represent any kind of object in a game scene.<br/>
/// Every <see cref="GameEntity"/> has an <see cref="Id"/> and is a container of
/// <see cref="ECS.IComponent"/>'s, <see cref="ECS.Behavior"/>'s and <see cref="ECS.Tags"/><br/>
/// <br/>
/// It is typically an object that can be rendered on screen like a cube, sphere, capsule, mesh, sprite, ... .<br/>
/// Therefore a renderable component needs to be added with <see cref="AddComponent{T}()"/> to a <see cref="GameEntity"/>.<br/>
/// <br/>
/// A <see cref="GameEntity"/> can be added to another <see cref="GameEntity"/> using <see cref="AddChild"/>.<br/>
/// The added <see cref="GameEntity"/> becomes a child of the <see cref="GameEntity"/> it is added to - its <see cref="Parent"/>.<br/>
/// This enables to build up a complex game scene with a hierarchy of <see cref="GameEntity"/>'s.<br/>
/// The order of children contained by an entity is the insertion order.<br/>  
/// <br/>
/// <see cref="ECS.Behavior"/>'s can be added to a <see cref="GameEntity"/> to add custom logic (behavior) and data to an entity.<br/>
/// <see cref="ECS.Behavior"/>'s are added or removed with <see cref="AddBehavior{T}"/> / <see cref="RemoveBehavior{T}"/>.<br/>
/// <br/>
/// <see cref="Tags"/> can be added to a <see cref="GameEntity"/> to enable filtering entities in queries.<br/>
/// By adding <see cref="Tags"/> to an <see cref="ArchetypeQuery"/> it can be restricted to return only entities matching the
/// these <see cref="Tags"/>.
/// </summary>
/// <remarks>
/// <b>general</b>
/// <list type="bullet">
///     <item><see cref="Id"/></item>
///     <item><see cref="Archetype"/></item>
///     <item><see cref="Store"/></item>
///     <item><see cref="StoreOwnership"/></item>
///     <item><see cref="TreeMembership"/></item>
/// </list>
/// <b>components</b> · generic
/// <list type="bullet">
///     <item><see cref="HasComponent{T}"/></item>
///     <item><see cref="GetComponent{T}"/> - read / write</item>
///     <item><see cref="TryGetComponent{T}"/></item>
///     <item><see cref="AddComponent{T}()"/></item>
///     <item><see cref="RemoveComponent{T}"/></item>
/// </list>
/// <b>components</b> · common
/// <list type="bullet">
///     <item><see cref="Name"/></item>
///     <item><see cref="Position"/></item>
///     <item><see cref="Rotation"/></item>
///     <item><see cref="Scale3"/></item>
///     <item><see cref="HasName"/></item>
///     <item><see cref="HasPosition"/></item>
///     <item><see cref="HasRotation"/></item>
///     <item><see cref="HasScale3"/></item>
/// </list>
/// <b>behaviors</b> · generic
/// <list type="bullet">
///     <item><see cref="Behaviors"/></item>
///     <item><see cref="GetBehavior{T}"/></item>
///     <item><see cref="TryGetBehavior{T}"/></item>
///     <item><see cref="AddBehavior{T}"/></item>
///     <item><see cref="RemoveBehavior{T}"/></item>
/// </list>
/// <b>tags</b>
/// <list type="bullet">
///     <item><see cref="Tags"/></item>
///     <item><see cref="AddTag{T}"/></item>
///     <item><see cref="AddTags"/></item>
///     <item><see cref="RemoveTag{T}"/></item>
///     <item><see cref="RemoveTags"/></item>
/// </list>
/// <b>tree nodes</b>
/// <list type="bullet">
///     <item><see cref="Parent"/></item>
///     <item><see cref="ChildNodes"/></item>
///     <item><see cref="ChildIds"/></item>
///     <item><see cref="ChildCount"/></item>
///     <item><see cref="AddChild"/></item>
///     <item><see cref="RemoveChild"/></item>
///     <item><see cref="DeleteEntity"/></item>
/// </list>
/// </remarks>
[CLSCompliant(true)]
public sealed partial class GameEntity
{
#region public properties
    /// <summary>Unique entity id.<br/>
    /// Uniqueness relates to the <see cref="GameEntity"/>'s stored in its <see cref="GameEntityStore"/></summary>
                    public   int            Id              => id;

    /// <remarks>The <see cref="Archetype"/> the entity is stored.<br/>Return null if the entity is <see cref="detached"/></remarks>
                    public   Archetype      Archetype       => archetype;
    
    /// <remarks>The <see cref="Store"/> the entity is <see cref="attached"/> to. Returns null if <see cref="detached"/></remarks>
    [Browse(Never)] public  GameEntityStore Store           => archetype?.gameEntityStore;
                    
    /// <remarks>If <see cref="attached"/> its <see cref="Store"/> and <see cref="Archetype"/> are not null. Otherwise null.</remarks>
    [Browse(Never)] public  StoreOwnership  StoreOwnership  => archetype != null ? attached : detached;
    
    /// <returns>
    /// <see cref="treeNode"/> if the entity is member of the <see cref="GameEntityStore"/> tree graph.<br/>
    /// Otherwise <see cref="floating"/></returns>
    [Browse(Never)] public  TreeMembership  TreeMembership  => archetype.gameEntityStore.GetTreeMembership(id);
    
    public override string                  ToString()      => GameEntityUtils.GameEntityToString(this, new StringBuilder());

    #endregion

#region public properties - components
    
    /// <exception cref="NullReferenceException"> if entity has no <see cref="EntityName"/></exception>
    [Browse(Never)] public  ref EntityName  Name        => ref archetype.std.name.    chunks[compIndex / ChunkSize].components[compIndex % ChunkSize];

    /// <exception cref="NullReferenceException"> if entity has no <see cref="Position"/></exception>
    [Browse(Never)] public  ref Position    Position    => ref archetype.std.position.chunks[compIndex / ChunkSize].components[compIndex % ChunkSize];
    
    /// <exception cref="NullReferenceException"> if entity has no <see cref="Rotation"/></exception>
    [Browse(Never)] public  ref Rotation    Rotation    => ref archetype.std.rotation.chunks[compIndex / ChunkSize].components[compIndex % ChunkSize];
    
    /// <exception cref="NullReferenceException"> if entity has no <see cref="Scale3"/></exception>
    [Browse(Never)] public  ref Scale3      Scale3      => ref archetype.std.scale3.  chunks[compIndex / ChunkSize].components[compIndex % ChunkSize];
    
    [Browse(Never)] public  bool            HasName         => archetype.std.name              != null;
    [Browse(Never)] public  bool            HasPosition     => archetype.std.position          != null;
    [Browse(Never)] public  bool            HasRotation     => archetype.std.rotation          != null;
    [Browse(Never)] public  bool            HasScale3       => archetype.std.scale3            != null;
    #endregion
    
#region public properties - tree nodes
    [Browse(Never)] public int              ChildCount  => archetype.gameEntityStore.Nodes[id].childCount;
    
                    /// <returns>
                    /// null if the entity has no parent.<br/>
                    /// <i>Note:</i>The <see cref="GameEntityStore"/>.<see cref="GameEntityStore.StoreRoot"/> returns always null
                    /// </returns>
                    /// <remarks>Executes in O(1)</remarks> 
                    public GameEntity       Parent      => archetype.gameEntityStore.GetParent(id);
    
                    /// <summary>
                    /// Use <b>ref</b> variable when iterating with <b>foreach</b> to copy struct copy. E.g. 
                    /// <code>
                    ///     foreach (ref var node in entity.ChildNodes)
                    /// </code>
                    /// </summary>
                    /// <remarks>Executes in O(1)</remarks>
                    public ChildNodes       ChildNodes  => archetype.gameEntityStore.GetChildNodes(id);
                    
    [Browse(Never)] public ReadOnlySpan<int> ChildIds   => archetype.gameEntityStore.GetChildIds(id);
    #endregion
    
#region internal fields
    [Browse(Never)] internal readonly   int                     id;                 //  4
    
    /// <summary>The <see cref="Archetype"/> used to store the components of they the entity</summary>
    [Browse(Never)] internal            Archetype               archetype;          //  8 - null if detached. See property Archetype

    /// <summary>The index within the <see cref="archetype"/> the entity is stored</summary>
    /// <remarks>The index will change if entity is moved to another <see cref="Archetype"/></remarks>
    [Browse(Never)] internal            int                     compIndex;          //  4
    
    [Browse(Never)] internal            int                     behaviorIndex;      //  4
    
    [Browse(Never)] private NotifyCollectionChangedEventHandler collectionChanged;  //  8  not nice having this for every GameEntity - check alternative
    
    // [c# - What is the memory overhead of a .NET Object - Stack Overflow]     // 16 overhead for reference type on x64
    // https://stackoverflow.com/questions/10655829/what-is-the-memory-overhead-of-a-net-object/10655864#10655864
    
    #endregion
    
#region constructor
    internal GameEntity(int id, Archetype archetype) {
        this.id         = id;
        this.archetype  = archetype;
        behaviorIndex   = GameEntityUtils.NoBehaviors;
    }
    #endregion

    // ------------------------------------ component methods ------------------------------------
#region component methods
    public  bool    HasComponent<T> ()  where T : struct, IComponent  => archetype.heapMap[StructHeap<T>.StructIndex] != null;

    /// <exception cref="NullReferenceException"> if entity has no component of Type <typeparamref name="T"/></exception>
    /// <remarks>Executes in O(1)</remarks>
    public  ref T   GetComponent<T>()   where T : struct, IComponent
    => ref ((StructHeap<T>)archetype.heapMap[StructHeap<T>.StructIndex]).chunks[compIndex / ChunkSize].components[compIndex % ChunkSize];
    
    /// <remarks>Executes in O(1)</remarks>
    public bool     TryGetComponent<T>(out T result) where T : struct, IComponent
    {
        var heap = archetype.heapMap[StructHeap<T>.StructIndex];
        if (heap == null) {
            result = default;
            return false;
        }
        result = ((StructHeap<T>)heap).chunks[compIndex / ChunkSize].components[compIndex % ChunkSize];
        return true;
    }
    
    /// <returns>true if component is newly added to the entity</returns>
    /// <remarks>Executes in O(1)</remarks>
    public bool AddComponent<T>()               where T : struct, IComponent => archetype.store.AddComponent<T>(id, ref archetype, ref compIndex, default);
    
    /// <returns>true if component is newly added to the entity</returns>
    /// <remarks>Executes in O(1)</remarks>
    public bool AddComponent<T>(in T component) where T : struct, IComponent => archetype.store.AddComponent(id, ref archetype, ref compIndex, in component);
    
    /// <returns>true if entity contained a component of the given type before</returns>
    /// <remarks>Executes in O(1)</remarks>
    public bool RemoveComponent<T>()            where T : struct, IComponent => archetype.store.RemoveComponent(id, ref archetype, ref compIndex, StructHeap<T>.StructIndex);
    
    /// <summary>
    /// Property is only used to display components in the Debugger.<br/>
    /// It has poor performance as is creates an array and boxes all components. 
    /// </summary>
    /// <remarks>
    /// To access a component use <see cref="GetComponent{T}"/>
    /// </remarks>
    [Obsolete($"use {nameof(GetComponent)}<T>() to access a component")]
    public      IComponent[]            Components_         => GameEntityUtils.GetComponentsDebug(this);
    #endregion
    
    // ------------------------------------ behavior methods -------------------------------------
#region behavior methods
    public      ReadOnlySpan<Behavior>  Behaviors           => new (GameEntityUtils.GetBehaviors(this));

    /// <returns>the <see cref="Behavior"/> of Type <typeparamref name="T"/>. Otherwise null</returns>
    public T    GetBehavior<T>()        where T : Behavior  => (T)GameEntityUtils.GetBehavior(this, typeof(T));
    
    /// <returns>true if the entity has a <see cref="Behavior"/> of Type <typeparamref name="T"/>. Otherwise false</returns>
    public bool TryGetBehavior<T>(out T result)
        where T : Behavior
    {
        result = (T)GameEntityUtils.GetBehavior(this, typeof(T));
        return result != null;
    }
    /// <returns>the <see cref="Behavior"/> previously added to the entity.</returns>
    public T AddBehavior<T>(T behavior) where T : Behavior  => (T)GameEntityUtils.AddBehavior(this, behavior, typeof(T), ClassType<T>.BehaviorIndex);
    /// <returns>the <see cref="Behavior"/> previously added to the entity.</returns>
    public T RemoveBehavior<T>()        where T : Behavior  => (T)GameEntityUtils.RemoveBehavior(this, typeof(T));
    #endregion
    
    // ------------------------------------ entity tag methods -----------------------------------
#region entity tag methods
    /// <returns>
    /// A copy of the <see cref="Tags"/> assigned to the <see cref="GameEntity"/>.<br/>
    /// <br/>
    /// Modifying the returned <see cref="Tags"/> value does <b>not</b> affect the <see cref="GameEntity"/>.<see cref="Tags"/>.<br/>
    /// Therefore use <see cref="AddTag{T}"/>, <see cref="AddTags"/>, <see cref="RemoveTag{T}"/> or <see cref="RemoveTags"/>.
    /// </returns>
    public ref readonly Tags    Tags                        => ref archetype.tags;
    // Note: no query Tags methods like HasTag<T>() here by intention. Tags offers query access
    public bool AddTag<T>()    where T : struct, IEntityTag => archetype.store.AddTags(Tags.Get<T>(), id, ref archetype, ref compIndex);
    public bool AddTags(in Tags tags)                       => archetype.store.AddTags(tags, id, ref archetype, ref compIndex);
    public bool RemoveTag<T>() where T : struct, IEntityTag => archetype.store.RemoveTags(Tags.Get<T>(), id, ref archetype, ref compIndex);
    public bool RemoveTags(in Tags tags)                    => archetype.store.RemoveTags(tags, id, ref archetype, ref compIndex);
    #endregion
    
    // ------------------------------------ tree methods -----------------------------------------
#region tree node methods
    /// <remarks>
    /// Executes in O(1).<br/>If its <see cref="TreeMembership"/> changes O(number of nodes in sub tree).<br/>
    /// The subtree structure of the added entity remains unchanged<br/>
    /// </remarks>
    public void AddChild(GameEntity entity) {
        var store = archetype.gameEntityStore;
        if (store != entity.archetype.store) throw EntityStore.InvalidStoreException(nameof(entity));
        store.AddChild(id, entity.id);
    }
    
    /// <remarks>
    /// Executes in O(1).<br/>If its <see cref="TreeMembership"/> changes O(number of nodes in sub tree).<br/>
    /// The subtree structure of the added entity remains unchanged<br/>
    /// </remarks>
    public void InsertChild(int index, GameEntity entity) {
        var store = archetype.gameEntityStore;
        if (store != entity.archetype.store) throw EntityStore.InvalidStoreException(nameof(entity));
        store.InsertChild(id, index, entity.id);
    }
    
    /// <remarks>
    /// Executes in O(1).<br/>If its <see cref="TreeMembership"/> changes (in-tree / floating) O(number of nodes in sub tree).<br/>
    /// The subtree structure of the removed entity remains unchanged<br/>
    /// </remarks>
    public void RemoveChild(GameEntity entity) {
        var store = archetype.gameEntityStore;
        if (store != entity.archetype.store) throw EntityStore.InvalidStoreException(nameof(entity));
        store.RemoveChild(id, entity.id);
    }
    
    /// <summary>
    /// Remove the entity from its <see cref="GameEntityStore"/>.<br/>
    /// The deleted instance is in <see cref="detached"/> state.
    /// Calling <see cref="GameEntity"/> methods result in <see cref="NullReferenceException"/>'s
    /// </summary>
    public void DeleteEntity()
    {
        var store = archetype.gameEntityStore;
        store.DeleteNode(id);
        if (archetype != store.defaultArchetype) {
            archetype.MoveLastComponentsTo(compIndex);
        }
        archetype = null;
    }
    
    public int GetChildByIndex(int index) {
        var childIds = archetype.gameEntityStore.GetNodeById(id).childIds;
        return archetype.gameEntityStore.GetNodeById(childIds[index]).id;
    }
    
    public int GetChildIndex(int childId)
    {
        var childIds    = archetype.gameEntityStore.GetNodeById(id).childIds;
        var count       = ChildCount;
        for (int n = 0; n < count; n++) {
            if (childId != childIds[n]) {
                continue;
            }
            return n;
        }
        return -1;
    }
    #endregion
}
