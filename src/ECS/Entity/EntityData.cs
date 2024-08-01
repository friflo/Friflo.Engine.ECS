// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// An <see cref="EntityData"/> is used to optimize access of entity components and tags.<br/>
/// An instance can be returned by <see cref="Entity.Data"/>. 
/// </summary>
/// <remarks>
/// It should be used if reading or updating multiple components of the same entity to optimize component access.
/// </remarks>
public readonly ref struct EntityData
{
#region entity properties
    /// <summary> Return the <see cref="ECS.Tags"/> added to an entity. </summary>
    /// <exception cref="NullReferenceException"> if the entity is deleted.</exception>
    /// <remarks>Executes in O(1)</remarks>
                    public  Tags                Tags            => archetype.tags;

    /// <summary> Returns true is the entity is deleted. </summary>
    /// <remarks>Executes in O(1)</remarks>
    [Browse(Never)] public  bool                IsNull          => archetype == null;

    /// <summary> Returns the archetype the entity belongs to. </summary>
    [Browse(Never)] public  Archetype           Archetype       => archetype;
    
    /// <summary>Return the <see cref="IComponent"/>'s added to the entity.</summary>
    /// <exception cref="NullReferenceException"> if the entity is deleted.</exception>
                    public  EntityComponents    Components      => new EntityComponents(new Entity(archetype.entityStore, Id));
    
     public override        string              ToString()      => $"Id: {Id}";
#endregion

#region fields
    [Browse(Never)] private readonly    StructHeap[]    heapMap;
    [Browse(Never)] private readonly    Archetype       archetype;
    
    /// <summary> <see cref="Entity"/> id </summary>
                    public  readonly    int             Id;
    [Browse(Never)] private readonly    int             compIndex;
    #endregion
    
#region entity getter
    /// <summary> Returns true if the entity contains a component of the specified type. </summary>
    /// <exception cref="NullReferenceException"> if the entity is deleted.</exception>
    public  bool        Has<T> ()  where T : struct, IComponent {
        return heapMap[StructInfo<T>.Index] != null;
    }
    
    /// <summary>Return the component of the given type as a reference.</summary>
    /// <exception cref="NullReferenceException"> if the entity is deleted or has no component of Type <typeparamref name="T"/></exception>
    /// <remarks>Executes in O(1)</remarks>
    public  ref T       Get<T>() where T : struct, IComponent {
        return ref ((StructHeap<T>)heapMap[StructInfo<T>.Index]).components[compIndex];
    }
    
    /// <summary>
    /// Gets the component of the specififed type.<br/>
    /// Returns true if the entity contains a component of specified type. Otherwise false.
    /// </summary>
    /// <exception cref="NullReferenceException"> if the entity is deleted.</exception>
    /// <remarks>Executes in O(1)</remarks>
    public  bool        TryGet<T>(out T value) where T : struct, IComponent {
        var type = heapMap[StructInfo<T>.Index];
        if (type != null) {
            value =  ((StructHeap<T>)type).components[compIndex];
            return true;
        }
        value = default;
        return false;
    }
    #endregion
    

#region methods
    internal EntityData(in EntityNode node, int id) {
        var type    = node.archetype;
        heapMap     = type.heapMap;
        archetype   = type;
        Id          = id;
        compIndex   = node.compIndex;
    }
    
    internal EntityData(int id) {
        Id = id;
    }
    #endregion
}