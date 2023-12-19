// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Specialized;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public enum ChildEntitiesChangedAction
{
    Add     = NotifyCollectionChangedAction.Add,
    Remove  = NotifyCollectionChangedAction.Remove,
//  Replace = NotifyCollectionChangedAction.Replace,
//  Move    = NotifyCollectionChangedAction.Move,
//  Reset   = NotifyCollectionChangedAction.Reset,
}