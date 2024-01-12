// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using static Friflo.Engine.ECS.StoreOwnership;
using static Friflo.Engine.ECS.TreeMembership;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

namespace Friflo.Engine.ECS;

/// <summary>
/// An <see cref="Entity"/> represent an object in an <see cref="EntityStore"/> - e.g. a game scene.<br/>
/// Every <see cref="Entity"/> has an <see cref="Id"/> and is a container of
/// <see cref="ECS.Tags"/>, <see cref="IComponent"/>'s, <see cref="Script"/>'s and other child <see cref="Entity"/>'s.<br/>
/// <br/>
/// Comparison to other game engines.
/// <list type="bullet">
///     <item>
///         <b>Unity</b>  - an <see cref="Entity"/> provides a similar features set as a <c>GameObject</c> and their ECS <c>Entity</c>.
///     </item>
///     <item>
///         <b>Godot</b>  - <see cref="Entity"/> is the counterpart of a <c>Node</c>.<br/>
///         The key difference is Godot is an OOP architecture inheriting from <c>Node</c> over multiple levels.
///     </item>
///     <item>
///         <b>FLAX</b>   - <see cref="Entity"/> is the counterpart of an <c>Actor</c> - an OOP architecture like Godot.
///     </item>
///     <item>
///         <b>STRIDE</b> - <see cref="Entity"/> is the counterpart of a STRIDE <c>Entity</c> - a component based architecture like Unity.<br/>
///         In contrast to this engine or Unity it has no ECS architecture - Entity Component System.
///     </item>
/// </list>
/// <br/>
/// An <see cref="Entity"/> is typically an object that can be rendered on screen like a cube, sphere, capsule, mesh, sprite, ... .<br/>
/// Therefore a renderable component needs to be added with <see cref="AddComponent{T}()"/> to an <see cref="Entity"/>.<br/>
/// <br/>
/// An <see cref="Entity"/> can be added to another <see cref="Entity"/> using <see cref="AddChild"/>.<br/>
/// The added <see cref="Entity"/> becomes a child of the <see cref="Entity"/> it is added to - its <see cref="Parent"/>.<br/>
/// This enables to build up a complex game scene with a hierarchy of <see cref="Entity"/>'s.<br/>
/// The order of children contained by an entity is the insertion order.<br/>  
/// <br/>
/// A <see cref="Script"/>'s can be added to an <see cref="Entity"/> to add custom logic (script) and data to an entity.<br/>
/// <see cref="Script"/>'s are added or removed with <see cref="AddScript{T}"/> / <see cref="RemoveScript{T}"/>.<br/>
/// <br/>
/// <see cref="Tags"/> can be added to an <see cref="Entity"/> to enable filtering entities in queries.<br/>
/// By adding <see cref="Tags"/> to an <see cref="ArchetypeQuery"/> it can be restricted to return only entities matching the
/// these <see cref="Tags"/>.
/// </summary>
/// <remarks>
/// <b>general</b>
/// <list type="bullet">
///     <item><see cref="Id"/></item>
///     <item><see cref="Pid"/></item>
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
///     <item><see cref="Components"/></item>
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
///     <item><see cref="ChildEntities"/></item>
///     <item><see cref="ChildIds"/></item>
///     <item><see cref="ChildCount"/></item>
///     <item><see cref="AddChild"/></item>
///     <item><see cref="InsertChild"/></item>
///     <item><see cref="RemoveChild"/></item>
///     <item><see cref="DeleteEntity"/></item>
///     <item><see cref="GetChildIndex"/></item>
/// </list>
/// </remarks>
[CLSCompliant(true)]
public readonly struct Entity : IEquatable<Entity>
{
    // ----------------------------------- general properties -------------------------------------
#region general - properties
    public              long                    Pid             => store.nodes[Id].pid;
                    
    public              EntityComponents        Components      => new EntityComponents(this);
                    
    public              ReadOnlySpan<Script>    Scripts         => new (EntityUtils.GetScripts(this));

    /// <returns>
    /// A copy of the <see cref="Tags"/> assigned to the <see cref="Entity"/>.<br/>
    /// <br/>
    /// Modifying the returned <see cref="Tags"/> value does <b>not</b> affect the <see cref="Entity"/>.<see cref="Tags"/>.<br/>
    /// Therefore use <see cref="AddTag{T}"/>, <see cref="AddTags"/>, <see cref="RemoveTag{T}"/> or <see cref="RemoveTags"/>.
    /// </returns>
    public     ref readonly Tags                Tags            => ref archetype.tags;

    /// <remarks>The <see cref="Archetype"/> the entity is stored.<br/>Return null if the entity is <see cref="detached"/></remarks>
    public                  Archetype           Archetype       => archetype;
    
    /// <remarks>The <see cref="Store"/> the entity is <see cref="attached"/> to. Returns null if <see cref="detached"/></remarks>
    [Browse(Never)] public  EntityStore         Store           => archetype?.entityStore;
                    
    /// <remarks>If <see cref="attached"/> its <see cref="Store"/> and <see cref="Archetype"/> are not null. Otherwise null.</remarks>
    [Browse(Never)] public  StoreOwnership      StoreOwnership  => archetype != null ? attached : detached;
    
    /// <returns>
    /// <see cref="treeNode"/> if the entity is member of the <see cref="EntityStore"/> tree graph.<br/>
    /// Otherwise <see cref="floating"/></returns>
    [Browse(Never)] public  TreeMembership      TreeMembership  => archetype.entityStore.GetTreeMembership(Id);
    
    [Browse(Never)] public  bool                IsNull          => store?.nodes[Id].archetype == null;
    
    /// <summary> Counterpart of <see cref="Serialize.DataEntity.DebugJSON"/> </summary>
    // Assigning JSON in a Debugger does not change the entity state as a developer would expect. So setter is only internal.   
                    public  string              DebugJSON { get => EntityUtils.EntityToJSON(this); internal set => EntityUtils.JsonToEntity(this, value);  }
    #endregion




    // ------------------------------------ component properties ----------------------------------
#region component - properties

    /// <exception cref="NullReferenceException"> if entity has no <see cref="EntityName"/></exception>
    [Browse(Never)] public  ref EntityName      Name        => ref archetype.std.name.    components[compIndex];

    /// <exception cref="NullReferenceException"> if entity has no <see cref="Position"/></exception>
    [Browse(Never)] public  ref Position        Position    => ref archetype.std.position.components[compIndex];
    
    /// <exception cref="NullReferenceException"> if entity has no <see cref="Rotation"/></exception>
    [Browse(Never)] public  ref Rotation        Rotation    => ref archetype.std.rotation.components[compIndex];
    
    /// <exception cref="NullReferenceException"> if entity has no <see cref="Scale3"/></exception>
    [Browse(Never)] public  ref Scale3          Scale3      => ref archetype.std.scale3.  components[compIndex];
    
    [Browse(Never)] public  bool                HasName     =>     archetype.std.name              != null;
    [Browse(Never)] public  bool                HasPosition =>     archetype.std.position          != null;
    [Browse(Never)] public  bool                HasRotation =>     archetype.std.rotation          != null;
    [Browse(Never)] public  bool                HasScale3   =>     archetype.std.scale3            != null;
    #endregion




    // ------------------------------------ child / tree properties -------------------------------
#region child / tree - properties
    [Browse(Never)] public  int                 ChildCount  => archetype.entityStore.nodes[Id].childCount;
    
    /// <returns>
    /// null if the entity has no parent.<br/>
    /// <i>Note:</i>The <see cref="EntityStore"/>.<see cref="EntityStore.StoreRoot"/> returns always null
    /// </returns>
    /// <remarks>Executes in O(1)</remarks> 
                    public  Entity              Parent      => EntityStore.GetParent(archetype.entityStore, Id);
    
    /// <summary>
    /// Return all child <see cref="Entity"/>'s. Enumerate with: 
    /// <code>
    ///     foreach (var child in entity.ChildEntities)
    /// </code>
    /// </summary>
    /// <remarks>Executes in O(1)</remarks>
                    public  ChildEntities       ChildEntities   => EntityStore.GetChildEntities(archetype.entityStore, Id);
                    
    [Browse(Never)] public  ReadOnlySpan<int>   ChildIds        => EntityStore.GetChildIds(archetype.entityStore, Id);
    #endregion




    // ------------------------------------ fields ------------------------------------------------
#region public / internal - fields
    // Note! Must not have any other fields to keep its size at 16 bytes
    /// <summary>Unique entity id.<br/>
    /// Uniqueness relates to the <see cref="Entity"/>'s stored in its <see cref="EntityStore"/></summary>
    // ReSharper disable once InconsistentNaming
                    public      readonly    int         Id;     //  4
    [Browse(Never)] internal    readonly    EntityStore store;  //  8
    #endregion
    
#region constructor
    internal Entity(int id, EntityStore store) {
        this.Id     = id;
        this.store  = store;
    }
    #endregion




    // ------------------------------------ component methods -------------------------------------
#region component - methods
    public  bool    HasComponent<T> ()  where T : struct, IComponent  => archetype.heapMap[StructHeap<T>.StructIndex] != null;

    /// <exception cref="NullReferenceException"> if entity has no component of Type <typeparamref name="T"/></exception>
    /// <remarks>Executes in O(1)</remarks>
    public  ref T   GetComponent<T>()   where T : struct, IComponent
    => ref ((StructHeap<T>)archetype.heapMap[StructHeap<T>.StructIndex]).components[compIndex];
    
    /// <remarks>Executes in O(1)</remarks>
    public bool     TryGetComponent<T>(out T result) where T : struct, IComponent
    {
        var heap = archetype.heapMap[StructHeap<T>.StructIndex];
        if (heap == null) {
            result = default;
            return false;
        }
        result = ((StructHeap<T>)heap).components[compIndex];
        return true;
    }
    
    /// <returns>true if component is newly added to the entity</returns>
    /// <remarks>Executes in O(1)<br/>
    /// <remarks>Note: Use <see cref="EntityUtils.AddEntityComponent"/> as non generic alternative</remarks>
    /// </remarks>
    public bool AddComponent<T>()               where T : struct, IComponent {
        int archIndex = 0;
        return EntityStoreBase.AddComponent<T>(Id, StructHeap<T>.StructIndex, ref refArchetype, ref refCompIndex, ref archIndex, default);
    }

    /// <returns>true if component is newly added to the entity</returns>
    /// <remarks>Executes in O(1)</remarks>
    public bool AddComponent<T>(in T component) where T : struct, IComponent {
        int archIndex = 0;
        return EntityStoreBase.AddComponent   (Id, StructHeap<T>.StructIndex, ref refArchetype, ref refCompIndex, ref archIndex, in component);
    }

    /// <returns>true if entity contained a component of the given type before</returns>
    /// <remarks>
    /// Executes in O(1)<br/>
    /// <remarks>Note: Use <see cref="EntityUtils.RemoveEntityComponent"/> as non generic alternative</remarks>
    /// </remarks>
    public bool RemoveComponent<T>()            where T : struct, IComponent {
        int archIndex = 0;
        return EntityStoreBase.RemoveComponent(Id, ref refArchetype, ref refCompIndex, ref archIndex, StructHeap<T>.StructIndex);
    }
    #endregion




    // ------------------------------------ script methods ----------------------------------------
#region script - methods
    /// <returns>The <see cref="Script"/> of Type <typeparamref name="T"/>. Otherwise null</returns>
    /// <remarks>Note: Use <see cref="EntityUtils.GetEntityScript"/> as non generic alternative</remarks> 
    public T    GetScript<T>()        where T : Script  => (T)EntityUtils.GetScript(this, typeof(T));
    
    /// <returns>true if the entity has a <see cref="Script"/> of Type <typeparamref name="T"/>. Otherwise false</returns>
    public bool TryGetScript<T>(out T result)
        where T : Script
    {
        result = (T)EntityUtils.GetScript(this, typeof(T));
        return result != null;
    }
    /// <returns>the <see cref="Script"/> previously added to the entity.</returns>
    /// <remarks>Note: Use <see cref="EntityUtils.AddNewEntityScript"/> as non generic alternative</remarks>
    public T AddScript<T>(T script)   where T : Script  => (T)EntityUtils.AddScript    (this, ClassType<T>.ScriptIndex, script);
    
    /// <returns>the <see cref="Script"/> previously added to the entity.</returns>
    /// <remarks>Note: Use <see cref="EntityUtils.RemoveEntityScript"/> as non generic alternative</remarks>
    public T RemoveScript<T>()        where T : Script  => (T)EntityUtils.RemoveScript (this, ClassType<T>.ScriptIndex);    
    #endregion




    // ------------------------------------ tag methods -------------------------------------------
#region tag - methods
    // Note: no query Tags methods like HasTag<T>() here by intention. Tags offers query access
    public bool AddTag<T>()    where T : struct, ITag {
        int index = 0;
        return EntityStoreBase.AddTags   (archetype.store, Tags.Get<T>(), Id, ref refArchetype, ref refCompIndex, ref index);
    }

    public bool AddTags(in Tags tags) {
        int index = 0;
        return EntityStoreBase.AddTags   (archetype.store, tags,          Id, ref refArchetype, ref refCompIndex, ref index);
    }

    public bool RemoveTag<T>() where T : struct, ITag {
        int index = 0;
        return EntityStoreBase.RemoveTags(archetype.store, Tags.Get<T>(), Id, ref refArchetype, ref refCompIndex, ref index);
    }

    public bool RemoveTags(in Tags tags) {
        int index = 0;
        return EntityStoreBase.RemoveTags(archetype.store, tags,          Id, ref refArchetype, ref refCompIndex, ref index);
    }
    #endregion




    // ------------------------------------ child / tree methods ----------------------------------
#region child / tree - methods
    /// <remarks>
    /// Executes in O(1).<br/>If its <see cref="TreeMembership"/> changes O(number of nodes in sub tree).<br/>
    /// The subtree structure of the added entity remains unchanged<br/>
    /// </remarks>
    /// <returns>
    /// The index within <see cref="ChildIds"/> the <paramref name="entity"/> is added.<br/>
    /// -1 if the <paramref name="entity"/> is already a child entity.
    /// </returns>
    public int AddChild(Entity entity) {
        var entityStore = archetype.entityStore;
        if (entityStore != entity.archetype.store) throw EntityStoreBase.InvalidStoreException(nameof(entity));
        return entityStore.AddChild(Id, entity.Id);
    }
    
    /// <remarks>
    /// Executes in O(1) in case <paramref name="index"/> == <see cref="ChildCount"/>.<br/>
    /// Otherwise O(N). N = <see cref="ChildCount"/> - <paramref name="index"/><br/>
    /// If its <see cref="TreeMembership"/> changes O(number of nodes in sub tree).<br/>
    /// The subtree structure of the added entity remains unchanged<br/>
    /// </remarks>
    public void InsertChild(int index, Entity entity) {
        var entityStore = archetype.entityStore;
        if (entityStore != entity.archetype.store) throw EntityStoreBase.InvalidStoreException(nameof(entity));
        entityStore.InsertChild(Id, entity.Id, index);
    }
    
    /// <remarks>
    /// Executes in O(N) to search the entity. N = <see cref="ChildCount"/><br/>
    /// If its <see cref="TreeMembership"/> changes (in-tree / floating) O(number of nodes in sub tree).<br/>
    /// The subtree structure of the removed entity remains unchanged<br/>
    /// </remarks>
    public bool RemoveChild(Entity entity) {
        var entityStore = archetype.entityStore;
        if (entityStore != entity.archetype.store) throw EntityStoreBase.InvalidStoreException(nameof(entity));
        return entityStore.RemoveChild(Id, entity.Id);
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
        var arch            = archetype;
        var componentIndex  = compIndex; 
        var entityStore = arch.entityStore;
        entityStore.DeleteNode(Id); 
        if (arch != entityStore.defaultArchetype) {
            Archetype.MoveLastComponentsTo(arch, componentIndex);
        }
    }

    public int  GetChildIndex(Entity child)     => archetype.entityStore.GetChildIndex(Id, child.Id);    
    #endregion




    // ------------------------------------ general methods ---------------------------------------
#region general - methods
    public static   bool    operator == (Entity a, Entity b)    => a.Id == b.Id && a.store == b.store;
    public static   bool    operator != (Entity a, Entity b)    => a.Id != b.Id || a.store != b.store;

    // --- IEquatable<T>
    public          bool    Equals(Entity other)                => Id == other.Id && store == other.store;

    // --- object
    /// <summary> Note: Not implemented to avoid excessive boxing. </summary>
    /// <remarks> Use <see cref="operator=="/> or <see cref="EntityUtils.EqualityComparer"/> </remarks>
    public override bool    Equals(object obj)  => throw EntityUtils.NotImplemented(Id, "== Equals(Entity)");
    
    /// <summary> Note: Not implemented to avoid excessive boxing. </summary>
    /// <remarks> Use <see cref="Id"/> or <see cref="EntityUtils.EqualityComparer"/> </remarks>
    public override int     GetHashCode()       => throw EntityUtils.NotImplemented(Id, nameof(Id));
    
    public override string  ToString()          => EntityUtils.EntityToString(this);
    #endregion

    
    // ------------------------------------ event methods -----------------------------------------
#region event - methods
    [Obsolete("Experimental")]
    public event Action<TagsChangedArgs>    OnTagsChanged     { add     => EntityStoreBase.AddEntityTagsChangedHandler   (store, Id, value);
                                                                remove  => EntityStoreBase.RemoveEntityTagsChangedHandler(store, Id, value);  }

    [Obsolete("Experimental")]
    public void AddHandler   <TEvent> (Action<TEvent> handler) where TEvent : struct  {  }
    
    [Obsolete("Experimental")]
    public void RemoveHandler<TEvent> (Action<TEvent> handler) where TEvent : struct  {  }
    #endregion

    // ------------------------------------ internal properties -----------------------------------
// ReSharper disable InconsistentNaming - placed on bottom to disable all subsequent hints
#region internal - properties
    /// <summary>The <see cref="Archetype"/> used to store the components of they the entity</summary>
    [Browse(Never)] internal    ref Archetype   refArchetype    => ref store.nodes[Id].archetype;
    [Browse(Never)] internal        Archetype      archetype    =>     store.nodes[Id].archetype;

    /// <summary>The index within the <see cref="refArchetype"/> the entity is stored</summary>
    /// <remarks>The index will change if entity is moved to another <see cref="Archetype"/></remarks>
    [Browse(Never)] internal    ref int         refCompIndex    => ref store.nodes[Id].compIndex;
    [Browse(Never)] internal        int            compIndex    =>     store.nodes[Id].compIndex;
    
    [Browse(Never)] internal    ref int         refScriptIndex  => ref store.nodes[Id].scriptIndex;
    [Browse(Never)] internal        int            scriptIndex  =>     store.nodes[Id].scriptIndex;

    // Deprecated comment. Was valid when Entity was a class
    // [c# - What is the memory overhead of a .NET Object - Stack Overflow]     // 16 overhead for reference type on x64
    // https://stackoverflow.com/questions/10655829/what-is-the-memory-overhead-of-a-net-object/10655864#10655864
    #endregion
}
