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
namespace Friflo.Engine.ECS.Systems
{
    public abstract class QuerySystem : BaseSystem
    {
    #region properties
        [Browse(Never)] protected       QueryFilter             Filter          => filter;
        [Browse(Never)] public          int                     EntityCount     => GetEntityCount();
        [Browse(Never)] public          ComponentTypes          ComponentTypes  => componentTypes;
                        public          Array<ArchetypeQuery>   Queries         => queries;
                        public          CommandBuffer           CommandBuffer   => commandBuffer;
                        internal        View                    System          => view ??= new View(this);              
        #endregion
        
    #region fields
        [Browse(Never)] private  readonly   QueryFilter             filter  = new ();
        [Browse(Never)] private  readonly   ComponentTypes          componentTypes;
        [Browse(Never)] private             Array<ArchetypeQuery>   queries;
        [Browse(Never)] private             CommandBuffer           commandBuffer;
        [Browse(Never)] private             View                    view;
        #endregion
        
    #region constructor
        internal QuerySystem(in ComponentTypes componentTypes) {
            this.componentTypes = componentTypes;
            queries             = new Array<ArchetypeQuery>(Array.Empty<ArchetypeQuery>());
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
        
    #region update
        /// <summary> Called for every query in <see cref="Queries"/>. </summary>
        protected abstract void OnUpdate();
        
        public    override void Update()
        {
            if (!Enabled) return;
            var commandBuffers = parentGroup.commandBuffers;
            for (int n = 0; n < queries.Count; n++)
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
        
        internal string GetString() => $"{Name} - {ComponentTypes}";
        #endregion
    }
}