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

public readonly struct EntityReference<TComponent>
    where TComponent : struct, IComponent
{
                    public              Entity          Target      => new Entity(Entity.store, target);
    
                    public  readonly    Entity          Entity;     // 16
    [Browse(Never)] private readonly    EntityRelations relations;  //  8
    [Browse(Never)] private readonly    int             target;     //  4

                    public  override    string          ToString()  => $"entity: {Entity.Id}";

    internal EntityReference(int target, in Entity entity, EntityRelations relations) {
        this.target     = target;
        Entity          = entity;
        this.relations  = relations;
    }
    
    public TComponent Component {
        get {
            if (relations != null) {
                return relations.GetEntityRelation<TComponent>(Entity.Id, target);
            }
            return Entity.GetComponent<TComponent>();
        }
    }
}

[DebuggerTypeProxy(typeof(EntityReferencesDebugView<>))]
public readonly struct EntityReferences<T> : IReadOnlyList<EntityReference<T>>
    where T : struct, IComponent
{
#region properties
    public              int         Count       => Entities.Count;
    public              Entity      Target      => new Entity(Entities.store, target);
    public              EntityStore Store       => Entities.store;
    public   override   string      ToString()  => $"EntityReference<{typeof(T).Name}>[{Entities.Count}]";
    #endregion
    
#region fields
                    public   readonly   Entities            Entities;   // 16
    [Browse(Never)] internal readonly   int                 target;     //  4
    [Browse(Never)] internal readonly   EntityRelations     relations;  //  8
    #endregion
    
#region general
    internal EntityReferences(in Entity target, in Entities entities, EntityRelations relations) {
        this.target     = target.Id;
        Entities        = entities;
        this.relations  = relations;
    }
    
    public EntityReference<T> this[int index] => new (target, Entities[index], relations);
    
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
    public EntityReferenceEnumerator<T>                             GetEnumerator() => new EntityReferenceEnumerator<T> (this);
    
    // --- IEnumerable
    IEnumerator                                         IEnumerable.GetEnumerator() => new EntityReferenceEnumerator<T> (this);

    // --- IEnumerable<>
    IEnumerator<EntityReference<T>> IEnumerable<EntityReference<T>>.GetEnumerator() => new EntityReferenceEnumerator<T> (this);
    #endregion
}


public struct EntityReferenceEnumerator<T> : IEnumerator<EntityReference<T>>
    where T : struct, IComponent
{
    private readonly    int             target;     //  4
    private readonly    Entities        entities;   // 16
    private readonly    EntityRelations relations;  //  8
    private             int             index;      //  4
    
    internal EntityReferenceEnumerator(in EntityReferences<T> references) {
        target      = references.target;
        entities    = references.Entities;
        relations   = references.relations;
        index       = -1;
    }
    
    // --- IEnumerator
    public          void         Reset()    => index = -1;

    readonly object IEnumerator.Current    => new EntityReference<T>(target, entities[index], relations);

    public   EntityReference<T> Current    => new EntityReference<T>(target, entities[index], relations);
    
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

internal sealed class EntityReferencesDebugView<T> where T : struct, IComponent
{
    [Browse(RootHidden)]
    internal            EntityReference<T>[]    Entities => references.ToArray();
    
    private readonly    EntityReferences<T>     references;
    
    internal EntityReferencesDebugView(EntityReferences<T> references) {
        this.references = references;
    }
}

