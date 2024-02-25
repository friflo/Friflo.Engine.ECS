// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// A query instance returned by <see cref="EntityStoreBase.Query{T1,T2,T3,T4,T5}()"/> to retrieve the given component types.<br/>
/// See <a href="https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#query-entities">Example.</a>
/// </summary>
public sealed class ArchetypeQuery<T1, T2, T3, T4, T5> : ArchetypeQuery
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
{
    [Browse(Never)] internal    T1[]    copyT1;
    [Browse(Never)] internal    T2[]    copyT2;
    [Browse(Never)] internal    T3[]    copyT3;
    [Browse(Never)] internal    T4[]    copyT4;
    [Browse(Never)] internal    T5[]    copyT5;
    
    public new ArchetypeQuery<T1, T2, T3, T4, T5> AllTags       (in Tags tags) { SetHasAllTags(tags);       return this; }
    public new ArchetypeQuery<T1, T2, T3, T4, T5> AnyTags       (in Tags tags) { SetHasAnyTags(tags);       return this; }
    public new ArchetypeQuery<T1, T2, T3, T4, T5> WithDisabled  ()             { SetWithDisabled();         return this; }
    public new ArchetypeQuery<T1, T2, T3, T4, T5> WithoutAllTags(in Tags tags) { SetWithoutAllTags(tags);   return this; }
    public new ArchetypeQuery<T1, T2, T3, T4, T5> WithoutAnyTags(in Tags tags) { SetWithoutAnyTags(tags);   return this; }
    
    public new ArchetypeQuery<T1, T2, T3, T4, T5> AllComponents       (in ComponentTypes componentTypes) { SetHasAllComponents(componentTypes);       return this; }
    public new ArchetypeQuery<T1, T2, T3, T4, T5> AnyComponents       (in ComponentTypes componentTypes) { SetHasAnyComponents(componentTypes);       return this; }
    public new ArchetypeQuery<T1, T2, T3, T4, T5> WithoutAllComponents(in ComponentTypes componentTypes) { SetWithoutAllComponents(componentTypes);   return this; }
    public new ArchetypeQuery<T1, T2, T3, T4, T5> WithoutAnyComponents(in ComponentTypes componentTypes) { SetWithoutAnyComponents(componentTypes);   return this; }
    
    internal ArchetypeQuery(EntityStoreBase store, in Signature<T1, T2, T3, T4, T5> signature)
        : base(store, signature.signatureIndexes) {
    }
    
    public ArchetypeQuery<T1, T2, T3, T4, T5> ReadOnly<T>()
        where T : struct, IComponent
    {
        if (typeof(T1) == typeof(T)) { copyT1 = new T1[ChunkSize]; return this; }
        if (typeof(T2) == typeof(T)) { copyT2 = new T2[ChunkSize]; return this; }
        if (typeof(T3) == typeof(T)) { copyT3 = new T3[ChunkSize]; return this; }
        if (typeof(T4) == typeof(T)) { copyT4 = new T4[ChunkSize]; return this; }
        if (typeof(T5) == typeof(T)) { copyT5 = new T5[ChunkSize]; return this; }
        throw ReadOnlyException(typeof(T));
    }
    
    /// <summary>
    /// Return the <see cref="Chunk{T}"/>'s storing the components and entities of an <see cref="ArchetypeQuery{T1,T2,T3,T4,T5}"/>.<br/>
    /// See <a href="https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#enumerate-query-chunks">Example.</a>
    /// </summary>
    public      QueryChunks    <T1, T2, T3, T4, T5>  Chunks         => new (this);
    
    /// <summary>
    /// Returns a <see cref="QueryJob"/> that enables <see cref="JobExecution.Parallel"/> query execution.  
    /// </summary>
    public QueryJob<T1, T2, T3, T4, T5> ForEach(Action<Chunk<T1>, Chunk<T2>, Chunk<T3>, Chunk<T4>, Chunk<T5>, ChunkEntities> action)  => new (this, action);
}
