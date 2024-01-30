// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal struct TypeFilter
{
    internal    int                 index;      //  4
    internal    SchemaTypeKind      kind;       //  1
    internal    EntityEventAction   action;     //  1
}

public struct EntityEvent {
    public      int                 Id;         //  4
    public      EntityEventAction   Action;     //  1
    public      byte                TypeIndex;  //  1
    public      SchemaTypeKind      Kind;       //  1   - used only for ToString()

    public override string          ToString() => GetString();
    
    private string GetString()
    {
        var schema = EntityStoreBase.Static.EntitySchema;
        switch (Kind) {
            case SchemaTypeKind.Component:
                return $"id: {Id} - {Action} [{schema.components[TypeIndex].Name}]";
            case SchemaTypeKind.Tag:
                return $"id: {Id} - {Action} [#{schema.tags[TypeIndex].Name}]";
        }
        throw new InvalidOperationException("unexpected kind");
    }
}

public enum EntityEventAction : byte
{
    Removed = 0,
    Added   = 1
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
