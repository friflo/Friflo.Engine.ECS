// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[StructComponent("name")]
public struct EntityName : IStructComponent
{
                            public  string              Value   { readonly get => value; set => SetValue(value); }
    [Browse(Never)] public readonly ReadOnlySpan<byte>  Utf8    => new (utf8);

    [Browse(Never)][Ignore] private string              value;  //  8
    [Browse(Never)][Ignore] private byte[]              utf8;   //  8
    
    public override         string              ToString() => $"Name: \"{value}\"";

    public EntityName (string value) {
        Value = value;
    }
    
    private void SetValue(string value) {
        this.value  = value;
        utf8        = value != null ? Encoding.UTF8.GetBytes(value) : null;
    }
}