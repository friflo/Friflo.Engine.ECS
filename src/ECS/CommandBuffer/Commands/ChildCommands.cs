// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal struct ChildCommand
{
    internal    int                         parentId;   //  4
    internal    int                         childId;    //  4
    internal    ChildEntitiesChangedAction  action;     //  1

    public override string ToString() => $"entity: {parentId} - {action} child: {childId}";
}
