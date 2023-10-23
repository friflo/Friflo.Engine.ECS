// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[StructComponent("behaviors")]
internal struct Behaviors : IStructComponent
{
    [Browse(Never)] internal    ClassComponent[]    classComponents;
    
    public          override    string              ToString() => GetString();

    internal Behaviors (ClassComponent[] classComponents)
    {
        this.classComponents = classComponents;
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var component in classComponents) {
            sb.Append(component.GetType().Name);
            sb.Append(", ");
        }
        sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }
}