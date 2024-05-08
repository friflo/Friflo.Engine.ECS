// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class SchemaTypeUtils
{
    internal static int GetStructIndex(Type type)
    {
        var schema = EntityStoreBase.Static.EntitySchema;
        return schema.ComponentTypeByType[type].StructIndex;
    }
    
    internal static int GetTagIndex(Type type)
    {
        var schema = EntityStoreBase.Static.EntitySchema;
        return schema.TagTypeByType[type].TagIndex;
    }
    
    internal static int GetScriptIndex(Type type)
    {
        var schema = EntityStoreBase.Static.EntitySchema;
        return schema.ScriptTypeByType[type].ScriptIndex;
    }
}