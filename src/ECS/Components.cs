// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// Used to create entity <b>Tag</b>'s by defining a struct without fields or properties extending <see cref="IEntityTag"/>
/// </summary>
public interface IEntityTag { }


/// <summary>
/// To enable adding <b>struct</b> components to a <see cref="GameEntity"/> it need to extend <see cref="IStructComponent"/>.<br/>
/// A <b>struct</b> component is a value type which only contains data <b>but no</b> behavior / methods.<br/>
/// A <see cref="GameEntity"/> can contain multiple struct components but only one of each type.
/// </summary>
public interface IStructComponent { }


/// <summary>
/// To enable adding <b>class</b> components to a <see cref="GameEntity"/> it need to extend <see cref="ClassComponent"/>.<br/>
/// A <b>class</b> component is a reference type which contains data <b>and</b> behavior / methods.<br/> 
/// A <see cref="GameEntity"/> can contain multiple class components but only one of each type.
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
    
    public override string      ToString()  => $"[*{GetType().Name}]";

    public  virtual     void    Start()     { }
    public  virtual     void    Update()    { }
}
 