// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using Friflo.Engine.ECS.Utils;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable UseCollectionExpression
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems;

/// <summary>
/// Contains a list of <see cref="ChildSystems"/> which are executed by calling <see cref="Update"/>. <br/>
/// Each group has a <see cref="CommandBuffer"/> per <see cref="EntityStore"/>.
/// </summary>
[DebuggerTypeProxy(typeof(SystemGroupDebugView))]
// IEnumerable + Add() enables collection initializer.
// IEnumerable<> cannot be used as Friflo.Json.Fliox Mapper does not support Read() for IEnumerable<>.
public class SystemGroup : BaseSystem, IEnumerable
{
#region properties
    /// <summary> The name of the group. Can be changed by <see cref="SetName"/>. </summary>
    [Browse(Never)] public override string                      Name            => name;
    
    /// <summary> If true the execution statics of its <see cref="ChildSystems"/> are collected. </summary>
    [Browse(Never)] public          bool                        MonitorPerf     => monitorPerf;

    /// <summary> The child systems added to the group. </summary>
    [Browse(Never)] public          ReadOnlyList<BaseSystem>    ChildSystems    => childSystems;

    /// <summary> The <see cref="ECS.CommandBuffer"/>'s shared by all <see cref="ChildSystems"/>. </summary>
                    internal        ReadOnlyList<CommandBuffer> CommandBuffers  => commandBuffers; // only for debug view
    
                    // only for display in debugger
                    internal        Item[]                      AllSystems      => Item.GetAllSystems(this);
                    
                    public override string                      ToString()      => $"'{name}' Group - child systems: {childSystems.Count}";
    #endregion
    
#region fields
    [Browse(Never)] [Serialize] private     string                      name;
    [Browse(Never)]             internal    ReadOnlyList<BaseSystem>    childSystems;
    [Browse(Never)]             internal    ReadOnlyList<CommandBuffer> commandBuffers;
    [Browse(Never)] [Ignore]    private     bool                        monitorPerf;
    #endregion
    
#region constructor
    /// <summary>
    /// Default constructor required to create a SystemGroup via deserialization.<br/>
    /// Group <see cref="Name"/> is set to <c>System</c> if missing in serialized JSON data.
    /// </summary>
    internal SystemGroup() {
        name            = "System";
        childSystems    = new ReadOnlyList<BaseSystem>(Array.Empty<BaseSystem>());
        commandBuffers  = new ReadOnlyList<CommandBuffer>(Array.Empty<CommandBuffer>());
    } 
    
    /// <summary>
    /// Creates a group with the passed <paramref name="name"/>.
    /// </summary>
    public SystemGroup(string name) {
        if (name is null or "") throw new ArgumentException("group name must not be null or empty");
        this.name       = name;
        childSystems    = new ReadOnlyList<BaseSystem>(Array.Empty<BaseSystem>());
        commandBuffers  = new ReadOnlyList<CommandBuffer>(Array.Empty<CommandBuffer>());
    }
    #endregion
    
#region enumerator
    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ChildSystems"/>.
    /// </summary>
    public       ReadOnlyListEnumerator<BaseSystem> GetEnumerator() => new ReadOnlyListEnumerator<BaseSystem>(childSystems);
    IEnumerator                         IEnumerable.GetEnumerator() => new ReadOnlyListEnumerator<BaseSystem>(childSystems);
    #endregion
    
#region group: add / insert / remove - system
    /// <summary>
    /// Adds the passed <paramref name="system"/> to the group.
    /// </summary>
    // Add() enables support for collection initializer
    public void Add(BaseSystem system)
    {
        if (system == null)             throw new ArgumentNullException(nameof(system));
        if (system is SystemRoot)       throw ExceptionUtils.ArgumentException($"{nameof(SystemRoot)} must not be a child system", nameof(system));
        if (system.ParentGroup != null) throw ExceptionUtils.ArgumentException($"system already added to Group '{system.ParentGroup.Name}'", nameof(system));
        childSystems.Add(system);
        system.SetParentAndRoot(this);
        // Send event. See: SEND_EVENT notes
        CastSystemAdded(system);
    }
    
    /// <summary>
    /// Adds the passed <paramref name="system"/> at the given <paramref name="index"/> to the group.<br/>
    /// If <paramref name="index"/> == -1 the system is added to the tail of the group.
    /// </summary>
    public void Insert(int index, BaseSystem system)
    {
        if (system == null)                             throw new ArgumentNullException(nameof(system));
        if (system is SystemRoot)                       throw ExceptionUtils.ArgumentException($"{nameof(SystemRoot)} must not be a child system", nameof(system));
        if (system.ParentGroup != null)                 throw ExceptionUtils.ArgumentException($"system already added to Group '{system.ParentGroup.Name}'", nameof(system));
        if (index < -1 || index > childSystems.Count)   throw ExceptionUtils.ArgumentException($"invalid index: {index}", nameof(index));
        if (index == -1) {
            childSystems.Add(system);
        } else {
            childSystems.Insert(index, system);
        }
        system.SetParentAndRoot(this);
        // Send event. See: SEND_EVENT notes
        CastSystemAdded(system);
    }
    
    /// <summary>
    /// Removes the passed <paramref name="system"/> from the group.
    /// </summary>
    public void Remove(BaseSystem system)
    {
        if (system == null)             throw new ArgumentNullException(nameof(system));
        if (system.ParentGroup != this) throw ExceptionUtils.ArgumentException($"system not child of Group '{Name}'", nameof(system));
        var oldRoot = system.SystemRoot;
        var index   = childSystems.IndexOf(system); // index never -1
        childSystems.RemoveAt(index);
        system.ClearParentAndRoot();
        // Send event. See: SEND_EVENT notes
        CastSystemRemoved(system, oldRoot, this);
    }
    #endregion
    
#region group: find system
    /// <summary>
    /// Returns the group with the specified <paramref name="name"/>.
    /// </summary>
    public SystemGroup FindGroup(string name, bool recursive)
    {
        if (recursive) {
            return FindGroupRecursive(name);
        }
        foreach (var child in childSystems)
        {
            if (child is SystemGroup group) {
                if (child.Name == name)
                    return group;
            }
        }
        return null;
    }
    
    private SystemGroup FindGroupRecursive(string name)
    {
        foreach (var child in childSystems)
        {
            if (child is SystemGroup group) {
                if (child.Name == name)
                    return group;
                var result = group.FindGroupRecursive(name);
                if (result != null) {
                    return result;
                }
            }
        }
        return null;
    }
    
    /// <summary>
    /// Returns the system with of the specified type <typeparamref name="T"/>. 
    /// </summary>
    public T FindSystem<T>(bool recursive) where T : BaseSystem
    {
        if (recursive) {
            return FindSystemRecursive<T>();
        }
        foreach (var child in childSystems) {
            if (child is T querySystem) {
                return querySystem;
            }
        }
        return null;
    }
    
    private T FindSystemRecursive<T>() where T : BaseSystem
    {
        foreach (var child in childSystems) {
            if (child is T querySystem) {
                return querySystem;
            }
            if (child is SystemGroup group) {
                var result = group.FindSystemRecursive<T>();
                if (result != null) {
                    return result;
                }
            }
        }
        return null;
    }
    #endregion
    
#region group: general
    /// <summary>
    /// Returns true if the group is an ancestor of the passed <paramref name="system"/>.
    /// </summary>
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
    
    /// <summary>
    /// Changes the name of the system group.
    /// </summary>
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
    /// <summary>
    /// Execute all systems within the group.
    /// </summary>
    public void Update(UpdateTick tick) {
        if (!enabled) {
            ClearPerfTicks(this);
            return;
        }
        this.tick = tick;
        var start = monitorPerf ? new PerfResource() : default;
        OnUpdateGroup();
        SetPerfTicks(this, start);
    }

    /// <summary>
    /// Is called when executing <see cref="Update"/>.
    /// </summary>
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
            child.tick = tick;
            child.OnUpdateGroupBegin();
        }
        // --- calls QuerySystem.OnUpdate() for every store in SystemRoot.Stores - commonly a single store.
        for (int n = 0; n < children.Count; n++) {
            var child = children[n];
            if (!child.enabled) { ClearPerfTicks(child); continue; }
            var start = monitorPerf ? new PerfResource() : default;
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
        }
    }
    #endregion
    
#region perf
    /// <summary>
    /// Enable / disable performance monitoring of its <see cref="ChildSystems"/>. 
    /// </summary>
    public void SetMonitorPerf(bool enable)
    {
        monitorPerf = enable;
        foreach (var child in childSystems) {
            if (child is SystemGroup systemGroup) {
                systemGroup.SetMonitorPerf(enable);
            }
        }
    }
    
    private static void SetPerfTicks(BaseSystem system, in PerfResource start)
    {
        if (start.time == 0) {
            return;
        }
        var time                = Stopwatch.GetTimestamp();
        var duration            = time - start.time;
        var memory              = PerfResource.GetAllocatedBytes() - start.memory;
        var history             = system.perf.history;
        var index               = system.perf.updateCount++ % history.Length;
        history[index]          = duration;
        system.perf.lastTicks   = duration;
        system.perf.sumTicks   += duration;
        system.perf.lastMemory  = memory;
        system.perf.sumMemory  += memory;
    }
    
    private void ClearPerfTicks(BaseSystem system)
    {
        if (!monitorPerf) return;
        system.perf.lastTicks = -1;
        if (system is SystemGroup systemGroup) {
            foreach (var child in systemGroup.childSystems) {
                ClearPerfTicks(child);
            }
        }
    }
    
    internal override void AppendPerfStats(StringBuilder sb, int depth)
    {
        base.AppendPerfStats(sb, depth);
        foreach (var child in childSystems) {
            child.AppendPerfStats(sb, depth + 1);
        }
    }
    #endregion
}
