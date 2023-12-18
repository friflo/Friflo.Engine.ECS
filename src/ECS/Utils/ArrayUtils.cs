// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal static class TypeExtensions
{
    /// create unique id for Type. <see cref="Type.GUID"/> is no option as it is magnitudes slower
    internal static long Handle(this Type type) {
        return type.TypeHandle.Value.ToInt64();
    }
}

internal static class ArrayUtils
{
    internal static void Resize<T>(ref T[] array, int len) {
        var newArray = new T[len];
        if (array != null) {
            Array.Copy(array, newArray, array.Length);
        }
        array = newArray;
    }
}