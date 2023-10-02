// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;
using static Friflo.Fliox.Engine.ECS.StructUtils;
using static Friflo.Fliox.Engine.ECS.TreeMembership;
using static Friflo.Fliox.Engine.ECS.NodeFlags;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable InconsistentNaming
// ReSharper disable UseNullPropagation
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// Has currently no id - if doing so the type id is fixed  
/// </summary>
public sealed class GameEntity
{
#region public properties
    // ReSharper disable once InconsistentNaming
    /// <summary>Unique entity id.<br/>
    /// Uniqueness is relates to the <see cref="GameEntity"/>'s stored in its <see cref="EntityStore"/></summary>
                    public   int            Id              => id;

    [Browse(Never)] public   int            ComponentCount  => archetype.componentCount + classComponents.Length;
    
    /// <remarks>The <see cref="Archetype"/> the entity is stored.<br/>Return null if the entity is <see cref="detached"/></remarks>
                    public   Archetype      Archetype       => archetype;
                    
    /// <remarks>If <see cref="attached"/> <see cref="Archetype"/> is not null. Otherwise null.</remarks>
    [Browse(Never)] public  StoreOwnership  StoreOwnership  => archetype != null ? attached : detached;
    
    /// <returns><see cref="treeNode"/> if the entity is member of the <see cref="EntityStore"/> tree. Otherwise <see cref="floating"/></returns>
    /// <remarks>
    /// If <see cref="TreeMembership"/> is <see cref="treeNode"/> <see cref="Root"/> is not null.<br/>
    /// If <see cref="floating"/> <see cref="Root"/> is null.
    /// </remarks>
    [Browse(Never)] public  TreeMembership  TreeMembership  => archetype.store.nodes[id].Is(TreeNode) ? treeNode : floating;
    
    [Browse(Never)]
    public   ReadOnlySpan<ClassComponent>   ClassComponents => new (classComponents);
    
    /// <summary>
    /// Property is only to display <b>struct</b> and <b>class</b> components in the Debugger.<br/>
    /// It has poor performance as is creates an array and boxes all struct components. 
    /// </summary>
    /// <remarks>
    /// To access <b>class</b>  components use <see cref="GetClassComponent{T}"/> or <see cref="ClassComponents"/><br/>
    /// To access <b>struct</b> components use <see cref="GetComponentValue{T}"/>
    /// </remarks>
    [Obsolete("use either GetClassComponent() or GetComponentValue()")]
    public  object[]                        Components_     => GetComponentsDebug();
    
    public override string                  ToString()      => GetString(new StringBuilder());

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
    [Browse(Never)] public  bool            HasScale3       => archetype.std.scale3             != null;
                    public  bool            HasComponent<T> () where T : struct
                                                            => archetype.FindComponentHeap<T>() != null;
    #endregion

#region public properties - tree nodes
    [Browse(Never)] public  int             ChildCount  => archetype.store.Nodes[id].childCount;
    
                    /// <returns>return null if the entity is <see cref="floating"/></returns>
                    /// <remarks>Executes in O(1) independent from its depth in the node tree</remarks>
                    public  GameEntity      Root        => archetype.store.nodes[id].Is(TreeNode) ? archetype.store.Root : null;
                    
                    /// <returns>
                    /// null if the entity has no parent.<br/>
                    /// <i>Note:</i>The <see cref="EntityStore"/>.<see cref="EntityStore.Root"/> returns always null
                    /// </returns>
                    /// <remarks>Executes in O(1)</remarks> 
                    public  GameEntity      Parent
                    { get {
                        var store       = archetype.store;
                        var parentNode  = store.nodes[id].parentId;
                        return EntityStore.HasParent(parentNode) ? store.nodes[parentNode].entity : null;
                    } }
    
                    /// <summary>
                    /// Use <b>ref</b> variable when iterating with <b>foreach</b> to copy struct copy. E.g. 
                    /// <code>
                    ///     foreach (ref var node in entity.ChildNodes)
                    /// </code>
                    /// </summary>
                    /// <remarks>Executes in O(1)</remarks>
                    public  ChildNodes      ChildNodes
                    { get {
                        var store       = archetype.store;
                        ref var node    = ref store.nodes[id];
                        return new ChildNodes(store.nodes, node.childIds, node.childCount);
                    } }
    #endregion
    
#region internal fields
    [Browse(Never)] internal readonly   int                 id;                 //  4
    /// <summary>The index within the <see cref="archetype"/> the entity is stored</summary>
    /// <remarks>The index will change if entity is moved to another <see cref="Archetype"/></remarks>
    [Browse(Never)] internal            int                 compIndex;          //  4
    
    /// <summary>The <see cref="Archetype"/> used to store the struct components of they the entity</summary>
    [Browse(Never)] internal            Archetype           archetype;          //  8 - never null
    
    /// <summary>Container of class type components added to the entity</summary>
    [Browse(Never)] private             ClassComponent[]    classComponents;    //  8 - never null
    
    // [c# - What is the memory overhead of a .NET Object - Stack Overflow]     // 16 overhead for reference type on x64
    // https://stackoverflow.com/questions/10655829/what-is-the-memory-overhead-of-a-net-object/10655864#10655864
    
    #endregion
    
#region initialize

    private static class Static {
        internal static readonly ClassComponent[] EmptyComponents   = Array.Empty<ClassComponent>();
    }
    
    internal GameEntity(int id, Archetype archetype) {
        this.id         = id;
        this.archetype  = archetype;
        classComponents = Static.EmptyComponents;
    }
    
    #endregion

    // -------------------------------- struct component methods ---------------------------------
#region struct component methods
    /// <remarks>Executes in O(1)</remarks>
    public  Component<T> GetComponent<T>()
        where T : struct
    {
        return new Component<T>((StructHeap<T>)archetype.HeapMap[StructHeap<T>.StructIndex], this);
    }

    /// <exception cref="NullReferenceException"> if entity has no component of Type <typeparamref name="T"/></exception>
    /// <remarks>Executes in O(1)</remarks>
    public  ref T        GetComponentValue<T>()
        where T : struct
    {
        var heap = (StructHeap<T>)archetype.HeapMap[StructHeap<T>.StructIndex];
        return ref heap.chunks[compIndex / ChunkSize].components[compIndex % ChunkSize];
    }
    
    /// <remarks>Executes in O(1)</remarks>
    public bool TryGetComponentValue<T>(out T result)
        where T : struct
    {
        var heap = archetype.FindComponentHeap<T>();
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
        where T : struct
    {
        var store = archetype.store;
        return store.AddComponent<T>(id, ref archetype, ref compIndex, default, store.gameEntityUpdater);
    }
    
    /// <returns>true if component is newly added to the entity</returns>
    /// <remarks>Executes in O(1)</remarks>
    public bool AddComponent<T>(in T component)
        where T : struct
    {
        var store = archetype.store;
        return store.AddComponent(id, ref archetype, ref compIndex, in component, store.gameEntityUpdater);
    }
    
    /// <returns>true if entity contained a component of the given type before</returns>
    /// <remarks>Executes in O(1)</remarks>
    public bool RemoveComponent<T>()
        where T : struct
    {
        var store = archetype.store;
        return store.RemoveComponent<T>(id, ref archetype, ref compIndex, store.gameEntityUpdater);
    }
    
    #endregion
    
    // --------------------------------- class component methods ---------------------------------
#region class component methods
    /// <returns>the entity component of Type <typeparamref name="T"/>. Otherwise null</returns>
    public T    GetClassComponent<T>()
        where T : ClassComponent
    {
        foreach (var component in classComponents) {
            if (component is T classComponent) {
                return classComponent;
            }
        }
        return null;
    }
    
    /// <returns>true if the entity has component of Type <typeparamref name="T"/>. Otherwise false</returns>
    public bool TryGetClassComponent<T>(out T result)
        where T : ClassComponent
    {
        foreach (var component in classComponents) {
            if (component is T classComponent) {
                result = classComponent;
                return true;
            }
        }
        result = null;
        return false;
    }

    /// <returns>the component previously added to the entity.</returns>
    public T AddClassComponent<T>(T component)
        where T : ClassComponent
    {
        if (ClassType<T>.ClassIndex == ClassUtils.MissingAttribute) {
            var msg = $"Missing attribute [ClassComponent(\"<key>\")] on type: {typeof(T).Namespace}.{typeof(T).Name}";
            throw new InvalidOperationException(msg);
        }
        if (component.entity != null) {
            throw new InvalidOperationException("component already added to an entity");
        }
        component.entity    = this;
        var classes         = classComponents;
        var len             = classes.Length;
        for (int n = 0; n < len; n++)
        {
            var current = classes[n]; 
            if (current is T classComponent) {
                classes[n] = component;
                classComponent.entity = null;
                return classComponent;
            }
        }
        // --- case: map does not contain a component Type
        Utils.Resize(ref classComponents, len + 1);
        classComponents[len] = component;
        return null;
    }
    
    /// <returns>the component previously added to the entity.</returns>
    public T RemoveClassComponent<T>()
        where T : ClassComponent
    {
        _           = ClassType<T>.ClassIndex; // register class component type
        var classes = classComponents;
        var len     = classes.Length;
        for (int n = 0; n < len; n++)
        {
            if (classes[n] is T classComponent)
            {
                classComponent.entity = null;
                if (len == 0) {
                    classComponents = Static.EmptyComponents;
                    return classComponent;
                }
                classComponents = new ClassComponent[len - 1];
                for (int i = 0; i < n; i++) {
                    classComponents[i]     = classes[i];
                }
                for (int i = n + 1; i < len; i++) {
                    classComponents[i - 1] = classes[i];
                }
                return classComponent;
            }
        }
        return null;
    }
    #endregion
    
    // -------------------------------------- tree methods ---------------------------------------
#region tree node methods
    /// <remarks>Executes in O(1)</remarks>
    public GameEntity GetChild(int index) {
        var store   = archetype.store;
        return store.nodes[store.nodes[id].childIds[index]].entity;
    }
    
    /// <remarks>
    /// Executes in O(1).<br/>If its <see cref="TreeMembership"/> changes O(number of nodes in sub tree).<br/>
    /// The subtree structure of the added entity remains unchanged<br/>
    /// </remarks>
    public void AddChild(GameEntity entity) {
        var store = archetype.store;
        if (store != entity.archetype.store) throw EntityStore.InvalidStoreException(nameof(entity));
        store.AddChild(id, entity.id);
    }
    
    /// <remarks>
    /// Executes in O(1).<br/>If its <see cref="TreeMembership"/> changes (in-tree / floating) O(number of nodes in sub tree).<br/>
    /// The subtree structure of the removed entity remains unchanged<br/>
    /// </remarks>
    public void RemoveChild(GameEntity entity) {
        var store = archetype.store;
        if (store != entity.archetype.store) throw EntityStore.InvalidStoreException(nameof(entity));
        store.RemoveChild(id, entity.id);
    }
    
    /// <summary>
    /// Remove the entity from its <see cref="EntityStore"/>.<br/>
    /// The deleted instance is in <see cref="detached"/> state.
    /// Calling <see cref="GameEntity"/> methods result in <see cref="NullReferenceException"/>'s
    /// </summary>
    public void DeleteEntity()
    {
        archetype.store.DeleteNode(id);
        archetype    = null;
    }
    
    #endregion
    
#region private methods
    private object[] GetComponentsDebug()
    {
        var objects = new object[ComponentCount];
        // --- add struct components
        var count       = archetype.componentCount;
        var heaps       = archetype.Heaps;
        for (int n = 0; n < count; n++) {
            objects[n] = heaps[n].GetComponentDebug(compIndex); 
        }
        // --- add class components
        foreach (var component in classComponents) {
            objects[count++] = component;
        }
        return objects;
    }
    
    internal string GetString(StringBuilder sb)
    {
        sb.Append("id: ");
        sb.Append(id);
        if (archetype == null) {
            sb.Append("  (detached)");
            return sb.ToString();
        }
        if (HasName) {
            var name = Name.Value;
            if (name != null) {
                sb.Append("  \"");
                sb.Append(name);
                sb.Append('\"');
                return sb.ToString();
            }
        }
        if (ComponentCount == 0) {
            sb.Append("  []");
        } else {
            sb.Append("  [");
            foreach (var refComp in classComponents) {
                sb.Append('*');
                sb.Append(refComp.GetType().Name);
                sb.Append(", ");
            }
            if (archetype != null) {
                foreach (var heap in archetype.Heaps) {
                    sb.Append(heap.type.Name);
                    sb.Append(", ");
                }
            }
            sb.Length -= 2;
            sb.Append(']');
        }
        return sb.ToString();
    }
    #endregion
}
