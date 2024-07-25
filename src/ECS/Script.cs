// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

namespace Friflo.Engine.ECS;

/// <summary>
/// To enable adding a script class to an <see cref="ECS.Entity"/> it need to extend <see cref="Script"/>.<br/>
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/general#script">Example.</a>
/// </summary>
/// <remarks>
/// A <see cref="Script"/> is a reference type - a class-  which contains data <b>and</b> behavior - aka scripts / methods.<br/> 
/// An <see cref="ECS.Entity"/> can contain multiple <see cref="Script"/>'s but only one of each type.<br/>
/// <see cref="Script"/>'s can be used if <b>OPP</b> programming approach is preferred
/// and dealing with less than a few 1.000 instances.<br/>
/// <br/>
/// Optionally attribute the extended class with <see cref="ComponentKeyAttribute"/><br/>
/// to assign a custom component key name used for JSON serialization.<br/>
/// <br/>
/// <i>Info:</i> Its functionality is similar to a class extending <c>MonoBehaviour</c> added to a <c>GameObject</c> in Unity.
/// </remarks>
public abstract class Script
{
    // --- public
    /// <summary>The entity the component is added to. Otherwise null.</summary>
    public          Entity      Entity  => entity;
    public          EntityStore Store   => entity.store;
                    
    // --- internal
    [Browse(Never)] internal    Entity  entity;     // 16
    
    public override string      ToString()  => $"[*{GetType().Name}]";

    public  virtual     void    Start()     { }
    public  virtual     void    Update()    { }
}
 