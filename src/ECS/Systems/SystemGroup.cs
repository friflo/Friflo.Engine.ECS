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
                        public override string                      ToString()      => $"'{name}' Group - systems: {childSystems.Count}";
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
        
    #region update
        public override void Update(Tick tick)
        {
            if (!Enabled) {
                return;
            }
            var startGroup  = perfEnabled ? GetTimestamp() : 0;
            Tick            = tick;
            var children    = childSystems;
            
            // --- calls OnUpdateGroupBegin() once per child system.
            foreach (var child in children) {
                if (!child.Enabled) continue;
                child.Tick = tick;
                child.OnUpdateGroupBegin();
            }
            // --- calls OnUpdate() for every QuerySystem child and every store of SystemRoot.Stores - commonly a single store.
            var start = perfEnabled ? GetTimestamp() : 0;
            foreach (var child in children) {
                child.Update(tick);
                SetChildDuration(child, ref start);
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
            SetGroupDuration(ref startGroup);
        }
        #endregion
        
    #region perf
        public void SetPerfEnabled(bool enable)
        {
            perfEnabled = enable;
            foreach (var child in childSystems) {
                if (child is SystemGroup systemGroup) {
                    systemGroup.SetPerfEnabled(enable);
                }
            }
        }
        
        private static long GetTimestamp() {
            return Stopwatch.GetTimestamp();
        }
        
        private static void SetChildDuration(BaseSystem system, ref long start)
        {
            if (start == 0) {
                system.durationTicks = 0;
                return;
            }
            if (system is SystemGroup) {
                return; // case: durations are set by: SystemGroup.Update(Tick) -> SetGroupDuration()
            }
            var time                = Stopwatch.GetTimestamp();
            var duration            = time - start;
            system.durationTicks    = duration;
            system.durationSumTicks+= duration;
            start                   = time;
        }
        
        private void SetGroupDuration(ref long start)
        {
            if (start == 0) {
                durationTicks = 0;
                return;
            }
            var time            = Stopwatch.GetTimestamp();
            var duration        = time - start;
            durationTicks       = duration;
            durationSumTicks   += duration;
            start               = time;
        }
        #endregion
    }
}