using System;

namespace Fliox.Engine.ECS;

internal static class TypeExtensions
{
    /// create unique id for Type. <see cref="Type.GUID"/> is no option as it is magnitudes slower
    internal static long Handle(this Type type) {
        return type.TypeHandle.Value.ToInt64();
    }
}

internal static class Utils
{
    internal static void Resize<T>(ref T[] array, int len) {
        var newArray = new T[len];
        Array.Copy(array, newArray, array.Length);
        array = newArray;
    }
}