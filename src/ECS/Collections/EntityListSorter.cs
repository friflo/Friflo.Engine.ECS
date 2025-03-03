using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Friflo.Engine.ECS;

public enum SortOrder
{
    None,
    Ascending,
    Descending
}

internal static class TypeMember<TComponent, TField>
{
    private static readonly Dictionary<string, MemberGetter<TComponent,TField>> GetterMap = new();   
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Not called for NativeAOT")]
    internal static MemberGetter<TComponent,TField> Getter(string memberName)
    {
        if (GetterMap.TryGetValue(memberName, out var getter)) {
            return getter;
        }
        var arg         = Expression.Parameter(typeof(TComponent), "component"); // "component" parameter name in MemberGetter<,>
        var expr        = Expression.PropertyOrField(arg, memberName);
        var compiled    = Expression.Lambda<MemberGetter<TComponent, TField>>(expr, arg).Compile();
        GetterMap.Add(memberName, compiled);
        return compiled;
    }
}

internal delegate TField MemberGetter<in TComponent, out TField> (TComponent component);


internal class GenericComparerAsc<TField> : IComparer<ComponentField<TField>>
    where TField : IComparable<TField>
{
    public int Compare(ComponentField<TField> e1, ComponentField<TField> e2) {
        var hasValueDiff = e1.hasField - e2.hasField;
        return hasValueDiff != 0 ? hasValueDiff : Comparer<TField>.Default.Compare(e1.field, e2.field);
    }
}
    
internal class GenericComparerDesc<TField> : IComparer<ComponentField<TField>>
    where TField : IComparable<TField>
{
    public int Compare(ComponentField<TField> e1, ComponentField<TField> e2) {
        var hasValueDiff = e2.hasField - e1.hasField;
        return hasValueDiff != 0 ? hasValueDiff : Comparer<TField>.Default.Compare(e2.field, e1.field);
    }
}

public struct ComponentField<TField> where TField : IComparable<TField>
{
    public  int     entityId;
    public  byte    hasField;
    public  TField  field;

    public override string ToString() {
        if (hasField == 0) {
            return $"id: {entityId}, value: null";    
        }
        return $"id: {entityId}, value: {field}";
    }

    private static readonly GenericComparerAsc<TField>  ComparerAsc  = new GenericComparerAsc<TField>();
    private static readonly GenericComparerDesc<TField> ComparerDesc = new GenericComparerDesc<TField>();
    
    internal static ComponentField<TField>[] Sort<TComponent>(EntityList  entities, string memberName, SortOrder sortOrder, ComponentField<TField>[] fields)
        where TComponent : struct, IComponent
    {
        var structIndex = StructInfo<TComponent>.Index;
        var count       = entities.Count;
        if (fields == null || fields.Length < count) {
            fields = new ComponentField<TField>[count];
        }
        var nodes   = entities.entityStore.nodes;
        var ids     = entities.ids;
        var getter  = TypeMember<TComponent, TField>.Getter(memberName);
        
        for (int index = 0; index < count; index++)
        {
            var id          = ids[index];
            ref var node    = ref nodes[id];
            var heap        = node.archetype?.heapMap[structIndex];
            ref var entry   = ref fields[index];
            entry.entityId  = id;
            if (heap == null) {
                entry.hasField = 0;
                continue;
            }
            ref var component = ref ((StructHeap<TComponent>)heap).components[node.compIndex];
            entry.field     = getter(component);
            entry.hasField  = 1;
        }

        switch (sortOrder) {
            case SortOrder.None:
                return fields;
            case SortOrder.Ascending:
                Array.Sort(fields, 0, count, ComparerAsc);
                break;
            case SortOrder.Descending:
                Array.Sort(fields, 0, count, ComparerDesc);
                break;
        }
        for (int n = 0; n < count; n++) {
            ids[n] = fields[n].entityId;
        }
        return fields;
    }
}
