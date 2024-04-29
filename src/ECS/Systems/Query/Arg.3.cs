// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems
{
    public abstract class QuerySystem<T1, T2, T3> : QuerySystem
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        protected       ArchetypeQuery<T1, T2, T3>  Query       => query;
        public override string                      ToString()  => GetString();
        
    #region fields
        [Browse(Never)] private     ArchetypeQuery<T1, T2, T3>    query;
        #endregion
        
        protected QuerySystem() : base (ComponentTypes.Get<T1, T2, T3>()) { }
        
        internal override void SetQuery(ArchetypeQuery query) { this.query = (ArchetypeQuery<T1, T2, T3>)query; }
        
        internal override ArchetypeQuery  CreateQuery(EntityStore store) {
            return store.Query<T1,T2,T3>(Filter);
        }
    }
}