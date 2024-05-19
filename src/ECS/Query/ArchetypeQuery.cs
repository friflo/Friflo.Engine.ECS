// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
// ReSharper disable UseCollectionExpression
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// <see cref="ArchetypeQuery"/> and all its generic implementations are designed to be reused.<br/>
/// By default, a query does not contain <see cref="Disabled"/> entities. Use <see cref="WithDisabled"/> if needed.<br/>
/// See <a href="https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#query-entities">Example.</a>
/// </summary>
public class ArchetypeQuery
{
#region public properties
    /// <summary>
    /// Return the number of entities matching the query.
    /// </summary>
    /// <remarks>
    /// Execution time O(matching <see cref="Archetypes"/>).<br/>
    /// Typically, there are only a few matching <see cref="Archetypes"/>.
    /// </remarks>
    public              int             Count           => Archetype.GetEntityCount(GetArchetypesSpan());
    
    /// <summary> Obsolete. Renamed to <see cref="Count"/>. </summary>
    [Obsolete($"Renamed to {nameof(Count)}")]
    [Browse(Never)]
    public              int             EntityCount     => Archetype.GetEntityCount(GetArchetypesSpan());
    
    /// <summary> Return the number of <c>Chunks</c> returned by the query. </summary>
    public              int             ChunkCount      => Archetype.GetChunkCount (GetArchetypesSpan());
    
    /// <summary> Returns the set of <see cref="Archetype"/>'s matching the query.</summary>
    public ReadOnlySpan<Archetype>      Archetypes      => GetArchetypesSpan();

    /// <summary> The <see cref="EntityStore"/> on which the query operates. </summary>
    public              EntityStore     Store           => store as EntityStore;
    
    /// <summary>
    /// Return the <see cref="ArchetypeQuery"/> entities mainly for debugging.<br/>
    /// For efficient access to entity <see cref="IComponent"/>'s use one of the generic <c>EntityStore.Query()</c> methods. 
    /// </summary>
    public              QueryEntities   Entities        => new (this);
    
    /// <summary>
    /// A <see cref="ECS.EventFilter"/> used to filter the query result for added/removed components/tags.<br/>
    /// See <a href="https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#eventfilter">Example.</a>
    /// </summary>
    public              EventFilter     EventFilter     => GetEventFilter();
    
    /// <summary> Return the <see cref="ComponentTypes"/> of components returned by a query result. </summary>
    [Browse(Never)]
    public ref readonly ComponentTypes  ComponentTypes  => ref components;
    
    
    public override     string          ToString()      => GetString();
    #endregion
    
#region public fields
    /// <summary> Return component and tag filters added to the query </summary>
                    public   readonly   QueryFilter         Filter;             //   8
    #endregion

#region private / internal fields
    // --- non blittable types
    [Browse(Never)] private  readonly   EntityStoreBase     store;              //   8
    [Browse(Never)] private             Archetype[]         archetypes;         //   8  current list of matching archetypes, can grow
    [Browse(Never)] private             EventFilter         eventFilter;        //   8  used to filter component/tag add/remove events
    [Browse(Never)] private             EntityList          entityList;         //   8  provide entities as list to perform structural changes

    
    // --- blittable types
    [Browse(Never)] private             int                 filterVersion;      //   4
    [Browse(Never)] private             int                 archetypeCount;     //   4  current number archetypes 
    [Browse(Never)] private             int                 lastArchetypeCount; //   4  number of archetypes the EntityStore had on last check
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   //  16  ordered struct indices of component types: T1,T2,T3,T4,T5
    [Browse(Never)] private  readonly   ComponentTypes      components;         //  32
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
    
    internal void SetHasAllTags     (in Tags tags) => Filter.AllTags(tags);
    internal void SetHasAnyTags     (in Tags tags) => Filter.AnyTags(tags);
    internal void SetWithoutAllTags (in Tags tags) => Filter.WithoutAllTags(tags);
    internal void SetWithoutAnyTags (in Tags tags) => Filter.WithoutAnyTags(tags);
    internal void SetWithDisabled()                => Filter.WithDisabled();
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
    
    internal void SetHasAllComponents       (in ComponentTypes types) => Filter.AllComponents(types);
    internal void SetHasAnyComponents       (in ComponentTypes types) => Filter.AnyComponents(types);
    internal void SetWithoutAllComponents   (in ComponentTypes types) => Filter.WithoutAllComponents(types);
    internal void SetWithoutAnyComponents   (in ComponentTypes types) => Filter.WithoutAnyComponents(types);
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
    /// Returns the query result as a <see cref="EntityList"/> used to perform structural changes.
    /// </summary>
    public EntityList ToEntityList()
    {
        var list = entityList ??= new EntityList(Store);
        list.Clear();
        list.entityStore = Store;
        foreach (var entity in Entities) {
            list.Add(entity.Id);   
        }
        return list;
    }
    
    /// <summary>
    /// The query <see cref="Filter"/> cannot be changed anymore.
    /// </summary>
    public ArchetypeQuery FreezeFilter() {
        Filter.FreezeFilter();
        return this;
    }
    
    internal void SetFreezeFilter() {
        Filter.FreezeFilter();
    }
    
    /// <summary>
    /// Called by generic ArchetypeQuery constructors. <br/>
    /// <see cref="Disabled"/> entities excluded by default.
    /// </summary>
    internal ArchetypeQuery(EntityStoreBase store, in SignatureIndexes indexes, QueryFilter filter)
    {
        this.store      = store;
        archetypes      = Array.Empty<Archetype>();
        components      = new ComponentTypes(indexes);
        signatureIndexes= indexes;
        Filter          = filter ?? new QueryFilter();
    }
    
    /// <summary>
    /// Called by <see cref="EntityStoreBase.Query()"/>. <br/>
    /// <see cref="Disabled"/> entities excluded by default.
    /// </summary>
    internal ArchetypeQuery(EntityStoreBase store, in ComponentTypes componentTypes, QueryFilter filter)
    {
        this.store      = store;
        archetypes      = Array.Empty<Archetype>();
        components      = componentTypes;
        Filter          = filter ?? new QueryFilter();
    }
    
    /// <summary> Called by <see cref="EntityStore.GetEntities"/> </summary>
    internal ArchetypeQuery(EntityStoreBase store)
    {
        this.store      = store;
        archetypes      = Array.Empty<Archetype>();
        Filter          = new QueryFilter(default).FreezeFilter();
    }
    
    /// <summary> Called by <see cref="Archetype.GetEntities"/> </summary>
    internal ArchetypeQuery(Archetype archetype)
    {
        singleArchetype = true;
        store           = archetype.store;
        archetypes      = new [] { archetype };
        components      = archetype.componentTypes;
        Filter          = new QueryFilter(archetype.tags).FreezeFilter();
    }
    
    private ReadOnlySpan<Archetype> GetArchetypesSpan() {
        var archs = GetArchetypes();
        return new ReadOnlySpan<Archetype>(archs.array, 0, archs.length);
    }
    
    internal Archetypes GetArchetypes()
    {
        var localFilter = Filter;
        if (filterVersion      != localFilter.version) {
            filterVersion       = localFilter.version;
            archetypes          = Array.Empty<Archetype>();
            archetypeCount      = 0;
            lastArchetypeCount  = 0;
        }
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
            // Filter conditions same as in IsMatch()
            if (!archetype.componentTypes.HasAll(components)) {
                continue;
            }
            if (!localFilter.IsTagsMatch(archetype.tags)) {
                continue;
            }
            if (!localFilter.IsComponentsMatch(archetype.componentTypes)) {
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
    
    /// <summary>
    /// Returns true if the passed <paramref name="componentTypes"/> and <paramref name="tags"/> matches the query filter.
    /// </summary>
    public bool IsMatch(in ComponentTypes componentTypes, in Tags tags)
    {
        if (!componentTypes.HasAll(components)) {
            return false;
        }
        if (!Filter.IsTagsMatch(tags)) {
            return false;
        }
        if (!Filter.IsComponentsMatch(componentTypes)) {
            return false;
        }
        return true;
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
        foreach (var tag in Filter.Condition.AllTags) {
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
