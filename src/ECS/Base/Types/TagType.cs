// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Engine.ECS.SchemaTypeKind;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Provide meta data for an <see cref="ITag"/> struct.
/// </summary>
public sealed class TagType : SchemaType 
{
#region fields
    /// <summary> The key name of an <see cref="ITag"/> used for JSON serialization. </summary>
    public   readonly   string  TagName;        //  8
    /// <summary> The index in <see cref="EntitySchema"/>.<see cref="EntitySchema.Tags"/>. </summary>
    public   readonly   int     TagIndex;       //  4
    #endregion

#region methods

    public  override    string  ToString() => $"tag: [#{Name}]";
    
    internal TagType(string tagName, Type type, int tagIndex)
        : base(null, type, Tag)
    {
        TagName    = tagName;
        TagIndex   = tagIndex;
    }
    #endregion
}

internal static class TagInfo<T>
    where T : struct, ITag
{
    // Check initialization by directly calling unit test method: Test_SchemaType.Test_SchemaType_Tag_Index()
    // readonly improves performance significant
    internal static readonly  int     Index = SchemaTypeUtils.GetTagIndex(typeof(T));
}

