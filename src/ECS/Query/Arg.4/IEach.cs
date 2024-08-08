// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public interface IEach<T1, T2, T3, T4>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
{
    void Execute(ref T1 c1, ref T2 c2, ref T3 c3, ref T4 c4);
}

public interface IEachEntity<T1, T2, T3, T4>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
{
    void Execute(ref T1 c1, ref T2 c2, ref T3 c3, ref T4 c4, int id);
}