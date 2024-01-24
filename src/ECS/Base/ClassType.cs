// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class ClassType<T>
    where T : Script
{
    internal static readonly    int     ScriptIndex  = ClassUtils.NewClassIndex(typeof(T));
}

internal static class ClassUtils
{
//  private  const      int     MissingAttribute    = 0;
    
    internal static int NewClassIndex(Type type)
    {
        var schema = EntityStore.GetEntitySchema();
        return schema.scriptTypeByType[type].ScriptIndex;
    }
}