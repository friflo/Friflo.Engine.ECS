// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems;

public abstract class QuerySystem<T1> : QuerySystem
    where T1 : struct, IComponent
{
    public          ArchetypeQuery<T1>  Query       => query;
    public override string              ToString()  => GetString(Signature.Get<T1>().signatureIndexes);

    #region fields
    [Browse(Never)] private     ArchetypeQuery<T1>    query;
    #endregion
    
    protected QuerySystem() : base (ComponentTypes.Get<T1>()) { }
    
    internal override void SetQuery(ArchetypeQuery query) { this.query = (ArchetypeQuery<T1>)query; }
    
    internal override ArchetypeQuery  CreateQuery(EntityStore store) {
        return store.Query<T1>(Filter);
    }
}
