// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Flags assigned to <see cref="EntityNode"/>'s internally stored in an <see cref="EntityStore"/>.
/// </summary>
[Flags]
public enum NodeFlags : byte
{
    NullNode        = 0b_0000_0000,
    Created         = 0b_0000_0001,
    // <summary> If set a node is a <see cref="treeNode"/>. Otherwise <see cref="floating"/> </summary>
    // TreeNode        = 0b_0000_0010,
    // - prefab flags
    PrefabLink      = 0b_0001_0000, // link to prefab location
    OpMask          = 0b_0000_1100,
    OpKeep          = 0b_0000_0100, // keep components of prefab entity as they are
    OpModify        = 0b_0000_1000, // modify components of prefab entity
    OpRemove        = 0b_0000_1100, // remove prefab entity
}
