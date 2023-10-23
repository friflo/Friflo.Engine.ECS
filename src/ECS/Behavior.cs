// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

namespace Friflo.Fliox.Engine.ECS;


/// <summary>
/// To enable adding <b>class</b> components to a <see cref="GameEntity"/> it need to extend <see cref="Behavior"/>.<br/>
/// A <b>class</b> component is a reference type which contains data <b>and</b> behavior / methods.<br/> 
/// A <see cref="GameEntity"/> can contain multiple class components but only one of each type.
/// </summary>
/// <remarks>
/// <b><c>class</c></b> components can be used if <b>OPP</b> programming approach is preferred
/// while dealing with a small amount (&lt; 100) of <see cref="GameEntity"/>'s.<br/>
/// <br/>
/// <i>Info:</i> Its functionality is similar to <c>MonoBehavior</c> added to <c>GameObject</c>'s in Unity
/// </remarks>
public abstract class Behavior
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
 