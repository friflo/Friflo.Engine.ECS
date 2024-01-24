// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable UseCollectionExpression
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal class SchemaTypes
{
    internal readonly   List<ComponentType> components  = new ();
    internal readonly   List<ScriptType>    scripts     = new ();
    internal readonly   List<TagType>       tags        = new ();
}

internal static class SchemaUtils
{
    internal static EntitySchema RegisterSchemaTypes(TypeStore typeStore)
    {
        var assemblyLoader  = new AssemblyLoader();
        var assemblies      = assemblyLoader.GetEngineDependants();
        
        var dependants  = assemblyLoader.dependants;
        var schemaTypes = new SchemaTypes();
        foreach (var assembly in assemblies) {
            var types           = AssemblyLoader.GetComponentTypes(assembly);
            var engineTypes     = new List<SchemaType>();
            foreach (var type in types) {
                var schemaType = CreateSchemaType(type, typeStore, schemaTypes);
                engineTypes.Add(schemaType);
            }
            dependants.Add(new EngineDependant (assembly, engineTypes));
        }
        Console.WriteLine(assemblyLoader);
        return new EntitySchema(dependants, schemaTypes);
    }
    
    internal static SchemaType CreateSchemaType(Type type, TypeStore typeStore, SchemaTypes schemaTypes)
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        
        if (type.IsValueType) {
            if (typeof(ITag).IsAssignableFrom(type))
            {
                var tagIndex        = schemaTypes.tags.Count + 1;
                var createParams    = new object[] { tagIndex };
                var method          = typeof(SchemaUtils).GetMethod(nameof(CreateTagType), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var tagType         = (TagType)genericMethod.Invoke(null, createParams);
                schemaTypes.tags.Add(tagType);
                return tagType;
            }
            if (typeof(IComponent).IsAssignableFrom(type))
            {
                var structIndex     = schemaTypes.components.Count + 1;
                var createParams    = new object[] { typeStore, structIndex };
                var method          = typeof(SchemaUtils).GetMethod(nameof(CreateComponentType), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var componentType   = (ComponentType)genericMethod.Invoke(null, createParams);
                schemaTypes.components.Add(componentType);
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
                var scriptIndex     = schemaTypes.scripts.Count + 1;
                var createParams    = new object[] { typeStore, scriptIndex };
                var method          = typeof(SchemaUtils).GetMethod(nameof(CreateScriptType), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var scriptType      = (ScriptType)genericMethod.Invoke(null, createParams);
                schemaTypes.scripts.Add(scriptType);
                return scriptType;
            }
        }
        throw new InvalidOperationException($"Cannot create SchemaType for Type: {type}");
    }
    
    internal static ComponentType CreateComponentType<T>(TypeStore typeStore, int structIndex)
        where T : struct, IComponent
    {
        var componentKey    = GetComponentKey(typeof(T));
        var typeMapper      = typeStore.GetTypeMapper<T>();
        return new ComponentType<T>(componentKey, structIndex, typeMapper);
    }
    
    internal static ScriptType CreateScriptType<T>(TypeStore typeStore, int scriptIndex)
        where T : Script, new()
    {
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
    
    internal static TagType CreateTagType<T>(int tagIndex)
        where T : struct, ITag
    {
        var tagName = GetTagName(typeof(T));
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
