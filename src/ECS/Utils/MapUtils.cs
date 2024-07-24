// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class MapUtils
{
    /* internal static ref TValue GetValueRefOrAddDefault<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, ref bool exists) where TKey : notnull
    {
        return ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out exists);
    } */
    
    /// <summary>
    /// Is used if CollectionsMarshal.GetValueRefOrAddDefault() is not available - !NET6_0_OR_GREATER
    /// </summary>
    internal static void Set<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
#if !NET6_0_OR_GREATER
        dictionary[key] = value;
#endif
    }
}