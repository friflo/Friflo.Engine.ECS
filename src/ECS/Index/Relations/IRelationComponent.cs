// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal interface IRelationComponent : IComponent
{
}

internal interface IRelationComponent<out TValue> : IRelationComponent
{
    TValue GetRelation();
}