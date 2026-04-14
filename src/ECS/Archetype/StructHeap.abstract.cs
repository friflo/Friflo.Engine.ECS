// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable UseNullPropagation
// ReSharper disable StaticMemberInGenericType
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <remarks>
/// <b>Note:</b> Should not contain any other fields. Reasons:<br/>
/// - to enable maximum efficiency when GC iterate <see cref="Archetype.structHeaps"/> <see cref="Archetype.heapMap"/>
///   for collection.
/// </remarks>
internal abstract class StructHeap<T> : StructHeap, IComponentStash<T>
    where T : struct
{
    internal        T       componentStash; //  sizeof(T)

    
    internal StructHeap(int structIndex)
        : base (structIndex)
    {
    }
    
    public   abstract   ref T   GetStashRef();

    public   abstract   T[]     Components { get; }
    
    internal abstract   ref T   GetComponent(int index); 				// SOA
    
    internal abstract   void    SetComponent(int index, T component); 	// SOA
}
