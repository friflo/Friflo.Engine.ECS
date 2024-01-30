// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// <see cref="ArchetypeQuery"/> an all its generic implementation are immutable and designed to reuse its instances.
/// </summary>
public class ArchetypeQuery
{
#region public properties
    /// <remarks>
    /// Execution time O(matching <see cref="Archetypes"/>).<br/>
    /// Typically there are only a few matching <see cref="Archetypes"/>.
    /// </remarks>
                    public              int                 EntityCount => Archetype.GetEntityCount(GetArchetypesSpan());
                    public              int                 ChunkCount  => Archetype.GetChunkCount (GetArchetypesSpan());
    
    /// <returns>A set of <see cref="Archetype"/>'s matching the <see cref="ArchetypeQuery"/></returns>
                    public ReadOnlySpan<Archetype>          Archetypes  => GetArchetypesSpan();
    
                    public              EntityStore         Store       => store as EntityStore;
                    
                    /// <summary>
                    /// Return the <see cref="ArchetypeQuery"/> entities mainly for debugging.<br/>
                    /// For efficient access to entity <see cref="IComponent"/>'s use one of the generic <b><c>EntityStore.Query()</c></b> methods. 
                    /// </summary>
                    public              QueryEntities       Entities    => new (this);
                    
                    public              EventFilter         EventFilter => GetEventFilter();
    
                    public override     string              ToString()  => GetString();
    #endregion

#region private / internal fields
    // --- non blittable types
    [Browse(Never)] private  readonly   EntityStoreBase     store;              //  8
    [Browse(Never)] private             Archetype[]         archetypes;         //  8   current list of matching archetypes, can grow
    // --- blittable types
    [Browse(Never)] private             int                 archetypeCount;     //  4   current number archetypes 
                    private             int                 lastArchetypeCount; //  4   number of archetypes the EntityStore had on last check
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   // 24   ordered struct indices of component types: T1,T2,T3,T4,T5
    [Browse(Never)] private  readonly   ComponentTypes      requiredComponents; // 32
    [Browse(Never)] private             Tags                requiredTags;       // 32   entity tags an Archetype must have
    [Browse(Never)] private             EventFilter         eventFilter;        //  8   used to filter component/tag add/remove events
    #endregion

#region methods
    public ArchetypeQuery   AllTags(in Tags tags) { SetRequiredTags(tags); return this; }
    
    /// <summary>
    /// Returns true if a component or tag was added / removed to / from the entity with the passed <paramref name="entityId"/>.<br/>
    /// The component / tag of interest need to be added to the <see cref="EventFilter"/>.  
    /// </summary>
    public bool HasEvent(int entityId)
    {
        var filter = eventFilter;
        if (filter._lastEventCount != filter._recorder.allEventsCount) {
            filter.InitFilter();
        }
        if (filter.componentFilters.items != null && filter.ContainsComponentEvent(entityId)) return true;
        if (filter.tagFilters      .items != null && filter.ContainsTagEvent      (entityId)) return true;
        return false;
    }
    
    internal ArchetypeQuery(EntityStoreBase store, in SignatureIndexes indexes)
    {
        this.store          = store;
        archetypes          = Array.Empty<Archetype>();
        lastArchetypeCount  = 1;
        requiredComponents  = new ComponentTypes(indexes);
        signatureIndexes    = indexes;
    }
    
    internal ArchetypeQuery(EntityStoreBase store, in ComponentTypes componentTypes)
    {
        this.store          = store;
        archetypes          = Array.Empty<Archetype>();
        lastArchetypeCount  = 1;
        requiredComponents  = componentTypes;
    }
    
    /// <remarks>
    /// Reset <see cref="lastArchetypeCount"/> to force update of <see cref="archetypes"/> on subsequent call to <see cref="Archetypes"/>
    /// </remarks>
    internal void SetRequiredTags(in Tags tags) {
        requiredTags        = tags;
        lastArchetypeCount  = 1;
    }
    
    private ReadOnlySpan<Archetype> GetArchetypesSpan() {
        var archs = GetArchetypes();
        return new ReadOnlySpan<Archetype>(archs.array, 0, archs.length);
    }
    
    internal Archetypes GetArchetypes()
    {
        if (store.ArchetypeCount == lastArchetypeCount) {
            return new Archetypes(archetypes, archetypeCount);
        }
        // --- update archetypes / archetypesCount: Add matching archetypes newly added to the store
        var storeArchetypes     = store.Archetypes;
        var newStoreLength      = storeArchetypes.Length;
        var nextArchetypes      = archetypes;
        var nextCount           = archetypeCount;
        
        for (int n = lastArchetypeCount; n < newStoreLength; n++)
        {
            var archetype         = storeArchetypes[n];
            var hasRequiredTypes  = archetype.componentTypes.HasAll(requiredComponents) &&
                                    archetype.tags.          HasAll(requiredTags);
            if (!hasRequiredTypes) {
                continue;
            }
            if (nextCount == nextArchetypes.Length) {
                var length = Math.Max(4, 2 * nextCount);
                ArrayUtils.Resize(ref nextArchetypes, length);
            }
            nextArchetypes[nextCount++] = archetype;
        }
        // --- order matters in case of parallel execution
        archetypes          = nextArchetypes;   // using changed (added) archetypes with old archetypeCount         => OK
        archetypeCount      = nextCount;        // archetypes already changed                                       => OK
        lastArchetypeCount  = newStoreLength;   // using old lastArchetypeCount result only in a redundant update   => OK
        return new Archetypes(nextArchetypes, nextCount);
    }
    
    internal static ArgumentException ReadOnlyException(Type type) {
        return new ArgumentException($"Query does not contain Component type: {type.Name}");
    }
    
    internal string GetQueryChunksString() {
        return signatureIndexes.GetString($"QueryChunks[{ChunkCount}]  Components: ");
    }
    
    private string GetString() {
        var sb          = new StringBuilder();
        var hasTypes    = false;
        sb.Append("Query: [");
        var components  = EntityStoreBase.Static.EntitySchema.components;
        for (int n = 0; n < signatureIndexes.length; n++)
        {
            var structIndex = signatureIndexes.GetStructIndex(n);
            var structType  = components[structIndex];
            sb.Append(structType.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        foreach (var tag in requiredTags) {
            sb.Append('#');
            sb.Append(tag.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        if (hasTypes) {
            sb.Length -= 2;
            sb.Append(']');
        }
        sb.Append("  EntityCount: ");
        sb.Append(EntityCount);
        return sb.ToString();
    }
    
    private EventFilter GetEventFilter()
    {
        if (eventFilter != null) {
            return eventFilter;
        }
        return eventFilter = new EventFilter(Store.EventRecorder);
    }
    #endregion
}

public sealed class ArchetypeQuery<T1> : ArchetypeQuery
    where T1 : struct, IComponent
{
    [Browse(Never)] internal    T1[]    copyT1;
    
    public new ArchetypeQuery<T1> AllTags (in Tags tags) { SetRequiredTags(tags); return this; }
    
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
    
     public new ArchetypeQuery<T1, T2> AllTags (in Tags tags) { SetRequiredTags(tags); return this; }
    
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
    
    public new ArchetypeQuery<T1, T2, T3> AllTags (in Tags tags) { SetRequiredTags(tags); return this; }
    
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
    
    public new ArchetypeQuery<T1, T2, T3, T4> AllTags (in Tags tags) { SetRequiredTags(tags); return this; }
    
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
    
    public new ArchetypeQuery<T1, T2, T3, T4, T5> AllTags (in Tags tags) { SetRequiredTags(tags); return this; }
    
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

internal static class EnumeratorUtils
{
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage]
    internal static void AssertComponentLenGreater0 (int componentLen) {
        if (componentLen <= 0) throw new InvalidOperationException("expect componentLen > 0");
    }
}