// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Can be added to an <see cref="Entity"/> to provide a descriptive name for debugging or in an editor.
/// </summary>
[ComponentKey("name")]
[ComponentSymbol("N", "0,0,0")]
public struct EntityName : IComponent
{
    /// <summary>Descriptive entity name for debugging or in an editor.</summary>
                    public  string  value;  //  8
    
    [Browse(Never)] public  byte[]  Utf8 => value == null ? null : Encoding.UTF8.GetBytes(value);
    
    public override         string  ToString() => $"'{value}'";

    public EntityName (string value) {
        this.value = value;
    }
}