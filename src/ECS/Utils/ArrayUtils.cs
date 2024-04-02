// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class TypeExtensions
{
    /// create unique id for Type. <see cref="Type.GUID"/> is no option as it is magnitudes slower
    internal static long Handle(this Type type) {
        return type.TypeHandle.Value.ToInt64();
    }
}

internal static class ArrayUtils
{
    internal static T[] Resize<T>(ref T[] array, int len)
    {
        var newArray = new T[len];
        if (array != null) {
            var curLength   = array.Length;
            var source      = new ReadOnlySpan<T>(array, 0, curLength);
            var target      = new Span<T>(newArray,      0, curLength);
            source.CopyTo(target);
        }
        return array = newArray;
    }
    
    internal static void Resize<T>(ref T[] array, int capacity, int count)
    {
        var newArray    = new T[capacity];
        var source      = new ReadOnlySpan<T>(array, 0, count);
        var target      = new Span<T>(newArray,      0, count);
        source.CopyTo(target);
        array = newArray;
    }
}