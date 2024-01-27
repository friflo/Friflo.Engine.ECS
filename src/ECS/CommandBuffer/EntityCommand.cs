// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal enum EntityCommandAction
{
    Add     = 0,
    Remove  = 1
}

internal struct EntityCommand
{
    internal EntityCommandAction    action;
    internal int                    entityId;
}