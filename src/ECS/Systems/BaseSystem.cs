// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems
{
    public abstract class BaseSystem
    {
    #region properties
                public virtual  string          Name        => systemName;
                public          SystemRoot      SystemRoot  => systemRoot;
                public          SystemGroup     ParentGroup => parentGroup;
        [Ignore]public          bool            Enabled     { get => enabled;       set => enabled = value; }
                public          int             Id          => id;
        #endregion
    
    #region internal fields
        [Serialize] [Browse(Never)] internal            int         id;
        [Serialize] [Browse(Never)] private             bool        enabled = true;
                    [Browse(Never)] private readonly    string      systemName;
                    [Browse(Never)] internal            SystemGroup parentGroup;
                    [Browse(Never)] private             SystemRoot  systemRoot;
         #endregion
         
    #region constructors
        protected BaseSystem() {
            systemName = GetType().Name;
        }
        #endregion
        
    #region system events
        public event Action<SystemChanged>  OnSystemChanged;

        public void CastSystemUpdate(string field, object value)
        {
            var change  = new SystemChanged(SystemChangedAction.Update, this, field, value);
            var root    = systemRoot;
            if (root != parentGroup) {
                parentGroup.OnSystemChanged?.Invoke(change);
            }
            root?.OnSystemChanged?.Invoke(change);
        }
        
        internal void CastSystemChanged(SystemChangedAction action)
        {
            var change  = new SystemChanged(action, this, null, null);
            var root    = systemRoot;
            if (root != parentGroup) {
                parentGroup.OnSystemChanged?.Invoke(change);
            }
            root?.OnSystemChanged?.Invoke(change);
        }
        
        internal void CastSystemRemoved(SystemRoot oldRoot, SystemGroup oldParent)
        {
            var change  = new SystemChanged(SystemChangedAction.Remove, this, null, null);
            if (oldRoot != oldParent) {
                oldParent.OnSystemChanged?.Invoke(change);
            }
            oldRoot?.OnSystemChanged?.Invoke(change);
        }
        #endregion

    #region virtual - store: add / remove
        internal           virtual  void RemoveStoreInternal(EntityStore store) { }
        internal           virtual  void AddStoreInternal   (EntityStore store) { }
        #endregion
        
    #region virtual - system: update
        public             virtual  void Update             (Tick tick) { }
        
        /// <summary>
        /// Called once per <see cref="BaseSystem"/> before <see cref="Update"/> within a <see cref="SystemGroup"/>.
        /// </summary>
        protected internal virtual  void OnUpdateGroupBegin (Tick tick) { }
        
        /// <summary>
        /// Called once per <see cref="BaseSystem"/> after <see cref="Update"/> within a <see cref="SystemGroup"/>.
        /// </summary>
        protected internal virtual  void OnUpdateGroupEnd   (Tick tick) { }
        #endregion
        
    #region system: move
        public int MoveSystemTo(SystemGroup systemGroup, int index)
        {
            if (systemGroup == null)                                    throw new ArgumentNullException(nameof(systemGroup));
            if (parentGroup == null)                                    throw new InvalidOperationException($"Group '{Name}' has no parent");
            if (index < -1 || index > systemGroup.childSystems.Count)   throw new ArgumentException($"invalid index: {index}");
            if (parentGroup == systemGroup) {
                var oldIndex = systemGroup.childSystems.Remove(this);
                if (index == -1) {
                    systemGroup.childSystems.Add(this);
                    index = systemGroup.childSystems.Count - 1;
                } else {
                    if (index > oldIndex) index--;
                    systemGroup.childSystems.InsertAt(index, this);
                }
                // Send event. See: SEND_EVENT notes
                CastSystemChanged(SystemChangedAction.Move); 
                return index;
            }
            if (systemRoot == systemGroup.systemRoot) {
                parentGroup.childSystems.Remove(this);
                if (index == -1) {
                    systemGroup.childSystems.Add(this);
                    index = systemGroup.childSystems.Count - 1;
                } else {
                    systemGroup.childSystems.InsertAt(index, this);
                }
                parentGroup = systemGroup;
                // Send event. See: SEND_EVENT notes
                CastSystemChanged(SystemChangedAction.Move);
                return index;
            }
            throw new NotImplementedException();
        }
        #endregion
        
    #region set parent and root
        internal void SetRoot(SystemRoot root)
        {
            systemRoot = root;
        }
        
        internal void SetParentAndRoot(SystemGroup group)
        {
            parentGroup     = group;
            var newRoot     = group.systemRoot;
            var currentRoot = systemRoot;
            if (currentRoot == newRoot) {
                return;
            }
            if (newRoot == null) {
                return;
            }
            var rootSystems = GetSubSystems(ref newRoot.systemBuffer);
            // add stores of SystemRoot to all root Systems
            foreach (var system in rootSystems) {
                system.systemRoot = newRoot;
                newRoot.AddSystemToRoot(system);
                foreach (var store in newRoot.stores) {
                    system.AddStoreInternal(store);
                }
            }
        }
        
        internal void ClearParentAndRoot()
        { 
            parentGroup         = null;
            var currentRoot     = systemRoot;
            if (currentRoot == null) {
                return;
            }
            var rootSystems = GetSubSystems(ref currentRoot.systemBuffer);
            // remove stores of SystemRoot from all root Systems
            foreach (var system in rootSystems) {
                system.systemRoot = null;
                currentRoot.RemoveSystemFromRoot(system);
                foreach (var store in currentRoot.stores) {
                    system.RemoveStoreInternal(store);
                }
            }
        }
        #endregion
        
    #region get systems of systems sub tree
        internal Array<BaseSystem> GetSubSystems(ref Array<BaseSystem> systemBuffer)
        {
            systemBuffer.Clear();
            AddSubSystems(ref systemBuffer, this);
            return systemBuffer;
        }
        
        private static void AddSubSystems(ref Array<BaseSystem> array, BaseSystem system)
        {
            array.Add(system);
            if (system is SystemGroup systemGroup) {
                foreach (var child in systemGroup.childSystems) {
                    AddSubSystems(ref array, child);
                }
            }
        }
        #endregion
    }
}