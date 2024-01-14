// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Engine.ECS.SchemaTypeKind;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public sealed class TagType : SchemaType 
{
    public   readonly   string  TagName;        //  8
    /// <summary>
    /// The index in <see cref="EntitySchema.Tags"/>.<br/>
    /// </summary>
    public   readonly   int     TagIndex;       //  4
    
    public  override    string  ToString() => $"tag: [#{Name}]";
    
    internal TagType(string tagName, Type type, int tagIndex)
        : base(null, type, Tag)
    {
        TagName    = tagName;
        TagIndex   = tagIndex;
    }
}
