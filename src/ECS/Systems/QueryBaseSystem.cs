// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable UseCollectionExpression
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems;

/// <summary>
/// A query system returning the components specified in a subclass extending <c>QuerySystem&lt;T1, ... , Tn></c>.
/// </summary>
public abstract class QuerySystemBase : BaseSystem
{
#region properties
    /// <summary>
    /// A query filter used to restrict the entities returned by its <c>Query</c> property.<br/>
    /// See remarks to add a tag filter to a custom <c>QuerySystem</c>.
    /// </summary>
    /// <remarks>
    /// Additional tag filters can be added in the constructor of a class extending a <c>QuerySystem</c>.
    /// <code>
    /// class MySystem : QuerySystem&lt;Scale3>
    /// {
    ///     public MySystem() => Filter.AnyTags(Tags.Get&lt;MyTag>()); 
    ///     protected override void OnUpdate() { ... }
    /// }
    /// </code>
    /// </remarks>
    [Browse(Never)] public          QueryFilter         Filter          => filter;

    /// <summary> The number of entities matching the <c>Query</c>. </summary>
    [Browse(Never)] public          int                 EntityCount     => GetEntityCount();

    /// <summary> The component types of components returned by its <c>Query</c> property. </summary>
    [Browse(Never)] public          ComponentTypes      ComponentTypes  => componentTypes;

    /// <summary> Return all system queries. One per store in <see cref="SystemRoot.Stores"/>. </summary>
                    public ReadOnlyList<ArchetypeQuery> Queries         => queries;

    /// <summary> Return the <see cref="CommandBuffer"/> of its <see cref="BaseSystem.ParentGroup"/>.</summary>
    [Browse(Never)] protected       CommandBuffer       CommandBuffer   => commandBuffer;
    #endregion
    
#region fields
    [Browse(Never)] private  readonly   QueryFilter                 filter  = new ();
    [Browse(Never)] private  readonly   ComponentTypes              componentTypes;
    [Browse(Never)] private             ReadOnlyList<ArchetypeQuery>queries;
    [Browse(Never)] private             CommandBuffer               commandBuffer;
    #endregion
    
#region constructor
    internal QuerySystemBase(in ComponentTypes componentTypes) {
        this.componentTypes = componentTypes;
        queries             = new ReadOnlyList<ArchetypeQuery>(Array.Empty<ArchetypeQuery>());
    }
    #endregion
    
#region abstract - query
    internal    abstract ArchetypeQuery CreateQuery(EntityStore store);
    internal    abstract void           SetQuery(ArchetypeQuery query);
    #endregion
    
#region store: add / remove
    internal override void AddStoreInternal(EntityStore entityStore)
    {
        var query = CreateQuery(entityStore);
        queries.Add(query);
    }
    
    internal override void RemoveStoreInternal(EntityStore entityStore)
    {
        foreach (var query in queries) {
            if (query.Store != entityStore) {
                continue;
            }
            queries.Remove(query);
            return;
        }
    }
    #endregion
    
#region system: update
    /// <summary> Called for every query in <see cref="Queries"/>. </summary>
    protected abstract void OnUpdate();
    
    protected internal override void OnUpdateGroup()
    {
        var commandBuffers = ParentGroup.commandBuffers;
        for (int n = 0; n < queries.count; n++)
        {
            var query       = queries[n];
            commandBuffer   = commandBuffers[n];
            SetQuery(query);
            OnUpdate();
            SetQuery(null);
            commandBuffer = null;
        }
    }
    #endregion
    
#region internal methods
    private int GetEntityCount() {
        int count = 0;
        foreach (var query in queries) {
            count += query.Count;
        }
        return count;
    }
    
    internal string GetString(in SignatureIndexes signature) => $"{Name} - {signature.GetString(null)}";
    #endregion
}
