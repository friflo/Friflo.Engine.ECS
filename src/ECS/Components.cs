// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// Used to create entity <b>Tag</b>'s by defining a struct without fields or properties extending <see cref="IEntityTag"/>
/// </summary>
public interface IEntityTag { }


/// <summary>
/// To enable adding <b>struct</b> components to a <see cref="GameEntity"/> it need to extend <see cref="IComponent"/>.<br/>
/// A <b>struct</b> component is a value type which only contains data <b>but no</b> behavior / methods.<br/>
/// A <see cref="GameEntity"/> can contain multiple struct components but only one of each type.
/// </summary>
public interface IComponent { }
