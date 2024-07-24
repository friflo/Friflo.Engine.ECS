// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class SchemaTypeUtils
{
    internal static int GetStructIndex(Type type)
    {
        var schema = EntityStoreBase.Static.EntitySchema;
        if (schema.ComponentTypeByType.TryGetValue(type, out var componentType)) {
            return componentType.StructIndex;
        }
        throw ComponentTypeException(type, nameof(IComponent));
    }
    
    internal static bool HasIndex(Type type)
    {
        var componentType = EntityStoreBase.Static.EntitySchema.ComponentTypeByType[type];
        return componentType.IndexType != null;
    }
    
    internal static bool IsRelation(Type type)
    {
        var componentType = EntityStoreBase.Static.EntitySchema.ComponentTypeByType[type];
        return componentType.RelationType != null;
    }
    
    internal static int GetTagIndex(Type type)
    {
        var schema = EntityStoreBase.Static.EntitySchema;
        if (schema.TagTypeByType.TryGetValue(type, out var tagType)) {
            return tagType.TagIndex;
        }
        throw ComponentTypeException(type, nameof(ITag));
    }
    
    internal static int GetScriptIndex(Type type)
    {
        var schema = EntityStoreBase.Static.EntitySchema;
        return schema.ScriptTypeByType[type].ScriptIndex;
    }
    
    internal static InvalidOperationException ComponentTypeException(Type type, string typeBase)
    {
        if (!type.IsGenericType) {
            return new InvalidOperationException($"{typeBase} type not found: {type}");
        }
        var sb = new StringBuilder();
        bool isFirst = true;
        foreach (var arg in type.GenericTypeArguments) {
            if (isFirst) {
                isFirst = false;
            } else {
                sb.Append(", ");
            }
            sb.Append("typeof(");
            sb.Append(arg.Name);
            sb.Append(')');
        }
        return new InvalidOperationException($"Missing attribute [GenericInstanceType(\"<key>\", {sb})] for generic {typeBase} type: {type}");
    }
}