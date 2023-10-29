// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using Friflo.Json.Fliox;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[Component("unresolved")]
public struct Unresolved : IComponent
{
    public          Dictionary<string, JsonValue>   components;
    public          HashSet<string>                 tags;
    
    public override string                          ToString() => GetString(); 
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append("unresolved");
        if (components != null) {
            sb.Append(" components: ");
            foreach (var pair in components) {
                sb.Append('\'');
                sb.Append(pair.Key);
                sb.Append("', ");
            }
            sb.Length -= 2;
        }
        if (tags != null) {
            sb.Append(" tags: ");
            foreach (var tag in tags) {
                sb.Append('\'');
                sb.Append(tag);
                sb.Append("', ");
            }
            sb.Length -= 2;
        }
        return sb.ToString();
    }
}
