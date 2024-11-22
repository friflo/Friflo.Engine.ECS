// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal interface IComponentStash
{
    internal object GetStashDebug();
}

internal interface IComponentStash<T> : IComponentStash
    where T : struct
{
    internal ref T GetStashRef();
}