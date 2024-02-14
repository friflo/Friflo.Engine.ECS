// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal enum BatchCommandType : byte
{
    AddComponent,
    RemoveComponent,
}

internal struct BatchCommand
{
    internal BatchCommandType    type;
    internal int                 typeIndex;
}

internal class BatchComponent { }

internal class BatchComponent<T> : BatchComponent where T : struct, IComponent
{
    internal T value;

    public override string ToString() => typeof(T).Name;
}
