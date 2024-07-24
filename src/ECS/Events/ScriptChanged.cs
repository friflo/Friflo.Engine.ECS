// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Diagnostics.CodeAnalysis;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace Friflo.Engine.ECS;

/// <summary>
/// The modification type of a <see cref="ScriptChanged"/> event: <see cref="Remove"/>, <see cref="Add"/> or <see cref="Replace"/> script.
/// </summary>
public enum ScriptChangedAction : byte
{
    /// <summary> A <see cref="Script"/> was removed from an <see cref="Entity"/>. </summary>
    Remove  = 0,
    /// <summary> A <see cref="Script"/> was added to an <see cref="Entity"/>. </summary>
    Add     = 1,
    /// <summary> An entity <see cref="Script"/> was replaced by another script. </summary>
    Replace = 2,
}

/// <summary>
/// Is the event for event handlers added to <see cref="ECS.Entity.OnScriptChanged"/>,
/// <see cref="EntityStore.OnScriptAdded"/> or <see cref="EntityStore.OnScriptRemoved"/>.
/// </summary>
/// <remarks>
/// <br/>
/// These events are fired on:
/// <list type="bullet">
///     <item><see cref="Entity.AddScript{TScript}"/></item>
///     <item><see cref="Entity.RemoveScript{T}"/></item>
/// </list>
/// </remarks>
public readonly struct  ScriptChanged
{
    /// <summary>The <see cref="Entity"/> that emitted the event - aka the publisher / subject.</summary>
    public readonly     Entity              Entity;     // 16
    
    /// <summary>The executed entity change: <see cref="ScriptChangedAction.Remove"/>,
    /// <see cref="ScriptChangedAction.Add"/> or <see cref="ScriptChangedAction.Replace"/> script.</summary>
    public readonly     ScriptChangedAction Action;     //  4
    
    /// <summary>
    /// The new <see cref="ECS.Script"/> after executing <see cref="ScriptChangedAction.Add"/> or <see cref="ScriptChangedAction.Replace"/>.<br/>
    /// Is null in case of <see cref="ScriptChangedAction.Remove"/>.
    /// </summary>
    /// <remarks>
    /// Use the following code snippet to switch on <see cref="Script"/> type:
    /// <br/>
    /// <code>
    ///     switch (args.Script) {
    ///         case TestScript1 script1:
    ///             break;
    ///         case TestScript2 script2:
    ///             break;
    ///     }
    /// </code>
    /// </remarks>
    public readonly     Script              Script;     //  8
    
    /// <summary>
    /// The <see cref="ECS.Script"/> before executing <see cref="ScriptChangedAction.Remove"/> or <see cref="ScriptChangedAction.Replace"/>.<br/>
    /// Is null in case of <see cref="ScriptChangedAction.Add"/>.
    /// </summary>
    public readonly     Script              OldScript;  //  8
    
    /// <summary>The <see cref="ECS.ScriptType"/> of the remove, added or replaced script.</summary>
    public readonly     ScriptType          ScriptType; //  8
    
    // --- properties
    /// <summary>The <see cref="EntityStore"/> containing the <see cref="Entity"/> that emitted the event.</summary>
    public readonly     EntityStore         Store => Entity.store;
    
    /// <remarks>Intentionally not exposed to direct using <see cref="Script"/> instead.</remarks>
    [Browse(Never)][ExcludeFromCodeCoverage]
    // ReSharper disable once UnusedMember.Local
    private             Type                Type        => ScriptType.Type;
    
    public override     string              ToString()  => $"entity: {Entity.Id} - event > {Action} {ScriptType}";

    internal ScriptChanged(Entity entity, ScriptChangedAction action, Script script, Script oldScript, ScriptType scriptType)
    {
        Entity      = entity;
        Action      = action;
        Script      = script;
        OldScript   = oldScript;
        ScriptType  = scriptType;
    }
}