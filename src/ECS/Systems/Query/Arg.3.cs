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
/// A query system returning entities with the specified component types via its <see cref="Query"/> property.
/// </summary>
public abstract class QuerySystem<T1, T2, T3> : QuerySystemBase
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
{
    /// <summary> Return all entities matching the <see cref="Query"/>. </summary>
    protected       ArchetypeQuery<T1, T2, T3>  Query       => query;
    
    public override string                      ToString()  => GetString(Signature.Get<T1,T2,T3>().signatureIndexes);
    
#region fields
    [Browse(Never)] private     ArchetypeQuery<T1, T2, T3>    query;
    #endregion
    
    protected QuerySystem() : base (Generic<T1, T2, T3>.ComponentTypes) { }
    
    internal override void SetQuery(ArchetypeQuery query) { this.query = (ArchetypeQuery<T1, T2, T3>)query; }
    
    internal override ArchetypeQuery  CreateQuery(EntityStore store) {
        return store.Query<T1,T2,T3>(Filter);
    }
}
