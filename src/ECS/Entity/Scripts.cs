// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Contains the <see cref="Script"/>'s added to an entity.
/// </summary>
[DebuggerTypeProxy(typeof(ScriptsDebugView))]
public readonly struct Scripts : IEnumerable<Script>
{
#region properties
    /// <summary> return number ob <see cref="Script"/>'s. </summary>
    public              int                     Length      => array.Length;
    
    /// <summary> return the <see cref="Script"/>'s as a Span. </summary>
    [Browse(Never)]
    public              ReadOnlySpan<Script>    Span        => new (array);
    
    public   override   string                  ToString()  => $"Script[{array.Length}]";
    #endregion

#region fields
    internal readonly   Script[]                array;
    #endregion
    
#region methods
    internal Scripts(Script[] array) {
        this.array = array;    
    }
    
    public Script this[int index] => array[index];
    
    public ScriptsEnumerator        GetEnumerator()         => new ScriptsEnumerator (array);

    // --- IEnumerable
    IEnumerator         IEnumerable.GetEnumerator()         => new ScriptsEnumerator (array);

    // --- IEnumerable<>
    IEnumerator<Script> IEnumerable<Script>.GetEnumerator() => new ScriptsEnumerator (array);
    #endregion
}

/// <summary>
/// Enumerator for entity <see cref="Scripts"/>.
/// </summary>
public struct ScriptsEnumerator : IEnumerator<Script>
{
    private readonly    Script[]    scripts;
    private readonly    int         last;
    private             int         index;
    
    internal ScriptsEnumerator(Script[] scripts) {
        this.scripts    = scripts;
        last            = scripts.Length - 1;
        index           = -1;
    }
    
    // --- IEnumerator
    public          void         Reset()    => index = -1;

    readonly object  IEnumerator.Current    => scripts[index];

    public   Script              Current    => scripts[index];
    
    // --- IEnumerator
    public bool MoveNext()
    {
        if (index < last) {
            index++;
            return true;
        }
        return false;
    }

    public readonly void Dispose() { }
}


internal sealed class ScriptsDebugView
{
    [Browse(RootHidden)]
    public              Script[]    Items => scripts.array;
    
    private readonly    Scripts     scripts;
    
    internal ScriptsDebugView(Scripts scripts) {
        this.scripts = scripts;
    }
} 
