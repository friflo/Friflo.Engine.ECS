// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Fliox.Engine.ECS.SchemaTypeKind;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed class TagType : SchemaType 
{
    /// <summary>
    /// The index in <see cref="EntitySchema.Tags"/>.<br/>
    /// </summary>
    public   readonly   int             tagIndex;       //  4
    
    public  override    string  ToString() => $"tag: [#{type.Name}]";
    
    internal TagType(Type type, int tagIndex)
        : base(null, type.Name, type, Tag, 0)
    {
        this.tagIndex = tagIndex;
    }
}
