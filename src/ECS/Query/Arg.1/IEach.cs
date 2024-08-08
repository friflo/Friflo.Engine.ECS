// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public interface IEach<T1>
    where T1 : IComponent
{
    void Execute(ref T1 c1);
}

public interface IEachEntity<T1>
    where T1 : IComponent
{
    void Execute(ref T1 c1, int id);
}