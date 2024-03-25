// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

// class is obsolete
internal static class StructInfo
{
    /// <summary> Is a multiple of 64. See <see cref="ComponentType{T}.PadCount512"/> </summary>
    internal const  int     ChunkSize           = 512; // check 64 - can be removed
    internal const  int     MissingAttribute    = 0;
}

internal static class StructUtils
{
    internal static int NewStructIndex(Type type)
    {
        var schema = EntityStoreBase.Static.EntitySchema;
        return schema.ComponentTypeByType[type].StructIndex;
    }
}
