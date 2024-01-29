// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal struct TypeFilter
{
    internal    int             index;
    internal    SchemaTypeKind  kind;
}

internal enum EventFilterAction
{
    Removed = 0,
    Added   = 1
}

internal struct EventFilters
{
    internal    TypeFilter[]        items;
    internal    int                 count;
    internal    EventFilterAction   action;

    public override string  ToString() => GetString();
    
    private string GetString()
    {
        if (count == 0) {
            return "[]";
        }
        var sb = new StringBuilder();
        if (action == EventFilterAction.Added) {
            sb.Append("added: ");
        } else {
            sb.Append("removed: ");
        }
        sb.Append('[');
        if (count > 0) {
            AppendString(sb);
            sb.Length -= 2;
        }
        sb.Append(']');
        return sb.ToString();
    }
    
    internal void AppendString(StringBuilder sb)
    {
        var schema  = EntityStoreBase.Static.EntitySchema;
        for (int n = 0; n < count; n++)
        {
            var filter = items[n];
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
