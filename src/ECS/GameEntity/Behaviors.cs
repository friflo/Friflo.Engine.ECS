// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal struct Behaviors
{
    internal readonly   int                 id;
    /// <summary>
    /// Invariant:<br/>
    /// <see cref="id"/> == 0   :   <see cref="classComponents"/> == null<br/>
    /// <see cref="id"/>  > 0   :   <see cref="classComponents"/> != null  <b>and</b> its Length > 0 
    /// </summary>
    internal            ClassComponent[]    classComponents;
    
    public   override   string              ToString() => GetString();

    internal Behaviors (int id, ClassComponent[] classComponents)
    {
        this.id                 = id;
        this.classComponents    = classComponents;
    }
    
    private string GetString()
    {
        if (classComponents == null) {
            return "unused";
        }
        var sb = new StringBuilder();
        sb.Append("id: ");
        sb.Append(id);
        sb.Append("  [");
        foreach (var component in classComponents) {
            sb.Append(component.GetType().Name);
            sb.Append(", ");
        }
        sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }
}