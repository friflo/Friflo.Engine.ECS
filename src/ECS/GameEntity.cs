// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;
using static Friflo.Fliox.Engine.ECS.StructInfo;
using static Friflo.Fliox.Engine.ECS.TreeGraphMembership;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// Has currently no id - if doing so the type id is fixed  
/// </summary>
/// <remarks>
/// <b>general</b>
/// <list type="bullet">
///     <item><see cref="Id"/></item>
///     <item><see cref="Archetype"/></item>
///     <item><see cref="ComponentCount"/></item>
///     <item><see cref="StoreOwnership"/></item>
///     <item><see cref="TreeGraphMembership"/></item>
/// </list>
/// <b>struct components</b> · generic
/// <list type="bullet">
///     <item><see cref="HasComponent{T}"/></item>
///     <item><see cref="ComponentRef{T}"/> - read / write</item>
///     <item><see cref="TryGetComponent{T}"/></item>
///     <item><see cref="AddComponent{T}()"/></item>
///     <item><see cref="RemoveComponent{T}"/></item>
/// </list>
/// <b>struct components</b> · common
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
/// <b>class components</b> · generic
/// <list type="bullet">
///     <item><see cref="ClassComponents"/></item>
///     <item><see cref="GetClassComponent{T}"/></item>
///     <item><see cref="TryGetClassComponent{T}"/></item>
///     <item><see cref="AddClassComponent{T}"/></item>
///     <item><see cref="RemoveClassComponent{T}"/></item>
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
///     <item><see cref="GraphOrigin"/></item>
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
public sealed class GameEntity
{
#region public properties
    /// <summary>Unique entity id.<br/>
    /// Uniqueness relates to the <see cref="GameEntity"/>'s stored in its <see cref="GameEntityStore"/></summary>
                    public   int            Id              => id;

    [Browse(Never)] public   int            ComponentCount  => archetype.structCount + classComponents.Length;
    
    /// <remarks>The <see cref="Archetype"/> the entity is stored.<br/>Return null if the entity is <see cref="detached"/></remarks>
                    public   Archetype      Archetype       => archetype;
                    
    /// <remarks>If <see cref="attached"/> <see cref="Archetype"/> is not null. Otherwise null.</remarks>
    [Browse(Never)] public  StoreOwnership  StoreOwnership  => archetype != null ? attached : detached;
    
    /// <returns>
    /// <see cref="graphNode"/> if the entity is member of the <see cref="GameEntityStore"/> tree graph.<br/>
    /// Otherwise <see cref="floating"/></returns>
    /// <remarks>
    /// If <see cref="TreeGraphMembership"/> is <see cref="graphNode"/> its <see cref="GraphOrigin"/> is not null.<br/>
    /// If <see cref="floating"/> its <see cref="GraphOrigin"/> is null.
    /// </remarks>
    [Browse(Never)] public  TreeGraphMembership  TreeGraphMembership  => archetype.gameEntityStore.GetTreeGraphMembership(id);
    
    [Browse(Never)]
    public   ReadOnlySpan<ClassComponent>   ClassComponents => new (classComponents);
    
    /// <summary>
    /// Property is only to display <b>struct</b> and <b>class</b> components in the Debugger.<br/>
    /// It has poor performance as is creates an array and boxes all struct components. 
    /// </summary>
    /// <remarks>
    /// To access <b>class</b>  components use <see cref="GetClassComponent{T}"/> or <see cref="ClassComponents"/><br/>
    /// To access <b>struct</b> components use <see cref="ComponentRef{T}"/>
    /// </remarks>
    [Obsolete("use either GetClassComponent() or GetComponentValue()")]
    public  object[]                        Components_     => GameEntityUtils.GetComponentsDebug(this);
    
    public override string                  ToString()      => GameEntityUtils.GameEntityToString(this, new StringBuilder());

    #endregion

#region public properties - struct components
    
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
                    public  bool            HasComponent<T> () where T : struct, IStructComponent
                                                            => archetype.heapMap[StructHeap<T>.StructIndex] != null;
    #endregion
    
#region public properties - tree nodes
    [Browse(Never)] public int              ChildCount  => archetype.gameEntityStore.Nodes[id].childCount;
    
                    /// <returns>return null if the entity is <see cref="floating"/></returns>
                    /// <remarks>Executes in O(1) independent from its depth in the node tree</remarks>
                    public GameEntity       GraphOrigin => archetype.gameEntityStore.GetGraphOrigin(id);
                    
                    /// <returns>
                    /// null if the entity has no parent.<br/>
                    /// <i>Note:</i>The <see cref="GameEntityStore"/>.<see cref="GameEntityStore.GraphOrigin"/> returns always null
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
    [Browse(Never)] internal readonly   int                 id;                 //  4
    
    /// <summary>The <see cref="Archetype"/> used to store the struct components of they the entity</summary>
    [Browse(Never)] internal            Archetype           archetype;          //  8 - null if detached. See property Archetype

    /// <summary>The index within the <see cref="archetype"/> the entity is stored</summary>
    /// <remarks>The index will change if entity is moved to another <see cref="Archetype"/></remarks>
    [Browse(Never)] internal            int                 compIndex;          //  4
    
    /// <summary>Container of class type components added to the entity</summary>
    [Browse(Never)] internal            ClassComponent[]    classComponents;    //  8 - never null
    
    // [c# - What is the memory overhead of a .NET Object - Stack Overflow]     // 16 overhead for reference type on x64
    // https://stackoverflow.com/questions/10655829/what-is-the-memory-overhead-of-a-net-object/10655864#10655864
    
    #endregion
    
#region constructor
    internal GameEntity(int id, Archetype archetype) {
        this.id         = id;
        this.archetype  = archetype;
        classComponents = GameEntityUtils.EmptyComponents;
    }
    #endregion

    // --------------------------------- struct component methods --------------------------------
#region struct component methods
    /// <exception cref="NullReferenceException"> if entity has no component of Type <typeparamref name="T"/></exception>
    /// <remarks>Executes in O(1)</remarks>
    public  ref T        ComponentRef<T>()
        where T : struct, IStructComponent
    {
        var heap = (StructHeap<T>)archetype.heapMap[StructHeap<T>.StructIndex];
        return ref heap.chunks[compIndex / ChunkSize].components[compIndex % ChunkSize];
    }
    
    /// <remarks>Executes in O(1)</remarks>
    public bool TryGetComponent<T>(out T result)
        where T : struct, IStructComponent
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
    public bool AddComponent<T>()
        where T : struct, IStructComponent
    {
        return archetype.store.AddComponent<T>(id, ref archetype, ref compIndex, default);
    }
    
    /// <returns>true if component is newly added to the entity</returns>
    /// <remarks>Executes in O(1)</remarks>
    public bool AddComponent<T>(in T component)
        where T : struct, IStructComponent
    {
        return archetype.store.AddComponent(id, ref archetype, ref compIndex, in component);
    }
    
    /// <returns>true if entity contained a component of the given type before</returns>
    /// <remarks>Executes in O(1)</remarks>
    public bool RemoveComponent<T>()
        where T : struct, IStructComponent
    {
        return archetype.store.RemoveComponent(id, ref archetype, ref compIndex, StructHeap<T>.StructIndex);
    }
    #endregion
    
    // --------------------------------- class component methods ---------------------------------
#region class component methods
    /// <returns>the entity component of Type <typeparamref name="T"/>. Otherwise null</returns>
    public T    GetClassComponent<T>()
        where T : ClassComponent
    => (T)GameEntityUtils.GetClassComponent(this, typeof(T));
    
    /// <returns>true if the entity has component of Type <typeparamref name="T"/>. Otherwise false</returns>
    public bool TryGetClassComponent<T>(out T result)
        where T : ClassComponent
    {
        var component = GameEntityUtils.GetClassComponent(this, typeof(T));
        result = (T)component;
        return component != null;
    }
    
    /// <returns>the component previously added to the entity.</returns>
    public T AddClassComponent<T>(T component) 
        where T : ClassComponent
    => (T)GameEntityUtils.AddClassComponent(this, component, typeof(T), ClassType<T>.ClassIndex);
    
    
    /// <returns>the component previously added to the entity.</returns>
    public T RemoveClassComponent<T>()
        where T : ClassComponent
    => (T)GameEntityUtils.RemoveClassComponent(this, typeof(T));
    #endregion
    
    // ------------------------------------ entity tag methods -----------------------------------
#region entity tag methods
    /// <returns>
    /// A copy of the <see cref="Tags"/> assigned to the <see cref="GameEntity"/>.<br/>
    /// <br/>
    /// Modifying the returned <see cref="Tags"/> value does <b>not</b> affect the <see cref="GameEntity"/>.<see cref="Tags"/>.<br/>
    /// Therefore use <see cref="AddTag{T}"/>, <see cref="AddTags"/>, <see cref="RemoveTag{T}"/> or <see cref="RemoveTags"/>.
    /// </returns>
    public  ref readonly Tags    Tags                       => ref archetype.tags;
    
    // Note: no query Tags methods like HasTag<T>() here by intention. Tags offers query access

    public  bool AddTag<T>()
        where T : struct, IEntityTag
    {
        var tags = Tags.Get<T>();
        return archetype.store.AddTags(tags, id, ref archetype, ref compIndex);
    }
    
    public  bool    AddTags(in Tags tags)
    {
        return archetype.store.AddTags(tags, id, ref archetype, ref compIndex);
    }
    
    public  bool    RemoveTag<T>()
        where T : struct, IEntityTag
    {
        var tags = Tags.Get<T>();
        return archetype.store.RemoveTags(tags, id, ref archetype, ref compIndex);
    }
    
    public  bool    RemoveTags(in Tags tags)
    {
        return archetype.store.RemoveTags(tags, id, ref archetype, ref compIndex);
    }
    #endregion
    
    // --------------------------------------- tree methods --------------------------------------
#region tree node methods
    /// <remarks>
    /// Executes in O(1).<br/>If its <see cref="TreeGraphMembership"/> changes O(number of nodes in sub tree).<br/>
    /// The subtree structure of the added entity remains unchanged<br/>
    /// </remarks>
    public void AddChild(GameEntity entity) {
        var store = archetype.gameEntityStore;
        if (store != entity.archetype.store) throw EntityStore.InvalidStoreException(nameof(entity));
        store.AddChild(id, entity.id);
    }
    
    /// <remarks>
    /// Executes in O(1).<br/>If its <see cref="TreeGraphMembership"/> changes (in-tree / floating) O(number of nodes in sub tree).<br/>
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
    #endregion
}
