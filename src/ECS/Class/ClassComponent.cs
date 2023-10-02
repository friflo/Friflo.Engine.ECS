// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;


/// <summary>
/// Instances of classes extending <see cref="ClassComponent"/> can be added as component to a <see cref="GameEntity"/>.<br/>
/// Every instance can be added to only one <see cref="GameEntity"/> at a time.
/// </summary>
/// <remarks>
/// <b><c>class</c></b> components can be used if <b>OPP</b> programming approach is preferred
/// while dealing with a small amount (&lt; 100) of <see cref="GameEntity"/>'s.<br/>
/// <br/>
/// <i>Info:</i> Its functionality is similar to <c>MonoBehavior</c> added to <c>GameObject</c>'s in Unity
/// </remarks>
public abstract class ClassComponent
{
    // --- public
    /// <summary>The entity the component is added to. Otherwise null.</summary>
                    public          GameEntity  Entity      => entity;
                    
    // --- internal
    [Browse(Never)] internal        GameEntity  entity;
    
                    public override string      ToString()  => GetString();

    public virtual void Start() {}
    public virtual void Update() {}
    
    private string GetString() {
        var sb = new StringBuilder();
        sb.Append("[*");
        sb.Append(GetType().Name);
        sb.Append(']');
        return sb.ToString();
    }
}