// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Return the <see cref="Script"/>'s added to an <see cref="Entity"/>.
/// </summary>
public struct EntityScripts : IEnumerable<Script>
{
#region public properties
    /// <summary>Return the number of <see cref="Script"/>'s of an entity.</summary>
    public              int                     Count       => scripts.Length;
    
    public   override   string                  ToString()  => GetString();
    #endregion
    
#region internal fields
                    internal readonly   int         id;         //  4   - entity containing the scripts
    /// <summary>
    /// Invariant:<br/>
    /// <see cref="id"/> == 0   :   <see cref="scripts"/> == null<br/>
    /// <see cref="id"/>  > 0   :   <see cref="scripts"/> != null  <b>and</b> its Length > 0 
    /// </summary>
    [Browse(Never)] internal            Script[]    scripts;    //  8   - scripts contained by an entity
    #endregion
    

    internal EntityScripts (int id, Script[] scripts)
    {
        this.id         = id;
        this.scripts    = scripts;
    }
    
    public readonly EntityScriptsEnumerator GetEnumerator()                     => new EntityScriptsEnumerator (scripts);

    // --- IEnumerable
    readonly        IEnumerator             IEnumerable.GetEnumerator()         => new EntityScriptsEnumerator (scripts);

    // --- IEnumerable<>
    readonly        IEnumerator<Script>     IEnumerable<Script>.GetEnumerator() => new EntityScriptsEnumerator (scripts);
    
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

/// <summary>
/// Used to enumerate the <see cref="Script"/>'s added to an <see cref="Entity"/>.
/// </summary>
public struct EntityScriptsEnumerator : IEnumerator<Script>
{
    private             int         index;
    private readonly    Script[]    scripts;
    
    // --- IEnumerator
    public          void            Reset()             { index = 0; }

    readonly        object          IEnumerator.Current => Current;

    public readonly Script          Current             => scripts[index - 1];
    
    internal EntityScriptsEnumerator(Script[] scripts) {
        this.scripts    = scripts;
    }
    
    // --- IEnumerator
    public bool MoveNext() {
        if (index < scripts.Length) {
            index++;
            return true;
        }
        return false;
    }

    public readonly void Dispose() { }
} 