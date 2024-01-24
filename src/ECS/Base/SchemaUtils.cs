// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class SchemaUtils
{
    internal static EntitySchema RegisterSchemaTypes(TypeStore typeStore)
    {
        var assemblyLoader  = new AssemblyLoader();
        var assemblies      = assemblyLoader.GetEngineDependants();
        
        var dependants  = assemblyLoader.dependants;
        foreach (var assembly in assemblies) {
            var types           = AssemblyLoader.GetComponentTypes(assembly);
            var schemaTypes     = new List<SchemaType>();
            foreach (var type in types) {
                var schemaType = CreateSchemaType(type, typeStore);
                schemaTypes.Add(schemaType);
            }
            dependants.Add(new EngineDependant (assembly, schemaTypes));
        }
        Console.WriteLine(assemblyLoader);
        
        var components  = new List<ComponentType>();
        var scripts     = new List<ScriptType>();
        var tags        = new List<TagType>();
        foreach (var dependant in dependants)
        {
            foreach (var type in dependant.Types)
            {
                switch (type.Kind) {
                    case SchemaTypeKind.Script:      scripts.   Add((ScriptType)    type);  break;
                    case SchemaTypeKind.Component:   components.Add((ComponentType) type);  break;
                    case SchemaTypeKind.Tag:         tags.      Add((TagType)       type);  break;
                }
            }
        }
        return new EntitySchema(dependants, components, scripts, tags);
    }
    
    internal static SchemaType CreateSchemaType(Type type, TypeStore typeStore)
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        
        if (type.IsValueType) {
            if (typeof(ITag).IsAssignableFrom(type))
            {
                var method          = typeof(SchemaUtils).GetMethod(nameof(CreateTagType), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var tagType         = (TagType)genericMethod.Invoke(null, null);
                return tagType;
            }
            if (typeof(IComponent).IsAssignableFrom(type))
            {
                var createParams    = new object[] { typeStore };
                var method          = typeof(SchemaUtils).GetMethod(nameof(CreateComponentType), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var componentType   = (ComponentType)genericMethod.Invoke(null, createParams);
                return componentType;
            }
        } else {
            /* foreach (var attr in type.CustomAttributes)
            {
                var attributeType = attr.AttributeType;
                if (attributeType == typeof(ScriptAttribute))
                {
                    var createParams    = new object[] { typeStore };
                    var method          = typeof(SchemaUtils).GetMethod(nameof(CreateScriptType), flags);
                    var genericMethod   = method!.MakeGenericMethod(type);
                    var scriptType      = (ScriptType)genericMethod.Invoke(null, createParams);
                    return scriptType;
                }
            } */
            if (type.IsClass && type.IsSubclassOf(typeof(Script)))
            {
                var createParams    = new object[] { typeStore };
                var method          = typeof(SchemaUtils).GetMethod(nameof(CreateScriptType), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var scriptType      = (ScriptType)genericMethod.Invoke(null, createParams);
                return scriptType;
            }
        }
        throw new InvalidOperationException($"Cannot create SchemaType for Type: {type}");
    }
    
    internal static ComponentType CreateComponentType<T>(TypeStore typeStore)
        where T : struct, IComponent
    {
        var structIndex     = StructHeap<T>.StructIndex;
        var componentKey    = GetComponentKey(typeof(T));
        var typeMapper      = typeStore.GetTypeMapper<T>();
        return new ComponentType<T>(componentKey, structIndex, typeMapper);
    }
    
    internal static ScriptType CreateScriptType<T>(TypeStore typeStore)
        where T : Script, new()
    {
        var scriptIndex = ClassType<T>.ScriptIndex;
        var scriptKey   = GetComponentKey(typeof(T));
        var typeMapper  = typeStore.GetTypeMapper<T>();
        return new ScriptType<T>(scriptKey, scriptIndex, typeMapper);
    }
    
    private static string GetComponentKey(Type type)
    {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(ComponentKeyAttribute)) {
                continue;
            }
            var arg     = attr.ConstructorArguments;
            return (string) arg[0].Value;
        }
        return type.Name;
    }
    
    internal static TagType CreateTagType<T>()
        where T : struct, ITag
    {
        var tagIndex    = TagType<T>.TagIndex;
        var tagName     = GetTagName(typeof(T));
        return new TagType(tagName, typeof(T), tagIndex);
    }
    
    private static string GetTagName(Type type)
    {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(TagNameAttribute)) {
                continue;
            }
            var arg     = attr.ConstructorArguments;
            return (string) arg[0].Value;
        }
        return type.Name;
    }    
}
