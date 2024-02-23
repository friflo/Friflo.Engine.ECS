// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable UseCollectionExpression
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
    public              int                 Count => Archetype.GetEntityCount(GetArchetypesSpan());
    
    [Obsolete($"Renamed to {nameof(Count)}")]
    [Browse(Never)]
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
    [Browse(Never)] private  readonly   EntityStoreBase     store;              //   8
    [Browse(Never)] private             Archetype[]         archetypes;         //   8  current list of matching archetypes, can grow
    [Browse(Never)] private             EventFilter         eventFilter;        //   8  used to filter component/tag add/remove events
    
    // --- blittable types
    [Browse(Never)] private             int                 archetypeCount;     //   4  current number archetypes 
    [Browse(Never)] private             int                 lastArchetypeCount; //   4  number of archetypes the EntityStore had on last check
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   //  24  ordered struct indices of component types: T1,T2,T3,T4,T5
    [Browse(Never)] private  readonly   ComponentTypes      components;         //  32
                    private             QueryFilter         filter;             // 304
    [Browse(Never)] private  readonly   bool                singleArchetype;    //   1  if true it returns only the entities a specific archetype
    #endregion

#region tags
    /// <summary> A query result will contain only entities having all passed <paramref name="tags"/>. </summary>
    /// <param name="tags"> Use <c>Tags.Get&lt;>()</c> to set the parameter. </param>
    public ArchetypeQuery   AllTags         (in Tags tags) { SetHasAllTags(tags); return this; }
    
    /// <summary> A query result will contain only entities having any of the the passed <paramref name="tags"/>. </summary>
    /// <param name="tags"> Use <c>Tags.Get&lt;>()</c> to set the parameter. </param>
    public ArchetypeQuery   AnyTags         (in Tags tags) { SetHasAnyTags(tags); return this; }
    
    /// <summary> A query result will contain <see cref="Disabled"/> entities. </summary>
    public ArchetypeQuery   WithDisabled    ()             { SetWithDisabled(); return this; }

    
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
        if (filter.withoutDisabled) {
            filter.withoutAnyTags.Add(EntityUtils.Disabled);
        }
        Reset();
    }
    
    internal void SetWithDisabled() {
        filter.withoutDisabled = false;
        filter.withoutAnyTags.Remove(EntityUtils.Disabled);
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
    
    /// <summary>
    /// Called by generic ArchetypeQuery's. <br/>
    /// <see cref="Disabled"/> entities excluded by default.
    /// </summary>
    internal ArchetypeQuery(EntityStoreBase store, in SignatureIndexes indexes)
    {
        this.store              = store;
        archetypes              = Array.Empty<Archetype>();
        components              = new ComponentTypes(indexes);
        signatureIndexes        = indexes;
        filter.withoutDisabled  = true;
        filter.withoutAnyTags   = EntityUtils.Disabled;
    }
    
    /// <summary>
    /// Called by <see cref="EntityStoreBase.Query()"/>. <br/>
    /// <see cref="Disabled"/> entities excluded by default.
    /// </summary>
    internal ArchetypeQuery(EntityStoreBase store, in ComponentTypes componentTypes)
    {
        this.store              = store;
        archetypes              = Array.Empty<Archetype>();
        components              = componentTypes;
        filter.withoutDisabled  = true;
        filter.withoutAnyTags   = EntityUtils.Disabled;
    }
    
    /// <summary> Called by <see cref="EntityStore.GetEntities"/> </summary>
    internal ArchetypeQuery(EntityStoreBase store)
    {
        this.store              = store;
        archetypes              = Array.Empty<Archetype>();
    }
    
    /// <summary> Called by <see cref="Archetype.GetEntities"/> </summary>
    internal ArchetypeQuery(Archetype archetype)
    {
        singleArchetype     = true;
        store               = archetype.store;
        archetypes          = new [] { archetype };
        components          = archetype.componentTypes;
        filter.allTags      = archetype.tags;
    }
    
    /// <remarks>
    /// Reset <see cref="lastArchetypeCount"/> to force update of <see cref="archetypes"/> on subsequent call to <see cref="Archetypes"/>
    /// </remarks>
    private void Reset () {
        archetypes          = Array.Empty<Archetype>();
        lastArchetypeCount  = 0;
        archetypeCount      = 0;
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
        if (singleArchetype) {
            return new Archetypes(archetypes, 1);
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
            if (!archetype.componentTypes.HasAll(components)) {
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
    
    internal string GetQueryJobString() {
        return signatureIndexes.GetString($"QueryJob ");
    }
    
    private string GetString() {
        var sb          = new StringBuilder();
        var hasTypes    = false;
        if (singleArchetype) {
            sb.Append("Archetype: [");
        } else {
            sb.Append("Query: [");
        }
        foreach (var component in components) {
            sb.Append(component.Name);
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
        }
        sb.Append(']');
        sb.Append("  Count: ");
        sb.Append(Count);
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
