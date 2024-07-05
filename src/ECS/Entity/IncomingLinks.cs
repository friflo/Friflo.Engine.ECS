// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct IncomingLink
{
    public              Entity          Target      => new Entity(Entity.store, target);
    public  override    string          ToString()  => $"Entity: {Entity.Id} -> Target: {target}  [{Component.GetType().Name}]";
    
    public  readonly    Entity          Entity;     // 16
    public  readonly    IComponent      Component;  //  8
    private readonly    int             target;     //  4

    internal IncomingLink(in Entity entity, int target, IComponent component) {
        Entity      = entity;
        this.target = target;
        Component   = component;
    }
}

[DebuggerTypeProxy(typeof(IncomingLinksDebugView))]
public readonly struct IncomingLinks : IReadOnlyList<IncomingLink>

{
#region properties
    public                  int         Count       => incomingLinks.Length;
    public                  EntityStore Store       => target.store;
    public   override       string      ToString()  => $"EntityLinks[{Count}]";
    #endregion
    
#region fields
    [Browse(Never)] private  readonly   Entity          target;         // 16
    [Browse(Never)] internal readonly   IncomingLink[]  incomingLinks;  //  8
    #endregion
    
#region general
    internal IncomingLinks(in Entity target, IncomingLink[]  links) {
        this.target          = target;
        incomingLinks   = links;
    }
    
    public IncomingLink this[int index] => incomingLinks[index];
    
    public string Debug()
    {
        if (Count == 0) return "{ }";
        var sb = new StringBuilder();
        sb.Append("{ ");
        foreach (var link in incomingLinks) {
            if (sb.Length > 2) sb.Append(", ");
            sb.Append(link.Entity.Id);
        }
        sb.Append(" }");
        return sb.ToString();
    }
    #endregion

    
#region IEnumerator
    public IncomingLinkEnumerator                       GetEnumerator() => new IncomingLinkEnumerator (this);
    
    // --- IEnumerable
    IEnumerator                             IEnumerable.GetEnumerator() => new IncomingLinkEnumerator (this);

    // --- IEnumerable<>
    IEnumerator<IncomingLink> IEnumerable<IncomingLink>.GetEnumerator() => new IncomingLinkEnumerator (this);
    #endregion
}


public struct IncomingLinkEnumerator : IEnumerator<IncomingLink>
{
    private  readonly   IncomingLink[]  incomingLinks;  //  8
    private             int             index;          //  4
    
    internal IncomingLinkEnumerator(IncomingLinks incomingLinks) {
        this.incomingLinks  = incomingLinks.incomingLinks;
        index               = -1;
    }
    
    // --- IEnumerator
    public          void         Reset()    => index = -1;

    readonly object IEnumerator.Current    => incomingLinks[index];

    public   IncomingLink       Current    => incomingLinks[index];
    
    // --- IEnumerator
    public bool MoveNext()
    {
        if (index < incomingLinks.Length - 1) {
            index++;
            return true;
        }
        return false;
    }
    
    public readonly void Dispose() { }
}

internal sealed class IncomingLinksDebugView
{
    [Browse(RootHidden)]
    private readonly    IncomingLink[]    links;
    
    internal IncomingLinksDebugView(IncomingLinks incomingLinks) {
        links = incomingLinks.incomingLinks;
    }
}

