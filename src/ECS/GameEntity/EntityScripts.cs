// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public struct EntityScripts
{
    internal readonly   int                 id;
    /// <summary>
    /// Invariant:<br/>
    /// <see cref="id"/> == 0   :   <see cref="scripts"/> == null<br/>
    /// <see cref="id"/>  > 0   :   <see cref="scripts"/> != null  <b>and</b> its Length > 0 
    /// </summary>
    internal            Script[]            scripts;
    
    public   override   string              ToString() => GetString();

    internal EntityScripts (int id, Script[] scripts)
    {
        this.id         = id;
        this.scripts    = scripts;
    }
    
    private string GetString()
    {
        if (scripts == null) {
            return "unused";
        }
        var sb = new StringBuilder();
        sb.Append("id: ");
        sb.Append(id);
        sb.Append("  [");
        foreach (var script in scripts) {
            sb.Append('*');
            sb.Append(script.GetType().Name);
            sb.Append(", ");
        }
        sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }
}