// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// An <see cref="EntityData"/> is used to optimize read / write access of entity components.br/>
/// An instance can be returned by <see cref="Entity.Data"/>. 
/// </summary>
/// <remarks>
/// It should be used if reading or updating multiple components of the same entity to optimize component access.
/// </remarks>
public readonly ref struct EntityData
{
#region entity getter
    /// <summary> Returns true is the entity is deleted. </summary>
    /// <remarks>Executes in O(1)</remarks>
    public  bool        IsNull          => archetype == null;
    
    /// <summary> Return the <see cref="ECS.Tags"/> added to an entity. </summary>
    /// <exception cref="NullReferenceException"> if the entity is deleted.</exception>
    /// <remarks>Executes in O(1)</remarks>
    public  Tags        Tags            => archetype.tags;
    
    public  bool        Has<T> ()  where T : struct, IComponent {
        return archetype.heapMap[StructInfo<T>.Index] != null;
    }
    
    /// <summary>Return the component of the given type as a reference.</summary>
    /// <exception cref="NullReferenceException"> if the entity is deleted or has no component of Type <typeparamref name="T"/></exception>
    /// <remarks>Executes in O(1)</remarks>
    public  ref T       Get<T>() where T : struct, IComponent {
        return ref ((StructHeap<T>)archetype.heapMap[StructInfo<T>.Index]).components[compIndex];
    }
    
    public  bool        TryGet<T>(out T value) where T : struct, IComponent {
        var type = archetype.heapMap[StructInfo<T>.Index];
        if (type != null) {
            value =  ((StructHeap<T>)type).components[compIndex];
            return true;
        }
        value = default;
        return false;
    }
    #endregion
    
#region fields
    private readonly    Archetype   archetype;
    private readonly    int         compIndex;
#endregion

    internal EntityData(Archetype archetype, int compIndex) {
        this.archetype  = archetype;
        this.compIndex  = compIndex;
    }
}