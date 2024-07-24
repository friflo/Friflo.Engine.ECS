// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable once CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global
namespace Friflo.Engine.ECS.Systems;

/// <summary>
/// The type of <see cref="SystemChanged"/> event. <br/>
/// See: <see cref="Remove"/>, <see cref="Add"/>, <see cref="Move"/> or <see cref="Update"/> a system. 
/// </summary>
public enum SystemChangedAction
{
    Remove  = 0,
    Add     = 1,
    Move    = 2,
    Update  = 3,
}

/// <summary>
/// The event for event handlers added to <see cref="BaseSystem.OnSystemChanged"/>.
/// </summary>
/// <remarks>
/// These events are fired on:
/// <list type="bullet">
///     <item><see cref="SystemGroup.Add"/></item>
///     <item><see cref="SystemGroup.Insert"/></item>
///     <item><see cref="SystemGroup.Remove"/></item>
///     <item><see cref="BaseSystem.MoveSystemTo"/></item>
///     <item><see cref="BaseSystem.CastSystemUpdate"/></item>
/// </list>
/// </remarks>
public readonly struct SystemChanged
{
    /// <summary> The <see cref="SystemChangedAction"/> type of the system change. </summary>
    public readonly     SystemChangedAction action;

    /// <summary> The changed system. </summary>
    public readonly     BaseSystem          system;

    /// <summary> The name of the changed system field. </summary>
    public readonly     string              field;

    /// <summary> The value of a changed system field. </summary>
    public readonly     object              value;

    public override     string              ToString() => GetString();

    internal SystemChanged(SystemChangedAction action, BaseSystem system, string field, object value) {
        this.action = action;
        this.system = system;
        this.field  = field;
        this.value  = value;
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append(action);
        sb.Append(" - ");
        sb.Append(system is SystemGroup ? "Group" : "System");
        sb.Append(" '");
        sb.Append(system.Name);
        sb.Append('\'');
        switch (action) {
            case SystemChangedAction.Add:
                sb.Append(" to '");
                sb.Append(system.ParentGroup.Name);
                sb.Append('\'');
                break;
            case SystemChangedAction.Remove:
                sb.Append(" from '");
                var oldParent = (SystemGroup)value;
                sb.Append(oldParent.Name);
                sb.Append('\'');
                break;
            case  SystemChangedAction.Move:
                sb.Append(" from '");
                oldParent = (SystemGroup)value;
                sb.Append(oldParent.Name);
                sb.Append("' to '");
                sb.Append(system.ParentGroup.Name);
                sb.Append('\'');
                break;
            case SystemChangedAction.Update:
                if (field != null) {
                    sb.Append(" field: ");
                    sb.Append(field);
                    sb.Append(", value: ");
                    sb.Append(value);
                }
                break;
        }
        return sb.ToString();
    }
}
