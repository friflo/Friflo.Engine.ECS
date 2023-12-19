// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public struct EntityScripts
{
    internal readonly   int         id;         //  4   - entity containing the scripts
    /// <summary>
    /// Invariant:<br/>
    /// <see cref="id"/> == 0   :   <see cref="scripts"/> == null<br/>
    /// <see cref="id"/>  > 0   :   <see cref="scripts"/> != null  <b>and</b> its Length > 0 
    /// </summary>
    internal            Script[]    scripts;    //  8   - scripts contained by an entity
    
    public   override   string      ToString() => GetString();

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