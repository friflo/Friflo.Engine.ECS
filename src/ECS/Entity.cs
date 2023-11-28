// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;
using static Friflo.Fliox.Engine.ECS.StructInfo;
using static Friflo.Fliox.Engine.ECS.TreeMembership;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="Entity"/> represent any kind of object in a game scene.<br/>
/// Every <see cref="Entity"/> has an <see cref="Id"/> and is a container of
/// <see cref="ECS.IComponent"/>'s, <see cref="ECS.Script"/>'s and <see cref="ECS.Tags"/><br/>
/// <br/>
/// It is typically an object that can be rendered on screen like a cube, sphere, capsule, mesh, sprite, ... .<br/>
/// Therefore a renderable component needs to be added with <see cref="AddComponent{T}()"/> to a <see cref="Entity"/>.<br/>
/// <br/>
/// A <see cref="Entity"/> can be added to another <see cref="Entity"/> using <see cref="AddChild"/>.<br/>
/// The added <see cref="Entity"/> becomes a child of the <see cref="Entity"/> it is added to - its <see cref="Parent"/>.<br/>
/// This enables to build up a complex game scene with a hierarchy of <see cref="Entity"/>'s.<br/>
/// The order of children contained by an entity is the insertion order.<br/>  
/// <br/>
/// <see cref="ECS.Script"/>'s can be added to a <see cref="Entity"/> to add custom logic (script) and data to an entity.<br/>
/// <see cref="ECS.Script"/>'s are added or removed with <see cref="AddScript{T}"/> / <see cref="RemoveScript{T}"/>.<br/>
/// <br/>
/// <see cref="Tags"/> can be added to a <see cref="Entity"/> to enable filtering entities in queries.<br/>
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
/// <b>scripts</b> · generic
/// <list type="bullet">
///     <item><see cref="Scripts"/></item>
///     <item><see cref="GetScript{T}"/></item>
///     <item><see cref="TryGetScript{T}"/></item>
///     <item><see cref="AddScript{T}"/></item>
///     <item><see cref="RemoveScript{T}"/></item>
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
///     <item><see cref="InsertChild"/></item>
///     <item><see cref="RemoveChild"/></item>
///     <item><see cref="DeleteEntity"/></item>
///     <item><see cref="GetChildIndex"/></item>
///     <item><see cref="GetChildNodeByIndex"/></item>
/// </list>
/// </remarks>
[CLSCompliant(true)]
public sealed class Entity
{
#region public properties
    /// <summary>Unique entity id.<br/>
    /// Uniqueness relates to the <see cref="Entity"/>'s stored in its <see cref="EntityStore"/></summary>
                    public   int            Id              => id;

    /// <remarks>The <see cref="Archetype"/> the entity is stored.<br/>Return null if the entity is <see cref="detached"/></remarks>
                    public   Archetype      Archetype       => archetype;
    
    /// <remarks>The <see cref="Store"/> the entity is <see cref="attached"/> to. Returns null if <see cref="detached"/></remarks>
    [Browse(Never)] public  EntityStore     Store           => archetype?.entityStore;
                    
    /// <remarks>If <see cref="attached"/> its <see cref="Store"/> and <see cref="Archetype"/> are not null. Otherwise null.</remarks>
    [Browse(Never)] public  StoreOwnership  StoreOwnership  => archetype != null ? attached : detached;
    
    /// <returns>
    /// <see cref="treeNode"/> if the entity is member of the <see cref="EntityStore"/> tree graph.<br/>
    /// Otherwise <see cref="floating"/></returns>
    [Browse(Never)] public  TreeMembership  TreeMembership  => archetype.entityStore.GetTreeMembership(id);
    
    
    [Obsolete($"use method only for debugging")]
                    public  string          DebugJSON       => EntityUtils.GetDebugJSON(this);
    
    public override string                  ToString()      => EntityUtils.EntityToString(this, new StringBuilder());

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
    
    [Browse(Never)] public  bool            HasName     =>     archetype.std.name              != null;
    [Browse(Never)] public  bool            HasPosition =>     archetype.std.position          != null;
    [Browse(Never)] public  bool            HasRotation =>     archetype.std.rotation          != null;
    [Browse(Never)] public  bool            HasScale3   =>     archetype.std.scale3            != null;
    #endregion
    
#region public properties - tree nodes
    [Browse(Never)] public int              ChildCount  => archetype.entityStore.Nodes[id].childCount;
    
                    /// <returns>
                    /// null if the entity has no parent.<br/>
                    /// <i>Note:</i>The <see cref="EntityStore"/>.<see cref="EntityStore.StoreRoot"/> returns always null
                    /// </returns>
                    /// <remarks>Executes in O(1)</remarks> 
                    public Entity           Parent      => archetype.entityStore.GetParent(id);
    
                    /// <summary>
                    /// Use <b>ref</b> variable when iterating with <b>foreach</b> to copy struct copy. E.g. 
                    /// <code>
                    ///     foreach (ref var node in entity.ChildNodes)
                    /// </code>
                    /// </summary>
                    /// <remarks>Executes in O(1)</remarks>
                    public ChildNodes       ChildNodes  => archetype.entityStore.GetChildNodes(id);
                    
    [Browse(Never)] public ReadOnlySpan<int> ChildIds   => archetype.entityStore.GetChildIds(id);
    #endregion
    
#region internal fields
    [Browse(Never)] internal readonly   int         id;             //  4
    
    /// <summary>The <see cref="Archetype"/> used to store the components of they the entity</summary>
    [Browse(Never)] internal            Archetype   archetype;      //  8 - null if detached. See property Archetype

    /// <summary>The index within the <see cref="archetype"/> the entity is stored</summary>
    /// <remarks>The index will change if entity is moved to another <see cref="Archetype"/></remarks>
    [Browse(Never)] internal            int         compIndex;      //  4
    
    [Browse(Never)] internal            int         scriptIndex;    //  4
    
    // [c# - What is the memory overhead of a .NET Object - Stack Overflow]     // 16 overhead for reference type on x64
    // https://stackoverflow.com/questions/10655829/what-is-the-memory-overhead-of-a-net-object/10655864#10655864
    
    #endregion
    
#region constructor
    internal Entity(int id, Archetype archetype) {
        this.id         = id;
        this.archetype  = archetype;
        scriptIndex     = EntityUtils.NoScripts;
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
    /// <remarks>Executes in O(1)<br/>
    /// <remarks>Note: Use <see cref="AddEntityComponent"/> as non generic alternative</remarks>
    /// </remarks>
    public bool AddComponent<T>()               where T : struct, IComponent {
        return archetype.entityStore.AddComponent<T>(id, StructHeap<T>.StructIndex, ref archetype, ref compIndex, out _, default);
    }

    /// <returns>true if component is newly added to the entity</returns>
    /// <remarks>Executes in O(1)</remarks>
    public bool AddComponent<T>(in T component) where T : struct, IComponent {
        return archetype.entityStore.AddComponent(id, StructHeap<T>.StructIndex, ref archetype, ref compIndex, out _, in component);
    }

    /// <returns>true if entity contained a component of the given type before</returns>
    /// <remarks>
    /// Executes in O(1)<br/>
    /// <remarks>Note: Use <see cref="RemoveEntityComponent"/> as non generic alternative</remarks>
    /// </remarks>
    public bool RemoveComponent<T>()            where T : struct, IComponent {
        return archetype.entityStore.RemoveComponent(id, ref archetype, ref compIndex, out _, StructHeap<T>.StructIndex);
    }

    /// <summary>
    /// Property is only used to display components in the Debugger.<br/>
    /// It has poor performance as is creates an array and boxes all components. 
    /// </summary>
    /// <remarks>
    /// To access a component use <see cref="GetComponent{T}"/>
    /// </remarks>
    [Obsolete($"use {nameof(GetComponent)}<T>() to access a component")]
    public      IComponent[]            Components_     => EntityUtils.GetComponentsDebug(this);
    #endregion
    
    // ------------------------------------ script methods -------------------------------------
#region script methods
    public      ReadOnlySpan<Script>  Scripts           => new (EntityUtils.GetScripts(this));

    /// <returns>the <see cref="Script"/> of Type <typeparamref name="T"/>. Otherwise null</returns>
    /// <remarks>Note: Use <see cref="GetEntityScript"/> as non generic alternative</remarks> 
    public T    GetScript<T>()        where T : Script  => (T)EntityUtils.GetScript(this, typeof(T));
    
    /// <returns>true if the entity has a <see cref="Script"/> of Type <typeparamref name="T"/>. Otherwise false</returns>
    public bool TryGetScript<T>(out T result)
        where T : Script
    {
        result = (T)EntityUtils.GetScript(this, typeof(T));
        return result != null;
    }
    /// <returns>the <see cref="Script"/> previously added to the entity.</returns>
    /// <remarks>Note: Use <see cref="AddEntityScript"/> as non generic alternative</remarks>
    public T AddScript<T>(T script)   where T : Script  => (T)EntityUtils.AddScript    (this, ClassType<T>.ScriptIndex, script);
    
    /// <returns>the <see cref="Script"/> previously added to the entity.</returns>
    /// <remarks>Note: Use <see cref="RemoveEntityScript"/> as non generic alternative</remarks>
    public T RemoveScript<T>()        where T : Script  => (T)EntityUtils.RemoveScript (this, ClassType<T>.ScriptIndex);
    
    #endregion
    
    // ------------------------------------ entity tag methods -----------------------------------
#region entity tag methods
    /// <returns>
    /// A copy of the <see cref="Tags"/> assigned to the <see cref="Entity"/>.<br/>
    /// <br/>
    /// Modifying the returned <see cref="Tags"/> value does <b>not</b> affect the <see cref="Entity"/>.<see cref="Tags"/>.<br/>
    /// Therefore use <see cref="AddTag{T}"/>, <see cref="AddTags"/>, <see cref="RemoveTag{T}"/> or <see cref="RemoveTags"/>.
    /// </returns>
    public ref readonly Tags    Tags                        => ref archetype.tags;
    // Note: no query Tags methods like HasTag<T>() here by intention. Tags offers query access
    public bool AddTag<T>()    where T : struct, IEntityTag {
        int index = 0;
        return archetype.store.AddTags(Tags.Get<T>(), id, ref archetype, ref compIndex, ref index);
    }

    public bool AddTags(in Tags tags) {
        int index = 0;
        return archetype.store.AddTags(tags, id, ref archetype, ref compIndex, ref index);
    }

    public bool RemoveTag<T>() where T : struct, IEntityTag {
        int index = 0;
        return archetype.store.RemoveTags(Tags.Get<T>(), id, ref archetype, ref compIndex, ref index);
    }

    public bool RemoveTags(in Tags tags) {
        int index = 0;
        return archetype.store.RemoveTags(tags, id, ref archetype, ref compIndex, ref index);
    }

    #endregion
    
    // ------------------------------------ tree methods -----------------------------------------
#region tree node methods
    /// <remarks>
    /// Executes in O(1).<br/>If its <see cref="TreeMembership"/> changes O(number of nodes in sub tree).<br/>
    /// The subtree structure of the added entity remains unchanged<br/>
    /// </remarks>
    public int AddChild(Entity entity) {
        var store = archetype.entityStore;
        if (store != entity.archetype.store) throw EntityStoreBase.InvalidStoreException(nameof(entity));
        return store.AddChild(id, entity.id);
    }
    
    /// <remarks>
    /// Executes in O(1) in case <paramref name="index"/> == <see cref="ChildCount"/>.<br/>
    /// Otherwise O(N). N = <see cref="ChildCount"/> - <paramref name="index"/><br/>
    /// If its <see cref="TreeMembership"/> changes O(number of nodes in sub tree).<br/>
    /// The subtree structure of the added entity remains unchanged<br/>
    /// </remarks>
    public void InsertChild(int index, Entity entity) {
        var store = archetype.entityStore;
        if (store != entity.archetype.store) throw EntityStoreBase.InvalidStoreException(nameof(entity));
        store.InsertChild(id, entity.id, index);
    }
    
    /// <remarks>
    /// Executes in O(N) to search the entity. N = <see cref="ChildCount"/><br/>
    /// If its <see cref="TreeMembership"/> changes (in-tree / floating) O(number of nodes in sub tree).<br/>
    /// The subtree structure of the removed entity remains unchanged<br/>
    /// </remarks>
    public bool RemoveChild(Entity entity) {
        var store = archetype.entityStore;
        if (store != entity.archetype.store) throw EntityStoreBase.InvalidStoreException(nameof(entity));
        return store.RemoveChild(id, entity.id);
    }
    
    /// <summary>
    /// Remove the entity from its <see cref="EntityStore"/>.<br/>
    /// The deleted instance is in <see cref="detached"/> state.
    /// Calling <see cref="Entity"/> methods result in <see cref="NullReferenceException"/>'s
    /// </summary>
    /// <remarks>
    /// Executes in O(1) in case the entity has no children and if it is the last entity in <see cref="Parent"/>.<see cref="ChildIds"/>
    /// </remarks>
    public void DeleteEntity()
    {
        var store = archetype.entityStore;
        store.DeleteNode(id);
        if (archetype != store.defaultArchetype) {
            archetype.MoveLastComponentsTo(compIndex);
        }
        archetype = null;
    }

    public              int         GetChildIndex(int childId)      => archetype.entityStore.GetChildIndex(id, childId);

    public ref readonly EntityNode  GetChildNodeByIndex(int index)  => ref archetype.entityStore.GetChildNodeByIndex(id, index);
    
    #endregion
    
#region non generic component methods
    /// <summary>
    /// Returns a copy of the entity component as an object.<br/>
    /// The returned <see cref="IComponent"/> is a boxed struct.<br/>
    /// So avoid using this method whenever possible. Use <see cref="GetComponent{T}"/> instead.
    /// </summary>
    public static  IComponent GetEntityComponent    (Entity entity, ComponentType componentType) {
        return entity.archetype.heapMap[componentType.structIndex].GetComponentDebug(entity.compIndex);
    }

    public static  bool       RemoveEntityComponent (Entity entity, ComponentType componentType)
    {
        return entity.archetype.entityStore.RemoveComponent(entity.id, ref entity.archetype, ref entity.compIndex, out _, componentType.structIndex);
    }
    
    public static  bool       AddEntityComponent    (Entity entity, ComponentType componentType) {
        return componentType.AddEntityComponent(entity);
    }
    #endregion
    
#region non generic script methods
    public static Script GetEntityScript    (Entity entity, ScriptType scriptType) => EntityUtils.GetScript       (entity, scriptType.type);
    public static Script RemoveEntityScript (Entity entity, ScriptType scriptType) => EntityUtils.RemoveScriptType(entity, scriptType);
    public static Script AddEntityScript    (Entity entity, ScriptType scriptType) => EntityUtils.AddScriptType   (entity, scriptType);
    #endregion
}
