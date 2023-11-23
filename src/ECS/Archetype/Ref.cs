// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

#if COMP_ITER
using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// Enables access to a component by reference using its property <see cref="Value"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public struct Ref<T>
    where T : struct, IComponent
{
    /// <summary>
    /// Returns a mutable component value by reference.<br/>
    /// <see cref="Value"/> modifications are instantaneously available via <see cref="Entity.GetComponent{T}"/>  
    /// </summary>
    public readonly ref T       Value => ref components[pos];
    
    private             T[]     components; //  8
    internal            int     pos;        //  4
    
    internal void Set(T[] components, T[] copy, int count) {
        if (copy == null) {
            this.components = components;
            return;
        }
        Array.Copy(components, copy, count);
        this.components = copy;
    }

    public  readonly override string  ToString() => Value.ToString();
}

#endif