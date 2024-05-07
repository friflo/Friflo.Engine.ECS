// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Diagnostics;
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
        [Browse(Never)] public override string                      Name            => name;
        [Browse(Never)] public          bool                        PerfEnabled     => perfEnabled;
                        public          ReadOnlyList<BaseSystem>    ChildSystems    => childSystems;
                        internal        ReadOnlyList<CommandBuffer> CommandBuffers  => commandBuffers; // only for debug view
                        public override string                      ToString()      => $"'{name}' Group - child systems: {childSystems.Count}";
        #endregion
        
    #region fields
        [Serialize] [Browse(Never)] private     string                      name;
                    [Browse(Never)] internal    ReadOnlyList<BaseSystem>    childSystems;
                    [Browse(Never)] internal    ReadOnlyList<CommandBuffer> commandBuffers;
        [Ignore]    [Browse(Never)] private     bool                        perfEnabled;
        #endregion
        
    #region constructor
        /// <summary> Default constructor required to create SystemGroup for deserialization </summary>
        internal SystemGroup() {
            childSystems    = new ReadOnlyList<BaseSystem>(Array.Empty<BaseSystem>());
            commandBuffers  = new ReadOnlyList<CommandBuffer>(Array.Empty<CommandBuffer>());
        } 
        
        public SystemGroup(string name) {
            if (name is null or "") throw new ArgumentException("group name must not be null or empty");
            this.name       = name;
            childSystems    = new ReadOnlyList<BaseSystem>(Array.Empty<BaseSystem>());
            commandBuffers  = new ReadOnlyList<CommandBuffer>(Array.Empty<CommandBuffer>());
        }
        #endregion
        
    #region group: add / insert / remove / get - system
        public void AddSystem(BaseSystem system)
        {
            if (system == null)             throw new ArgumentNullException(nameof(system));
            if (system is SystemRoot)       throw new ArgumentException($"{nameof(SystemRoot)} must not be a child system", nameof(system));
            if (system.ParentGroup != null) throw new ArgumentException($"system already added to Group '{system.ParentGroup.Name}'", nameof(system));
            childSystems.Add(system);
            system.SetParentAndRoot(this);
            // Send event. See: SEND_EVENT notes
            CastSystemAdded(system);
        }
        
        public void InsertSystemAt(int index, BaseSystem system)
        {
            if (system == null)                             throw new ArgumentNullException(nameof(system));
            if (system is SystemRoot)                       throw new ArgumentException($"{nameof(SystemRoot)} must not be a child system", nameof(system));
            if (system.ParentGroup != null)                 throw new ArgumentException($"system already added to Group '{system.ParentGroup.Name}'", nameof(system));
            if (index < -1 || index > childSystems.Count)   throw new ArgumentException($"invalid index: {index}");
            if (index == -1) {
                childSystems.Add(system);
            } else {
                childSystems.InsertAt(index, system);
            }
            system.SetParentAndRoot(this);
            // Send event. See: SEND_EVENT notes
            CastSystemAdded(system);
        }
        
        public void RemoveSystem(BaseSystem system)
        {
            if (system == null)             throw new ArgumentNullException(nameof(system));
            if (system.ParentGroup != this) throw new ArgumentException($"system not child of Group '{Name}'", nameof(system));
            var oldRoot = system.SystemRoot;
            
            foreach (var child in childSystems) {
                if (child == system) {
                    childSystems.Remove(child);
                    system.ClearParentAndRoot();
                    CastSystemRemoved(system, oldRoot, this);
                    return;
                }
            }
        }
        #endregion
        
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
        
    #region system: update
        public void Update(Tick tick) {
            if (!enabled) {
                ClearPerfTicks(this);
                return;
            }
            Tick = tick;
            var start = perfEnabled ? Stopwatch.GetTimestamp() : 0;
            OnUpdateGroup();
            SetPerfTicks(this, start);
            Tick = default;
        }
    
        protected internal override void OnUpdateGroup()
        {
            var children = childSystems;
            var commands = commandBuffers;
            // --- clear command buffers in case Playback() was not called in a previous Update() caused by an exception
            for (int n = 0; n < commands.Count; n++) { commands[n].Clear(); }
            
            // --- calls OnUpdateGroupBegin() once per child system.
            for (int n = 0; n < children.Count; n++) {
                var child = children[n];
                if (!child.enabled) continue;
                child.Tick = Tick;
                child.OnUpdateGroupBegin();
            }
            // --- calls OnUpdate() for every QuerySystem child and every store in SystemRoot.Stores - commonly a single store.
            for (int n = 0; n < children.Count; n++) {
                var child = children[n];
                if (!child.enabled) { ClearPerfTicks(child); continue; }
                var start = perfEnabled ? Stopwatch.GetTimestamp() : 0;
                child.OnUpdateGroup();
                SetPerfTicks(child, start);
            }
            // --- apply command buffer changes
            for (int n = 0; n < commands.Count; n++) { commands[n].Playback(); }
            
            // --- calls OnUpdateGroupEnd() once per child system.
            for (int n = 0; n < children.Count; n++) {
                var child = children[n];
                if (!child.enabled) continue;
                child.OnUpdateGroupEnd();
                child.Tick = default;
            }
        }
        #endregion
        
    #region perf
        private static void SetPerfTicks(BaseSystem system, long start)
        {
            if (start == 0) {
                return;
            }
            var time                = Stopwatch.GetTimestamp();
            var duration            = time - start;
            var history             = system.perf.history;
            var index               = system.perf.updateCount++ % history.Length;
            history[index]          = duration;
            system.perf.lastTicks   = duration;
            system.perf.sumTicks   += duration;
        }
        
        private void ClearPerfTicks(BaseSystem system)
        {
            if (!perfEnabled) return;
            system.perf.lastTicks = -1;
            if (system is SystemGroup systemGroup) {
                foreach (var child in systemGroup.childSystems) {
                    ClearPerfTicks(child);
                }
            }
        }
    
        public void SetPerfEnabled(bool enable)
        {
            perfEnabled = enable;
            foreach (var child in childSystems) {
                if (child is SystemGroup systemGroup) {
                    systemGroup.SetPerfEnabled(enable);
                }
            }
        }
        #endregion
    }
}