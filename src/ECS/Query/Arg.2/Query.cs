// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


public sealed class ArchetypeQuery<T1, T2> : ArchetypeQuery // : IEnumerable <>  // <- not implemented to avoid boxing
    where T1 : struct, IComponent
    where T2 : struct, IComponent
{
    [Browse(Never)] internal    T1[]    copyT1;
    [Browse(Never)] internal    T2[]    copyT2;
    
     public new ArchetypeQuery<T1, T2> AllTags       (in Tags tags) { SetHasAllTags(tags);      return this; }
     public new ArchetypeQuery<T1, T2> AnyTags       (in Tags tags) { SetHasAnyTags(tags);      return this; }
     public new ArchetypeQuery<T1, T2> WithoutAllTags(in Tags tags) { SetWithoutAllTags(tags);  return this; }
     public new ArchetypeQuery<T1, T2> WithoutAnyTags(in Tags tags) { SetWithoutAnyTags(tags);  return this; }
     
     public new ArchetypeQuery<T1, T2> AllComponents       (in ComponentTypes componentTypes) { SetHasAllComponents(componentTypes);       return this; }
     public new ArchetypeQuery<T1, T2> AnyComponents       (in ComponentTypes componentTypes) { SetHasAnyComponents(componentTypes);       return this; }
     public new ArchetypeQuery<T1, T2> WithoutAllComponents(in ComponentTypes componentTypes) { SetWithoutAllComponents(componentTypes);   return this; }
     public new ArchetypeQuery<T1, T2> WithoutAnyComponents(in ComponentTypes componentTypes) { SetWithoutAnyComponents(componentTypes);   return this; }
    
    internal ArchetypeQuery(EntityStoreBase store, in Signature<T1, T2> signature)
        : base(store, signature.signatureIndexes) {
    }
    
    public ArchetypeQuery<T1, T2> ReadOnly<T>()
        where T : struct, IComponent
    {
        if (typeof(T1) == typeof(T)) { copyT1 = new T1[ChunkSize]; return this; }
        if (typeof(T2) == typeof(T)) { copyT2 = new T2[ChunkSize]; return this; }
        throw ReadOnlyException(typeof(T));
    }
    
    /// <summary> Return the <see cref="Chunk{T}"/>'s storing the components and entities of an <see cref="ArchetypeQuery{T1,T2}"/>. </summary> 
    public      QueryChunks    <T1,T2>  Chunks                                      => new (this);
    
    public QueryJob<T1, T2> ForEach(Action<Chunk<T1>, Chunk<T2>, ChunkEntities> action)  => new (this, action);
}