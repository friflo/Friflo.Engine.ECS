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
                    public  readonly    Entity  Source;
    [Browse(Never)] private readonly    int     index;

                    public  override    string  ToString() => $"Source: {Source.Id}";

    internal EntityReference(Entity source, int index) {
        Source      = source;
        this.index  = index;
    }
    
    public TComponent Component {
        get {
            if (StructInfo<TComponent>.IsRelation) {
                var relations = Source.store.extension.relationsMap[StructInfo<TComponent>.Index];
                return relations.GetRelationAt<TComponent>(Source.Id, index);
            }
            return Source.GetComponent<TComponent>();
        }
    }
}

public readonly struct EntityReferences<T> : IReadOnlyList<EntityReference<T>>
    where T : struct, IComponent
{
#region properties
    public              int         Count       => Sources.Count;
    public              EntityStore Store       => Sources.store;
    public   override   string      ToString()  => $"EntityReference<{typeof(T).Name}>[{Sources.Count}]";
    #endregion
    
#region fields
    public   readonly   Entities    Sources;   //  8
    #endregion
    
#region general
    internal EntityReferences(in Entities sources) {
        Sources   = sources;
    }
    
    public EntityReference<T> this[int index] => new (Sources[index], index);
    
    public string Debug()
    {
        if (Count == 0) return "{ }";
        var sb = new StringBuilder();
        sb.Append("{ ");
        foreach (var entity in Sources) {
            if (sb.Length > 2) sb.Append(", ");
            sb.Append(entity.Id);
        }
        sb.Append(" }");
        return sb.ToString();
    }
    #endregion

    
#region IEnumerator
    public EntityReferenceEnumerator<T>                             GetEnumerator() => new EntityReferenceEnumerator<T> (Sources);
    
    // --- IEnumerable
    IEnumerator                                         IEnumerable.GetEnumerator() => new EntityReferenceEnumerator<T> (Sources);

    // --- IEnumerable<>
    IEnumerator<EntityReference<T>> IEnumerable<EntityReference<T>>.GetEnumerator() => new EntityReferenceEnumerator<T> (Sources);
    #endregion
}


public struct EntityReferenceEnumerator<T> : IEnumerator<EntityReference<T>>
    where T : struct, IComponent
{
    private readonly    Entities    sources;    // 16
    private             int         index;      //  4
    
    internal EntityReferenceEnumerator(in Entities sources) {
        this.sources    = sources;
        index           = -1;
    }
    
    // --- IEnumerator
    public          void         Reset()    => index = 0;

    readonly object IEnumerator.Current    => new EntityReference<T>(sources[index], index);

    public   EntityReference<T> Current    => new EntityReference<T>(sources[index], index);
    
    // --- IEnumerator
    public bool MoveNext()
    {
        if (index < sources.count - 1) {
            index++;
            return true;
        }
        return false;
    }
    
    public readonly void Dispose() { }
}

