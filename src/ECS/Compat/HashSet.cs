using System.Collections.Generic;

namespace Friflo.Engine.ECS.Compat;

#if NETSTANDARD && !NETSTANDARD2_1_OR_GREATER
public static class HashSet
{
    public static bool TryGetValue<T>(this HashSet<T> set, T value, out T actualValue)
    {
        if (set.Contains(value)) {
            actualValue = value;
            return true;
        }
        actualValue = default;
        return false;
    }
}
#endif
