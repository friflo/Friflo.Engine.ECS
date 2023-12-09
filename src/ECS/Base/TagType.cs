// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedTypeParameter
namespace Friflo.Fliox.Engine.ECS;

internal static class TagType<T>
    where T : struct, ITag
{
    // ReSharper disable once StaticMemberInGenericType
    internal static readonly    int     TagIndex  = TagUtils.NewTagIndex();
}

internal static class TagUtils
{
    private  static             int     _nextTagIndex             = 1;

    internal static int NewTagIndex() {
        return _nextTagIndex++;
    }
}
