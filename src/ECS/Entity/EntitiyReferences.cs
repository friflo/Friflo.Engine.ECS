// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct EntityReference<TComponent>
    where TComponent : struct, IComponent
{
                    public  readonly    Entity  Entity;
    [Browse(Never)] private readonly    int     index;

                    public  override    string  ToString() => $"Source: {Entity.Id}";

    internal EntityReference(Entity entity, int index) {
        Entity      = entity;
        this.index  = index;
    }
    
    public TComponent Component {
        get {
            if (StructInfo<TComponent>.IsRelation) {
                var relations = Entity.store.extension.relationsMap[StructInfo<TComponent>.Index];
                return relations.GetRelationAt<TComponent>(Entity.Id, index);
            }
            return Entity.GetComponent<TComponent>();
        }
    }
}

public readonly struct EntityReferences<T> : IReadOnlyList<EntityReference<T>>
    where T : struct, IComponent
{
#region properties
    public              int         Count       => Entities.Count;
    public              EntityStore Store       => Entities.store;
    public   override   string      ToString()  => $"EntityReference<{typeof(T).Name}>[{Entities.Count}]";
    #endregion
    
#region fields
    public   readonly   Entities    Entities;   //  8
    #endregion
    
#region general
    internal EntityReferences(in Entities entities) {
        Entities   = entities;
    }
    
    public EntityReference<T> this[int index] => new (Entities[index], index);
    
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
    public EntityReferenceEnumerator<T>                             GetEnumerator() => new EntityReferenceEnumerator<T> (Entities);
    
    // --- IEnumerable
    IEnumerator                                         IEnumerable.GetEnumerator() => new EntityReferenceEnumerator<T> (Entities);

    // --- IEnumerable<>
    IEnumerator<EntityReference<T>> IEnumerable<EntityReference<T>>.GetEnumerator() => new EntityReferenceEnumerator<T> (Entities);
    #endregion
}


public struct EntityReferenceEnumerator<T> : IEnumerator<EntityReference<T>>
    where T : struct, IComponent
{
    private readonly    Entities    entities;   // 16
    private             int         index;      //  4
    
    internal EntityReferenceEnumerator(in Entities entities) {
        this.entities    = entities;
        index           = -1;
    }
    
    // --- IEnumerator
    public          void         Reset()    => index = 0;

    readonly object IEnumerator.Current    => new EntityReference<T>(entities[index], index);

    public   EntityReference<T> Current    => new EntityReference<T>(entities[index], index);
    
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

