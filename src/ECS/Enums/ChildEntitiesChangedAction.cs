// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Specialized;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// The modification type of an <see cref="ChildEntitiesChanged"/> event: <see cref="Add"/> or <see cref="Remove"/> entity.
/// </summary>
public enum ChildEntitiesChangedAction
{
    /// <summary> An entity was added as a child to another <see cref="Entity"/>. </summary>
    Add     = NotifyCollectionChangedAction.Add,
    /// <summary> A child entity was removed from an <see cref="Entity"/>. </summary>
    Remove  = NotifyCollectionChangedAction.Remove,
//  Replace = NotifyCollectionChangedAction.Replace,
//  Move    = NotifyCollectionChangedAction.Move,
//  Reset   = NotifyCollectionChangedAction.Reset,
}