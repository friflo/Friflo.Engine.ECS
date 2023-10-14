// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal enum BindNode
{
    /// <summary>
    /// Does not create and bind a <see cref="GameEntity"/> to an <see cref="EntityNode"/>.<br/>
    /// In this case <see cref="EntityNode"/>.<see cref="EntityNode.entity"/> is always null.
    /// </summary>
    None            = 0,
    
    /// <summary>
    /// Create and bind a <see cref="GameEntity"/> to an <see cref="EntityNode"/>.<br/>
    /// The <see cref="GameEntity"/> is available via <see cref="EntityNode"/>.<see cref="EntityNode.entity"/>.
    /// </summary>
    ToGameEntity    = 1
}
