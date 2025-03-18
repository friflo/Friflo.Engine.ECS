using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Friflo.Engine.ECS;

/// <summary>
/// Specify the sort order in <see cref="EntityList.SortByComponentField{TComponent,TField}"/>.
/// </summary>
public enum SortOrder
{
    /// <summary>
    /// Leave entity order unchanged.<br/>
    /// A custom sort can be applied to the <see cref="ComponentField{TField}"/> array returned
    /// by <see cref="EntityList.SortByComponentField{TComponent,TField}"/>.
    /// </summary> 
    None,
    /// Sort entities in ascending order
    Ascending,
    /// Sort entities in descending order
    Descending
}


/// <summary>
/// Contains an entity and its component field/property value returned by <see cref="EntityList.SortByComponentField{TComponent,TField}"/>. 
/// </summary>
public struct ComponentField<TField>
{
    /// Entity id
    public  RawEntity   entityId;
    /// Is 1 if the entity has a component field. Otherwise, 0.
    public  byte        hasField;
    /// The component field value if <see cref="hasField"/> == 1.
    public  TField      field;

    public override string ToString() {
        if (hasField == 0) {
            return $"id: {entityId.Id}, value: null";    
        }
        return $"id: {entityId.Id}, value: {field}";
    }
    
#if !NET5_0_OR_GREATER
    private class GenericComparerAsc : IComparer<ComponentField<TField>>
    {
        public int Compare(ComponentField<TField> e1, ComponentField<TField> e2) {
            if (e1.hasField == 1 && e2.hasField == 1) {
                return Comparer<TField>.Default.Compare(e1.field, e2.field);
            }
            return e1.hasField - e2.hasField;
        }
    }
    
    private class GenericComparerDesc : IComparer<ComponentField<TField>>
    {
        public int Compare(ComponentField<TField> e1, ComponentField<TField> e2) {
            if (e1.hasField == 1 && e2.hasField == 1) {
                return Comparer<TField>.Default.Compare(e2.field, e1.field);
            }
            return e2.hasField - e1.hasField;
        }
    }

    private static readonly GenericComparerAsc  ComparerAsc  = new ();
    private static readonly GenericComparerDesc ComparerDesc = new ();
#endif

    private static readonly Comparison<ComponentField<TField>> ComparisonAsc = (e1, e2) => {
        if (e1.hasField == 1 && e2.hasField == 1) {
            return Comparer<TField>.Default.Compare(e1.field, e2.field);
        }
        return e1.hasField - e2.hasField;
    };
    
    private static readonly Comparison<ComponentField<TField>> ComparisonDesc = (e1, e2) => {
        if (e1.hasField == 1 && e2.hasField == 1) {
            return Comparer<TField>.Default.Compare(e2.field, e1.field);
        }
        return e2.hasField - e1.hasField;
    };

    
    internal static ComponentField<TField>[] Sort<TComponent>(EntityList entities, string memberName, SortOrder sortOrder, ComponentField<TField>[] fields)
        where TComponent : struct, IComponent
    {
        var structIndex     = StructInfo<TComponent>.Index;
        var componentType   = EntityStoreBase.Static.EntitySchema.components[structIndex];
        var count           = entities.Count;
        if (fields == null || fields.Length < count) {
            fields = new ComponentField<TField>[count];
        }
        var nodes       = entities.entityStore.nodes;
        var ids         = entities.ids;
        var getter      = (MemberGetter<TComponent, TField>)ComponentFieldInfo.Get(componentType, memberName).getter;
        
        for (int index = 0; index < count; index++)
        {
            var id          = ids[index];
            ref var node    = ref nodes[id.Id];
            var heap        = node.archetype?.heapMap[structIndex];
            ref var entry   = ref fields[index];
            entry.entityId  = id;
            if (heap == null) {
                entry.hasField  = 0;
                entry.field     = default;
                continue;
            }
            entry.field     = getter(((StructHeap<TComponent>)heap).components[node.compIndex]);
            entry.hasField  = 1;
        }

        switch (sortOrder) {
            case SortOrder.None:
                return fields;
            case SortOrder.Ascending:
#if NET5_0_OR_GREATER
                Span<ComponentField<TField>> span = new Span<ComponentField<TField>>(fields, 0, count);
                span.Sort(ComparisonAsc);
#else
                Array.Sort(fields, 0, count, ComparerAsc);  // allocates a single System.Comparision<ComponentField<>> instance
#endif
                break;
            case SortOrder.Descending:
#if NET5_0_OR_GREATER
                span = new Span<ComponentField<TField>>(fields, 0, count);
                span.Sort(ComparisonDesc);
#else
                Array.Sort(fields, 0, count, ComparerDesc);  // allocates a single System.Comparision<ComponentField<>> instance
#endif
                break;
        }
        for (int n = 0; n < count; n++) {
            ids[n] = fields[n].entityId;
        }
        return fields;
    }
}
