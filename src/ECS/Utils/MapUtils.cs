// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class MapUtils
{
    internal static ref TValue GetValueRefOrAddDefault<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, ref bool exists) where TKey : notnull
    {
        return ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out exists);
    }
}