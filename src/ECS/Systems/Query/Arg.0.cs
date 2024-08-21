// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems;

/// <summary>
/// A query system returning entities with the specified component type via its <see cref="Query"/> property.
/// </summary>
public abstract class QuerySystem : QuerySystemBase
{
    /// <summary> Return all entities matching the <see cref="Query"/>. </summary>
    protected       ArchetypeQuery      Query       => query;
    
    public override string              ToString()  => $"{Name} - []";

    #region fields
    [Browse(Never)] private     ArchetypeQuery    query;
    #endregion
    
    protected QuerySystem() : base (default) { }
    
    internal override void SetQuery(ArchetypeQuery query) { this.query = query; }
    
    internal override ArchetypeQuery  CreateQuery(EntityStore store) {
        return store.Query(Filter);
    }
}
