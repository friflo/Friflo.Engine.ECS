using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;


namespace Friflo.Engine.ECS;

public enum SortOrder
{
    None,
    Ascending,
    Descending
}

internal static class ComponentField<TComponent, TField>  where TComponent : struct
{
    private static readonly Dictionary<string, MemberGetter<TComponent,TField>> GetterMap = new();   
    
    internal static MemberGetter<TComponent,TField> GetGetter(string memberName)
    {
        if (GetterMap.TryGetValue(memberName, out var getter)) {
            return getter;
        }
        var arg         = Expression.Parameter(typeof(TComponent), "component");
        var expr        = Expression.PropertyOrField(arg, memberName);
        var compiled    = Expression.Lambda<MemberGetter<TComponent, TField>>(expr, arg).Compile();
        GetterMap.Add(memberName, compiled);
        return compiled;
    }
}

internal delegate TField MemberGetter<in TComponent, out TField> (TComponent component) where TComponent : struct;


internal class GenericComparerAsc<T> : IComparer<ComponentField<T>>
{
    public int Compare(ComponentField<T> e1, ComponentField<T> e2) {
        var hasValueDiff = e1.hasValue - e2.hasValue;
        return hasValueDiff != 0 ? hasValueDiff : Comparer<T>.Default.Compare(e1.value, e2.value);
    }
}
    
internal class GenericComparerDesc<T> : IComparer<ComponentField<T>>
{
    public int Compare(ComponentField<T> e1, ComponentField<T> e2) {
        var hasValueDiff = e2.hasValue - e1.hasValue;
        return hasValueDiff != 0 ? hasValueDiff : Comparer<T>.Default.Compare(e2.value, e1.value);
    }
}

internal struct ComponentField<TField>
{
    private     int     id;
    internal    byte    hasValue;
    internal    TField  value;
    
    private static          ComponentField<TField>[]    SortEntriesArray = [];
    private static readonly GenericComparerAsc<TField>  ComparerAsc  = new GenericComparerAsc<TField>();
    private static readonly GenericComparerDesc<TField> ComparerDesc = new GenericComparerDesc<TField>();
    
    internal static void Sort<TComponent>(EntityList  entities, string memberName, SortOrder sortOrder)
        where TComponent : struct, IComponent
    {
        var structIndex = StructInfo<TComponent>.Index;
        var count       = entities.Count;
        if (SortEntriesArray.Length < count) {
            SortEntriesArray = new ComponentField<TField>[count];
        }
        var entries = SortEntriesArray;
        var nodes   = entities.entityStore.nodes;
        var ids     = entities.Ids;
        var getter  = ComponentField<TComponent, TField>.GetGetter(memberName);
        int index   = 0;
        
        foreach (var id in ids)
        {
            ref var node    = ref nodes[id];
            var heap        = node.archetype?.heapMap[structIndex];
            ref var entry   = ref entries[index++];
            entry.id        = id;
            if (heap == null) {
                entry.hasValue = 0;
                continue;
            }
            ref var component = ref ((StructHeap<TComponent>)heap).components[node.compIndex];
            entry.value     = getter(component);
            entry.hasValue  = 1;
        }

        switch (sortOrder) {
            case SortOrder.None:
                return;
            case SortOrder.Ascending:
                Array.Sort(entries, 0, count, ComparerAsc);
                break;
            case SortOrder.Descending:
                Array.Sort(entries, 0, count, ComparerDesc);
                break;
        }
        entities.Clear();
        for (int n = 0; n < count; n++) {
            entities.Add(entries[n].id);
        }
    }
}
