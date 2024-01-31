// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


public sealed class ArchetypeQuery<T1> : ArchetypeQuery
    where T1 : struct, IComponent
{
    [Browse(Never)] internal    T1[]    copyT1;
    
    public new ArchetypeQuery<T1> AllTags       (in Tags tags) { SetHasAllTags(tags);       return this; }
    public new ArchetypeQuery<T1> AnyTags       (in Tags tags) { SetHasAnyTags(tags);       return this; }
    public new ArchetypeQuery<T1> WithoutAllTags(in Tags tags) { SetWithoutAllTags(tags);   return this; }
    public new ArchetypeQuery<T1> WithoutAnyTags(in Tags tags) { SetWithoutAnyTags(tags);   return this; }
    
    public new ArchetypeQuery<T1> AllComponents       (in ComponentTypes componentTypes) { SetHasAllComponents(componentTypes);       return this; }
    public new ArchetypeQuery<T1> AnyComponents       (in ComponentTypes componentTypes) { SetHasAnyComponents(componentTypes);       return this; }
    public new ArchetypeQuery<T1> WithoutAllComponents(in ComponentTypes componentTypes) { SetWithoutAllComponents(componentTypes);   return this; }
    public new ArchetypeQuery<T1> WithoutAnyComponents(in ComponentTypes componentTypes) { SetWithoutAnyComponents(componentTypes);   return this; }
    
    internal ArchetypeQuery(EntityStoreBase store, in Signature<T1> signature)
        : base(store, signature.signatureIndexes) {
    }
    
    public ArchetypeQuery<T1> ReadOnly<T>()
        where T : struct, IComponent
    {
        if (typeof(T1) == typeof(T)) { copyT1 = new T1[ChunkSize]; return this; }
        throw ReadOnlyException(typeof(T));
    }
    
    /// <summary> Return the <see cref="Chunk{T}"/>'s storing the components and entities of an <see cref="ArchetypeQuery{T1}"/>. </summary> 
    public      QueryChunks <T1>  Chunks                                      => new (this);
}

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
}

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
}

public sealed class ArchetypeQuery<T1, T2, T3, T4> : ArchetypeQuery
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
{
    [Browse(Never)] internal    T1[]    copyT1;
    [Browse(Never)] internal    T2[]    copyT2;
    [Browse(Never)] internal    T3[]    copyT3;
    [Browse(Never)] internal    T4[]    copyT4;
    
    public new ArchetypeQuery<T1, T2, T3, T4> AllTags       (in Tags tags) { SetHasAllTags(tags);       return this; }
    public new ArchetypeQuery<T1, T2, T3, T4> AnyTags       (in Tags tags) { SetHasAnyTags(tags);       return this; }
    public new ArchetypeQuery<T1, T2, T3, T4> WithoutAllTags(in Tags tags) { SetWithoutAllTags(tags);   return this; }
    public new ArchetypeQuery<T1, T2, T3, T4> WithoutAnyTags(in Tags tags) { SetWithoutAnyTags(tags);   return this; }
    
    public new ArchetypeQuery<T1, T2, T3, T4> AllComponents       (in ComponentTypes componentTypes) { SetHasAllComponents(componentTypes);       return this; }
    public new ArchetypeQuery<T1, T2, T3, T4> AnyComponents       (in ComponentTypes componentTypes) { SetHasAnyComponents(componentTypes);       return this; }
    public new ArchetypeQuery<T1, T2, T3, T4> WithoutAllComponents(in ComponentTypes componentTypes) { SetWithoutAllComponents(componentTypes);   return this; }
    public new ArchetypeQuery<T1, T2, T3, T4> WithoutAnyComponents(in ComponentTypes componentTypes) { SetWithoutAnyComponents(componentTypes);   return this; }
    
    internal ArchetypeQuery(EntityStoreBase store, in Signature<T1, T2, T3, T4> signature)
        : base(store, signature.signatureIndexes) {
    }
    
    public ArchetypeQuery<T1, T2, T3, T4> ReadOnly<T>()
        where T : struct, IComponent
    {
        if (typeof(T1) == typeof(T)) { copyT1 = new T1[ChunkSize]; return this; }
        if (typeof(T2) == typeof(T)) { copyT2 = new T2[ChunkSize]; return this; }
        if (typeof(T3) == typeof(T)) { copyT3 = new T3[ChunkSize]; return this; }
        if (typeof(T4) == typeof(T)) { copyT4 = new T4[ChunkSize]; return this; }
        throw ReadOnlyException(typeof(T));
    }
    
    /// <summary> Return the <see cref="Chunk{T}"/>'s storing the components and entities of an <see cref="ArchetypeQuery{T1,T2,T3,T4}"/>. </summary>
    public      QueryChunks    <T1, T2, T3, T4>  Chunks         => new (this);
}

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
    
    /// <summary> Return the <see cref="Chunk{T}"/>'s storing the components and entities of an <see cref="ArchetypeQuery{T1,T2,T3,T4,T5}"/>. </summary>
    public      QueryChunks    <T1, T2, T3, T4, T5>  Chunks         => new (this);
}
