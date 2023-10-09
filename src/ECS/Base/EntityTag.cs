// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// Use to create entity <b>Tag</b>'s by defining a struct without fields or properties extending <see cref="IEntityTag"/>
/// </summary>
public interface IEntityTag { }


internal static class TagTypeInfo<T>
    where T : struct, IEntityTag
{
    // ReSharper disable once StaticMemberInGenericType
    internal static readonly    int     TagIndex  = TagUtils.NewTagIndex();
}

public static class TagUtils
{
    private  static             int     _nextTagIndex             = 1;

    internal static int NewTagIndex() {
        return _nextTagIndex++;
    }
}
