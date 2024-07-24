// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Friflo.Engine.ECS.Relations;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct EntityLink<TComponent>
    where TComponent : struct, IComponent
{
#region properties
    public              Entity          Target      => new Entity(Entity.store, target);
    public  override    string          ToString()  => $"Entity: {Entity.Id} -> Target: {target}";
    #endregion

#region fields
                    public  readonly    Entity          Entity;     // 16
    [Browse(Never)] private readonly    EntityRelations relations;  //  8
    [Browse(Never)] private readonly    int             target;     //  4
    #endregion

 
    internal EntityLink(int target, in Entity entity, EntityRelations relations) {
        this.target     = target;
        Entity          = entity;
        this.relations  = relations;
    }
    
    public ref TComponent Component {
        get {
            if (relations != null) {
                return ref relations.GetEntityRelation<TComponent>(Entity.Id, target);
            }
            return ref Entity.GetComponent<TComponent>();
        }
    }
}

[DebuggerTypeProxy(typeof(EntityLinksDebugView<>))]
public readonly struct EntityLinks<T> : IReadOnlyList<EntityLink<T>>
    where T : struct, IComponent
{
#region properties
    public              int         Count       => Entities.Count;
    public              EntityStore Store       => Entities.store;
    public   override   string      ToString()  => $"EntityLinks<{typeof(T).Name}>[{Entities.Count}]";
    #endregion
    
#region fields
                    public   readonly   Entities            Entities;   // 16
    [Browse(Never)] internal readonly   int                 target;     //  4
    [Browse(Never)] internal readonly   EntityRelations     relations;  //  8
    #endregion
    
#region general
    internal EntityLinks(in Entity target, in Entities entities, EntityRelations relations) {
        this.target     = target.Id;
        Entities        = entities;
        this.relations  = relations;
    }
    
    public EntityLink<T> this[int index] => new (target, Entities[index], relations);
    
    /// <summary>
    /// Return the entity ids as a string.<br/>E.g <c>"{ 1, 3, 7 }"</c>
    /// </summary>
    public string Debug()
    {
        if (Count == 0) return "{ }";
        var sb = new StringBuilder();
        sb.Append("{ ");
        foreach (var entity in Entities) {
            if (sb.Length > 2) sb.Append(", ");
            sb.Append(entity.Id);
        }
        sb.Append(" }");
        return sb.ToString();
    }
    #endregion

    
#region IEnumerator
    public EntityLinkEnumerator<T>                         GetEnumerator() => new EntityLinkEnumerator<T> (this);
    
    // --- IEnumerable
    IEnumerator                                IEnumerable.GetEnumerator() => new EntityLinkEnumerator<T> (this);

    // --- IEnumerable<>
    IEnumerator<EntityLink<T>>  IEnumerable<EntityLink<T>>.GetEnumerator() => new EntityLinkEnumerator<T> (this);
    #endregion
}


public struct EntityLinkEnumerator<T> : IEnumerator<EntityLink<T>>
    where T : struct, IComponent
{
    private readonly    int             target;     //  4
    private readonly    Entities        entities;   // 16
    private readonly    EntityRelations relations;  //  8
    private             int             index;      //  4
    
    internal EntityLinkEnumerator(in EntityLinks<T> entityLinks) {
        target      = entityLinks.target;
        entities    = entityLinks.Entities;
        relations   = entityLinks.relations;
        index       = -1;
    }
    
    // --- IEnumerator
    public          void         Reset()    => index = -1;

    readonly object IEnumerator.Current    => new EntityLink<T>(target, entities[index], relations);

    public   EntityLink<T>      Current    => new EntityLink<T>(target, entities[index], relations);
    
    // --- IEnumerator
    public bool MoveNext()
    {
        if (index < entities.count - 1) {
            index++;
            return true;
        }
        return false;
    }
    
    public readonly void Dispose() { }
}

internal sealed class EntityLinksDebugView<T> where T : struct, IComponent
{
    [Browse(RootHidden)]
    internal            EntityLink<T>[]    Entities => entityLinks.ToArray();
    
    private readonly    EntityLinks<T>     entityLinks;
    
    internal EntityLinksDebugView(EntityLinks<T> entityLinks) {
        this.entityLinks = entityLinks;
    }
}

