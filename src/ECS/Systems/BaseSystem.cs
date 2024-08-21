// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems;

/// <summary>
/// Base class for all systems either a query system, a custom system or a system group.
/// </summary>
public abstract class BaseSystem
{
#region properties
    /// <summary>The <see cref="UpdateTick"/> passed to <see cref="SystemGroup.Update"/>. </summary>
    [Browse(Never)] [Ignore]    public ref readonly UpdateTick  Tick        => ref tick;
    
    /// <summary> The system name. The Name of a <see cref="SystemGroup"/> can be changed. </summary>
    [Browse(Never)]             public virtual      string      Name        => systemName;

    /// <summary> The <see cref="SystemRoot"/> containing this system. </summary>
    [Browse(Never)]             public              SystemRoot  SystemRoot  => systemRoot;

    /// <summary> The parent <see cref="SystemGroup"/> containing this system. </summary>
    [Browse(Never)]             public              SystemGroup ParentGroup => parentGroup;

    /// <summary> If true the system is executed when calling <see cref="SystemGroup.Update"/></summary>
    [Browse(Never)] [Ignore]    public              bool        Enabled     { get => enabled; set => enabled = value; }

    /// <summary> Unique system id of all systems of a <see cref="SystemRoot"/>. </summary>
    [Browse(Never)]             public              int         Id          => id;

    /// <summary> Provide execution statistics of a system if <see cref="SystemGroup.MonitorPerf"/> is enabled. </summary>
    [Browse(Never)]             public ref readonly SystemPerf  Perf        => ref perf;
    
                                internal            View        System      => view ??= new View(this);
    #endregion
        
#region internal fields
    [Browse(Never)] [Ignore]    internal            UpdateTick      tick;
    [Browse(Never)] [Serialize] internal            int             id;
    [Browse(Never)] [Serialize] internal            bool            enabled = true;
    [Browse(Never)]             private readonly    string          systemName;
    [Browse(Never)]             private             SystemGroup     parentGroup;
    [Browse(Never)]             private             SystemRoot      systemRoot;
    [Browse(Never)] [Ignore]    internal            SystemPerf      perf;
    [Browse(Never)]             private             View            view;
    [Browse(Never)]             private             StringBuilder   stringBuffer;
    #endregion
     
#region constructors
    protected BaseSystem() {
        systemName  = GetType().Name;
        perf        = new SystemPerf(new long[10]);
    }
    #endregion
    
#region system events
    /// <summary>
    /// Event handlers to notify a system has changed.<br/>
    /// Like a changed system field or a system added / removed to / from a <see cref="SystemGroup"/>.
    /// </summary>
    public event Action<SystemChanged>  OnSystemChanged;

    /// <summary>
    /// Send an event to <see cref="OnSystemChanged"/> handlers to notify a changed system <paramref name="field"/>.
    /// </summary>
    public void CastSystemUpdate(string field, object value)
    {
        var system  = this;
        var change  = new SystemChanged(SystemChangedAction.Update, system, field, value);
        var root    = system.SystemRoot;
        if (root != system) {
            system.OnSystemChanged?.Invoke(change);
        }
        root?.OnSystemChanged?.Invoke(change);
    }
    
    private static void CastSystemMoved(BaseSystem system, SystemGroup oldParent)
    {
        var change  = new SystemChanged(SystemChangedAction.Move, system, null, oldParent);
        var root    = system.SystemRoot;
        var parent  = system.parentGroup;
        if (parent == oldParent) {
            parent.OnSystemChanged?.Invoke(change);
        } else {
            oldParent.OnSystemChanged?.Invoke(change);
            parent.   OnSystemChanged?.Invoke(change);
        }
        if (root != parent && root != oldParent) {
            root?.OnSystemChanged?.Invoke(change);
        }
    }
    
    internal static void CastSystemAdded(BaseSystem system)
    {
        var change  = new SystemChanged(SystemChangedAction.Add, system, null, null);
        var root    = system.SystemRoot;
        var parent  = system.parentGroup;
        if (root != parent) {
            parent.OnSystemChanged?.Invoke(change);
        }
        root?.OnSystemChanged?.Invoke(change);
    }
    
    internal static void CastSystemRemoved(BaseSystem system, SystemRoot oldRoot, SystemGroup oldParent)
    {
        var change  = new SystemChanged(SystemChangedAction.Remove, system, null, oldParent);
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
    /// <summary>
    /// Called for every system of the parent <see cref="SystemGroup.ChildSystems"/>.
    /// </summary>
    protected internal  virtual void OnUpdateGroup      () { }
    
    /// <summary>
    /// Called for every system of the parent <see cref="SystemGroup.ChildSystems"/> before group <see cref="SystemGroup.OnUpdateGroup"/>.
    /// </summary>
    protected internal  virtual void OnUpdateGroupBegin () { }
    
    /// <summary>
    /// Called for every system of the parent <see cref="SystemGroup.ChildSystems"/> after group <see cref="SystemGroup.OnUpdateGroup"/>.
    /// </summary>
    protected internal  virtual void OnUpdateGroupEnd   () { }
    #endregion
    
#region system: move
    /// <summary>
    /// Move the system to the specified <paramref name="targetGroup"/> at the given <paramref name="index"/>.<br/>
    /// If <paramref name="index"/> is -1 the system is moved to the tail of the group.
    /// </summary>
    public int MoveSystemTo(SystemGroup targetGroup, int index)
    {
        if (targetGroup == null)                                    throw new ArgumentNullException(nameof(targetGroup));
        if (parentGroup == null)                                    throw new InvalidOperationException($"System '{Name}' has no parent");
        if (index < -1 || index > targetGroup.childSystems.Count)   throw new ArgumentException($"invalid index: {index}");
        if (parentGroup == targetGroup) {
            // case:    Change system position within its parent  
            var oldIndex = targetGroup.childSystems.Remove(this);
            if (index == -1) {
                targetGroup.childSystems.Add(this);
                index = targetGroup.childSystems.Count - 1;
            } else {
                if (index > oldIndex) index--;
                targetGroup.childSystems.Insert(index, this);
            }
            // Send event. See: SEND_EVENT notes
            CastSystemMoved(this, targetGroup); 
            return index;
        }
        if (systemRoot != targetGroup.systemRoot) {
            var expect = systemRoot.Name;
            var msg = $"Expect {nameof(targetGroup)} == {nameof(SystemRoot)}. Expected: '{expect}' was: '{targetGroup.Name}'";
            throw new InvalidOperationException(msg);
        }
        // case:    System moved to another parent group  
        var oldParent = parentGroup;
        oldParent.childSystems.Remove(this);
        if (index == -1) {
            targetGroup.childSystems.Add(this);
            index = targetGroup.childSystems.Count - 1;
        } else {
            targetGroup.childSystems.Insert(index, this);
        }
        parentGroup = targetGroup;
        // Send event. See: SEND_EVENT notes
        CastSystemMoved(this, oldParent);
        return index;
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
        if (newRoot == null) {
            return; // case: new parent is not part of a SystemRoot hierarchy
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
        parentGroup     = null;
        var currentRoot = systemRoot;
        if (currentRoot == null) {
            return; // case: system in not part of a SystemRoot hierarchy
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
    internal ReadOnlyList<BaseSystem> GetSubSystems(ref ReadOnlyList<BaseSystem> systemBuffer)
    {
        systemBuffer.Clear();
        AddSubSystems(ref systemBuffer, this);
        return systemBuffer;
    }
    
    private static void AddSubSystems(ref ReadOnlyList<BaseSystem> readOnlyList, BaseSystem system)
    {
        readOnlyList.Add(system);
        if (system is SystemGroup systemGroup) {
            foreach (var child in systemGroup.childSystems) {
                AddSubSystems(ref readOnlyList, child);
            }
        }
    }
    #endregion
    
#region perf
    /// <summary>
    /// Returns performance statistics formatted as a table intended for logging.
    /// </summary>
    public string GetPerfLog()
    {
        var sb = stringBuffer ??= new StringBuilder();
        sb.Clear();
        AppendPerfLog (sb);
        return sb.ToString();
    }
    
    /// <summary>
    /// Add performance statistics formatted as a table to the given <see cref="StringBuilder"/> without memory allocations.
    /// </summary>
    public void AppendPerfLog(StringBuilder stringBuilder) {
        var stores  = SystemRoot?.stores.count ?? 0;
        stringBuilder.Append($"stores: {stores,-3}                   on      last ms       sum ms      updates     last mem      sum mem     entities\n");
        stringBuilder.Append("---------------------         --     --------     --------     --------     --------     --------     --------\n");
        AppendPerfStats(stringBuilder, 0);
        stringBuilder.Replace(',', '.'); // no more patience with NumberFormatInfo
    }

    internal virtual void AppendPerfStats(StringBuilder sb, int depth)
    {
        var start = sb.Length;
        if (depth > 0) {
            sb.Append("| ");
        }
        for (int n = 1; n < depth; n++) {
            sb.Append("  ");
        }
        sb.Append(Name);
        if (this is SystemGroup group) {
            sb.Append(" [");
            sb.Append(group.childSystems.Count);
            sb.Append(']');
        }
        var len = 30 - (sb.Length - start);
        sb.Append(' ', len);
        sb.Append(enabled ? " +" : " -");
        sb.Append($" {(double)Perf.LastMs,12:0.000}"); // (double) prevents allocation
        sb.Append($" {(double)Perf.SumMs,12:0.000}");
        sb.Append($" {Perf.UpdateCount,12}");
        sb.Append($" {Perf.LastMemory,12}");
        sb.Append($" {Perf.SumMemory,12}");
        
        if (this is QuerySystemBase querySystem) {
            sb.Append($" {querySystem.EntityCount,12}");
        }
        sb.Append('\n');
    }
    #endregion
}
