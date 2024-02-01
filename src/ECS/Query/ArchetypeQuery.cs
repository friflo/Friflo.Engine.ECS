// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// <see cref="ArchetypeQuery"/> and all its generic implementations are designed to be reused.
/// </summary>
public class ArchetypeQuery
{
#region public properties
    /// <summary>
    /// Return the number of entities matching the query.
    /// </summary>
    /// <remarks>
    /// Execution time O(matching <see cref="Archetypes"/>).<br/>
    /// Typically there are only a few matching <see cref="Archetypes"/>.
    /// </remarks>
    public              int                 EntityCount => Archetype.GetEntityCount(GetArchetypesSpan());
    
    /// <summary> Return the number of <c>Chunks</c> returned by the query. </summary>
    public              int                 ChunkCount  => Archetype.GetChunkCount (GetArchetypesSpan());
    
    /// <summary> Returns the set of <see cref="Archetype"/>'s matching the query.</summary>
    public ReadOnlySpan<Archetype>          Archetypes  => GetArchetypesSpan();

    /// <summary> The <see cref="EntityStore"/> on which the query operates. </summary>
    public              EntityStore         Store       => store as EntityStore;
    
    /// <summary>
    /// Return the <see cref="ArchetypeQuery"/> entities mainly for debugging.<br/>
    /// For efficient access to entity <see cref="IComponent"/>'s use one of the generic <c>EntityStore.Query()</c> methods. 
    /// </summary>
    public              QueryEntities       Entities    => new (this);
    
    /// <summary> An <see cref="ECS.EventFilter"/> used to filter the query result for added/removed components/tags. </summary>
    public              EventFilter         EventFilter => GetEventFilter();

    public override     string              ToString()  => GetString();
    #endregion

#region private / internal fields
    // --- non blittable types
    [Browse(Never)] private  readonly   EntityStoreBase     store;                  //   8
    [Browse(Never)] private             Archetype[]         archetypes;             //   8  current list of matching archetypes, can grow
    [Browse(Never)] private             EventFilter         eventFilter;            //   8  used to filter component/tag add/remove events
    
    // --- blittable types
    [Browse(Never)] private             int                 archetypeCount;         //   4  current number archetypes 
    [Browse(Never)] private             int                 lastArchetypeCount;     //   4  number of archetypes the EntityStore had on last check
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;       //  24  ordered struct indices of component types: T1,T2,T3,T4,T5
    [Browse(Never)] private  readonly   ComponentTypes      requiredComponents;     //  32
                    private             Filter              filter;                 // 304
    
    private partial struct Filter
    {
                        internal        Tags                allTags;                //  32  entity must have all tags
                        internal        Tags                anyTags;                //  32  entity must have any tag
                        internal        Tags                withoutAllTags;         //  32  entity must not have all tags
                        internal        Tags                withoutAnyTags;         //  32  entity must not have any tag
                        
                        internal        ComponentTypes      allComponents;          //  32  entity must have all component types
                        internal        ComponentTypes      anyComponents;          //  32  entity must have any component types
                        internal        ComponentTypes      withoutAllComponents;   //  32  entity must not have all component types
                        internal        ComponentTypes      withoutAnyComponents;   //  32  entity must not have any component types
   
        [Browse(Never)] internal        int                 withoutAllTagsCount;        //   8
        [Browse(Never)] internal        int                 anyTagsCount;               //   8
        [Browse(Never)] internal        int                 allTagsCount;               //   8
        
        [Browse(Never)] internal        int                 withoutAllComponentsCount;  //   8
        [Browse(Never)] internal        int                 anyComponentsCount;         //   8
        [Browse(Never)] internal        int                 allComponentsCount;         //   8
    }
    #endregion

#region tags
    /// <summary> A query result will contain only entities having all passed <paramref name="tags"/>. </summary>
    /// <param name="tags"> Use <c>Tags.Get&lt;>()</c> to set the parameter. </param>
    public ArchetypeQuery   AllTags         (in Tags tags) { SetHasAllTags(tags); return this; }
    
    /// <summary> A query result will contain only entities having any of the the passed <paramref name="tags"/>. </summary>
    /// <param name="tags"> Use <c>Tags.Get&lt;>()</c> to set the parameter. </param>
    public ArchetypeQuery   AnyTags         (in Tags tags) { SetHasAnyTags(tags); return this; }
    
    /// <summary> Entities having all passed <paramref name="tags"/> are excluded from query result. </summary>
    /// <param name="tags"> Use <c>Tags.Get&lt;>()</c> to set the parameter. </param>
    public ArchetypeQuery   WithoutAllTags  (in Tags tags) { SetWithoutAllTags(tags); return this; }
    
    /// <summary> Entities having any of the passed <paramref name="tags"/> are excluded from query result. </summary>
    /// <param name="tags"> Use <c>Tags.Get&lt;>()</c> to set the parameter. </param>
    public ArchetypeQuery   WithoutAnyTags  (in Tags tags) { SetWithoutAnyTags(tags); return this; }
    
    internal void SetHasAllTags(in Tags tags) {
        filter.allTags          = tags;
        filter.allTagsCount     = tags.Count;
        Reset();
    }
    
    internal void SetHasAnyTags(in Tags tags) {
        filter.anyTags          = tags;
        filter.anyTagsCount     = tags.Count;
        Reset();
    }
    
    internal void SetWithoutAllTags(in Tags tags) {
        filter.withoutAllTags       = tags;
        filter.withoutAllTagsCount  = tags.Count;
        Reset();
    }
    
    internal void SetWithoutAnyTags(in Tags tags) {
        filter.withoutAnyTags       = tags;
        Reset();
    }
    #endregion
    
#region components
    /// <summary> A query result will contain only entities having all passed <paramref name="componentTypes"/>. </summary>
    /// <param name="componentTypes"> Use <c>ComponentTypes.Get&lt;>()</c> to set the parameter. </param>
    public ArchetypeQuery   AllComponents         (in ComponentTypes componentTypes) { SetHasAllComponents(componentTypes); return this; }
    
    /// <summary> A query result will contain only entities having any of the the passed <paramref name="componentTypes"/>. </summary>
    /// <param name="componentTypes"> Use <c>ComponentTypes.Get&lt;>()</c> to set the parameter. </param>
    public ArchetypeQuery   AnyComponents         (in ComponentTypes componentTypes) { SetHasAnyComponents(componentTypes); return this; }
    
    /// <summary> Entities having all passed <paramref name="componentTypes"/> are excluded from query result. </summary>
    /// <param name="componentTypes"> Use <c>ComponentTypes.Get&lt;>()</c> to set the parameter. </param>
    public ArchetypeQuery   WithoutAllComponents  (in ComponentTypes componentTypes) { SetWithoutAllComponents(componentTypes); return this; }
    
    /// <summary> Entities having any of the passed <paramref name="componentTypes"/> are excluded from query result. </summary>
    /// <param name="componentTypes"> Use <c>ComponentTypes.Get&lt;>()</c> to set the parameter. </param>
    public ArchetypeQuery   WithoutAnyComponents  (in ComponentTypes componentTypes) { SetWithoutAnyComponents(componentTypes); return this; }
    
    internal void SetHasAllComponents(in ComponentTypes types) {
        filter.allComponents        = types;
        filter.allComponentsCount   = types.Count;
        Reset();
    }
    
    internal void SetHasAnyComponents(in ComponentTypes types) {
        filter.anyComponents        = types;
        filter.anyComponentsCount   = types.Count;
        Reset();
    }
    
    internal void SetWithoutAllComponents(in ComponentTypes types) {
        filter.withoutAllComponents         = types;
        filter.withoutAllComponentsCount    = types.Count;
        Reset();
    }
    
    internal void SetWithoutAnyComponents(in ComponentTypes types) {
        filter.withoutAnyComponents         = types;
        Reset();
    }
    #endregion
    
#region general
    /// <summary>
    /// Returns true if a component or tag was added / removed to / from the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    /// <remarks>
    /// Therefore <see cref="EntityStore.EventRecorder"/> needs to be enabled and<br/> 
    /// the component / tag (add / remove) events of interest need to be added to the <see cref="EventFilter"/>.<br/>
    /// <br/>
    /// <b>Note</b>: <see cref="HasEvent"/> can be called from any thread.<br/>
    /// No structural changes like adding / removing components/tags must not be executed at the same time by another thread.
    /// </remarks>
    public bool HasEvent(int entityId)
    {
        if (eventFilter != null) {
            return eventFilter.HasEvent(entityId);
        }
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
    private void Reset () {
        archetypes          = Array.Empty<Archetype>();
        lastArchetypeCount  = 1;
        archetypeCount      = 0;
    }
    
    private ReadOnlySpan<Archetype> GetArchetypesSpan() {
        var archs = GetArchetypes();
        return new ReadOnlySpan<Archetype>(archs.array, 0, archs.length);
    }
    
    private partial struct Filter
    {
        internal bool IsTagsMatch(in Tags tags)
        {
            if (anyTagsCount > 0)
            {
                if (!tags.HasAny(anyTags))
                {
                    if (allTagsCount == 0) {
                        return false;
                    }
                    if (!tags.HasAll(allTags)) {
                        return false;
                    }
                }
            } else {
                if (!tags.HasAll(allTags)) {
                    return false;
                }
            }
            if (tags.HasAny(withoutAnyTags)) {
                return false;
            }
            if (withoutAllTagsCount > 0 && tags.HasAll(withoutAllTags)) {
                return false;
            }
            return true;
        }
        
        internal bool IsComponentsMatch(in ComponentTypes componentTypes)
        {
            if (anyComponentsCount > 0)
            {
                if (!componentTypes.HasAny(anyComponents))
                {
                    if (allComponentsCount == 0) {
                        return false;
                    }
                    if (!componentTypes.HasAll(allComponents)) {
                        return false;
                    }
                }
            } else {
                if (!componentTypes.HasAll(allComponents)) {
                    return false;
                }
            }
            if (componentTypes.HasAny(withoutAnyComponents)) {
                return false;
            }
            if (withoutAllComponentsCount > 0 && componentTypes.HasAll(withoutAllComponents)) {
                return false;
            }
            return true;
        }
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
        var lastCount           = lastArchetypeCount;
        var nextCount           = archetypeCount;
        
        for (int n = lastCount; n < newStoreLength; n++)
        {
            var archetype = storeArchetypes[n];
            if (!archetype.componentTypes.HasAll(requiredComponents)) {
                continue;
            }
            if (!filter.IsTagsMatch(archetype.tags)) {
                continue;
            }
            if (!filter.IsComponentsMatch(archetype.componentTypes)) {
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
        foreach (var tag in filter.allTags) {
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
