// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedTypeParameter

using System;

namespace Friflo.Fliox.Engine.ECS;

internal static class TagType<T>
    where T : struct, ITag
{
    // ReSharper disable once StaticMemberInGenericType
    internal static readonly    int     TagIndex  = TagUtils.NewTagIndex(typeof(T), out TagKey);
    internal static readonly    string  TagKey;
}

internal static class TagUtils
{
    private  static             int     _nextTagIndex             = 1;

    internal static int NewTagIndex(Type type, out string tagKey)
    {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(ComponentAttribute)) {
                continue;
            }
            var arg = attr.ConstructorArguments;
            tagKey  = (string) arg[0].Value;
            return _nextTagIndex++;
        }
        tagKey = type.Name;
        return _nextTagIndex++;
    }
}
