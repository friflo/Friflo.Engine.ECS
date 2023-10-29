// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[Component("name")]
public struct EntityName : IComponent
{
    public          string  value;  //  8
    
    public          byte[]  Utf8 => value == null ? null : Encoding.UTF8.GetBytes(value);
    
    public override string  ToString() => $"EntityName: '{value}'";

    public EntityName (string value) {
        this.value = value;
    }
}