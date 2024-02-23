// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


public sealed class ArchetypeQuery<T1, T2, T3> : ArchetypeQuery
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
{
    [Browse(Never)] internal    T1[]    copyT1;
    [Browse(Never)] internal    T2[]    copyT2;
    [Browse(Never)] internal    T3[]    copyT3;
    
    public new ArchetypeQuery<T1, T2, T3> AllTags       (in Tags tags) { SetHasAllTags(tags);       return this; }
    public new ArchetypeQuery<T1, T2, T3> AnyTags       (in Tags tags) { SetHasAnyTags(tags);       return this; }
    public new ArchetypeQuery<T1, T2, T3> WithDisabled  ()             { SetWithDisabled();         return this; }
    public new ArchetypeQuery<T1, T2, T3> WithoutAllTags(in Tags tags) { SetWithoutAllTags(tags);   return this; }
    public new ArchetypeQuery<T1, T2, T3> WithoutAnyTags(in Tags tags) { SetWithoutAnyTags(tags);   return this; }
    
    public new ArchetypeQuery<T1, T2, T3> AllComponents       (in ComponentTypes componentTypes) { SetHasAllComponents(componentTypes);       return this; }
    public new ArchetypeQuery<T1, T2, T3> AnyComponents       (in ComponentTypes componentTypes) { SetHasAnyComponents(componentTypes);       return this; }
    public new ArchetypeQuery<T1, T2, T3> WithoutAllComponents(in ComponentTypes componentTypes) { SetWithoutAllComponents(componentTypes);   return this; }
    public new ArchetypeQuery<T1, T2, T3> WithoutAnyComponents(in ComponentTypes componentTypes) { SetWithoutAnyComponents(componentTypes);   return this; }
    
    internal ArchetypeQuery(EntityStoreBase store, in Signature<T1, T2, T3> signature)
        : base(store, signature.signatureIndexes) {
    }
    
    public ArchetypeQuery<T1, T2, T3> ReadOnly<T>()
        where T : struct, IComponent
    {
        if (typeof(T1) == typeof(T)) { copyT1 = new T1[ChunkSize]; return this; }
        if (typeof(T2) == typeof(T)) { copyT2 = new T2[ChunkSize]; return this; }
        if (typeof(T3) == typeof(T)) { copyT3 = new T3[ChunkSize]; return this; }
        throw ReadOnlyException(typeof(T));
    }
    
    /// <summary> Return the <see cref="Chunk{T}"/>'s storing the components and entities of an <see cref="ArchetypeQuery{T1,T2,T3}"/>. </summary>
    public      QueryChunks    <T1, T2, T3>  Chunks         => new (this);
    
    /// <summary>
    /// Returns a <see cref="QueryJob"/> that enables <see cref="JobExecution.Parallel"/> query execution.  
    /// </summary>
    public QueryJob<T1, T2, T3> ForEach(Action<Chunk<T1>, Chunk<T2>, Chunk<T3>, ChunkEntities> action)  => new (this, action);
}