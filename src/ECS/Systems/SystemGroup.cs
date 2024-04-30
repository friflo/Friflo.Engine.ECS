// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable UseCollectionExpression
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems
{
    public class SystemGroup : BaseSystem
    {
    #region properties
        [Browse(Never)] public override string              Name            => name;
                        public          Array<BaseSystem>   ChildSystems    => childSystems;
                        internal        View                System          => view ??= new View(this);
                        internal        Array<CommandBuffer>CommandBuffers  => commandBuffers; // only for debug view
                        public override string              ToString()      => $"'{name}' Group - systems: {childSystems.Count}";
        #endregion
        
    #region fields
        [Serialize]
        [Browse(Never)] private     string                  name;
        [Browse(Never)] internal    Array<BaseSystem>       childSystems;
        [Browse(Never)] internal    Array<CommandBuffer>    commandBuffers;
        [Browse(Never)] private     View                    view;
        #endregion
        
    #region constructor
        /// <summary> Default constructor required to create SystemGroup for deserialization </summary>
        internal SystemGroup() {
            childSystems    = new Array<BaseSystem>(Array.Empty<BaseSystem>());
            commandBuffers  = new Array<CommandBuffer>(Array.Empty<CommandBuffer>());
        } 
        
        public SystemGroup(string name) {
            if (name is null or "") throw new ArgumentException("group name must not be null or empty");
            this.name       = name;
            childSystems    = new Array<BaseSystem>(Array.Empty<BaseSystem>());
            commandBuffers  = new Array<CommandBuffer>(Array.Empty<CommandBuffer>());
        }
        #endregion
        
    #region group: add / remove system
        public void AddSystem(BaseSystem system)
        {
            if (system == null)             throw new ArgumentNullException(nameof(system));
            if (system is SystemRoot)       throw new ArgumentException($"{nameof(SystemRoot)} must not be a child system", nameof(system));
            if (system.ParentGroup != null) throw new ArgumentException($"system already added to Group '{system.ParentGroup.Name}'", nameof(system));
            childSystems.Add(system);
            system.SetParentAndRoot(this);
            // Send event. See: SEND_EVENT notes
            system.CastSystemChanged(SystemChangedAction.Add);
        }
        
        public void RemoveSystem(BaseSystem system)
        {
            if (system == null)             throw new ArgumentNullException(nameof(system));
            if (system.ParentGroup != this) throw new ArgumentException($"system not child of Group '{Name}'", nameof(system));
            
            foreach (var child in childSystems) {
                if (child == system) {
                    childSystems.Remove(child);
                    var oldRoot = system.SystemRoot;
                    system.ClearParentAndRoot();
                    system.CastSystemRemoved(oldRoot, this);
                    return;
                }
            }
        }
        #endregion
        
        /*
        public void InsertSystemAt(int index, BaseSystem system)
        {
            if (system == null)                     throw new ArgumentNullException(nameof(system));
            if (index < 0 || index > systems.Count) throw new ArgumentException("index out of range", nameof(index));
            var currentParent   = system.parentGroup;
            var action          = SystemChangedAction.Add;
            if (currentParent != null) {
                action = SystemChangedAction.Move;
                foreach (var child in currentParent.systems) {
                    if (child == system) {
                        var oldIndex = currentParent.systems.Remove(child);
                        if (currentParent == this && index > oldIndex) {
                            index--;
                        }
                        break;
                    }
                }
            }
            system.parentGroup = this;
            systems.InsertAt(index, system);
            system.CastSystemChanged(action, null, null);
        } */
        
    #region group: find system
        public SystemGroup FindGroup(string name)
        {
            foreach (var child in childSystems)
            {
                if (child is SystemGroup group) {
                    if (child.Name == name)
                        return group;
                    var result = group.FindGroup(name);
                    if (result != null) {
                        return result;
                    }
                }
            }
            return null;
        }
        
        public T FindSystem<T>() where T : BaseSystem
        {
            foreach (var child in childSystems) {
                if (child is T querySystem) {
                    return querySystem;
                }
                if (child is SystemGroup group) {
                    var result = group.FindSystem<T>();
                    if (result != null) {
                        return result;
                    }
                }
            }
            return null;
        }
        #endregion
        
    #region group: general     
        public bool IsAncestorOf(BaseSystem system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            while (system.ParentGroup != null) {
                if (system.ParentGroup == this) {
                    return true;
                }
                system = system.ParentGroup;
            }
            return false;
        }
        
        public void SetName(string name) {
            if (name is null or "") throw new ArgumentException("group name must not be null or empty");
            this.name   = name;
            // Send event. See: SEND_EVENT notes
            CastSystemUpdate(nameof(Name), name);
        }
        #endregion
        
    #region store: add / remove
        internal override void AddStoreInternal(EntityStore entityStore)
        {
            var commandBuffer = entityStore.GetCommandBuffer();
            commandBuffer.ReuseBuffer = true;
            commandBuffers.Add(commandBuffer);
        }
        
        internal override void RemoveStoreInternal(EntityStore entityStore)
        {
            foreach (var commandBuffer in commandBuffers) {
                if (commandBuffer.EntityStore != entityStore) {
                    continue;
                }
                commandBuffers.Remove(commandBuffer);
                commandBuffer.ReturnBuffer();
                return;
            }
        }
        #endregion
        
    #region update
        public override void Update(Tick tick)
        {
            if (!Enabled) {
                return;
            }
            Tick = tick;
            var children = childSystems;
            // --- calls OnUpdateGroupBegin() once per child system.
            foreach (var child in children) {
                if (!child.Enabled) continue;
                child.Tick = tick;
                child.OnUpdateGroupBegin();
            }
            // --- calls OnUpdate() for every QuerySystem child and every store of SystemRoot.Stores - commonly a single store.
            foreach (var child in children) {
                child.Update(tick);
            }
            // --- apply command buffer changes
            foreach (var commandBuffer in commandBuffers) {
                commandBuffer.Playback();
            }
            // --- calls OnUpdateGroupEnd() once per child system.
            foreach (var child in children) {
                if (!child.Enabled) continue;
                child.OnUpdateGroupEnd();
                child.Tick = default;
            }
            Tick = default;
        }
        #endregion
        

    }
}