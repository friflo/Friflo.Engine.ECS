using System;
using System.Reflection;

namespace Tests.Utils;

public static class ReflectionExtensions
{
    public static object GetInternalField(this Object obj, string name) {
        var type = obj.GetType();
        var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null) {
            throw new InvalidOperationException($"field not found. type: {type}, name: {name}");
        }
        return field.GetValue(obj);
    }
}