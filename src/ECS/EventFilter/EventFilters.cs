// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal struct TypeFilter
{
    internal    int                 index;      //  4
    internal    SchemaTypeKind      kind;       //  1
    internal    EntityEventAction   action;     //  1
}


internal struct EventFilters
{
    internal    TypeFilter[]        items;
    internal    int                 count;

    public override string  ToString() => GetString();
    
    private string GetString()
    {
        if (count == 0) {
            return "[]";
        }
        var sb = new StringBuilder();
        sb.Append("added: [");
        var start = sb.Length;
        AppendString(sb, EntityEventAction.Added);
        if (sb.Length > start) sb.Length -= 2;
        sb.Append("]  ");
        
        sb.Append("removed: [");
        start = sb.Length;
        AppendString(sb, EntityEventAction.Removed);
        if (sb.Length > start) sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }
    
    internal void AppendString(StringBuilder sb, EntityEventAction action)
    {
        var schema  = EntityStoreBase.Static.EntitySchema;
        for (int n = 0; n < count; n++)
        {
            TypeFilter filter = items[n];
            if (filter.action != action) {
                continue;
            }
            if (filter.kind == SchemaTypeKind.Component) {
                var type = schema.components[filter.index];
                sb.Append(type.Name);
            } else {
                var type = schema.tags[filter.index];
                sb.Append('#');
                sb.Append(type.Name);
            }
            sb.Append(", ");
        }
    }
}
