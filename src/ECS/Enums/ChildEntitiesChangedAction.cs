// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Specialized;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// The modification type of an <see cref="ChildEntitiesChanged"/> event: <see cref="Add"/> or <see cref="Remove"/>.
/// </summary>
public enum ChildEntitiesChangedAction
{
    Add     = NotifyCollectionChangedAction.Add,
    Remove  = NotifyCollectionChangedAction.Remove,
//  Replace = NotifyCollectionChangedAction.Replace,
//  Move    = NotifyCollectionChangedAction.Move,
//  Reset   = NotifyCollectionChangedAction.Reset,
}