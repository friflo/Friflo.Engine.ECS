﻿// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Fliox.Engine.ECS.TreeMembership;

// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[Flags]
public enum NodeFlags : byte
{
    NullNode        = 0b_0000_0000,
    Created         = 0b_0000_0001,
    /// <summary>
    /// If set node is a <see cref="rootTreeNode"/>. Otherwise <see cref="floating"/>
    /// </summary>
    TreeNode        = 0b_0000_0010,
    // - prefab flags
    PrefabLink      = 0b_0001_0000, // link to prefab location
    OpMask          = 0b_0000_1100,
    OpKeep          = 0b_0000_0100, // keep components of prefab entity as they are
    OpModify        = 0b_0000_1000, // modify components of prefab entity
    OpRemove        = 0b_0000_1100, // remove prefab entity
}
