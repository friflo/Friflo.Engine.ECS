// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable UseCollectionExpression
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal sealed class SchemaTypes
{
    internal readonly   List<ComponentType> components  = new ();
    internal readonly   List<ScriptType>    scripts     = new ();
    internal readonly   List<TagType>       tags        = new ();
}

internal static class SchemaUtils
{
    internal static EntitySchema RegisterSchemaTypes(TypeStore typeStore)
    {
        if (!System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeCompiled) {
            return NativeAOT.GetSchema();
        }
        var assemblyLoader  = new AssemblyLoader();
        var assemblies      = assemblyLoader.GetEngineDependants();
        
        var dependants  = assemblyLoader.dependants;
        var schemaTypes = new SchemaTypes();
        var types       = new List<Type>();
        foreach (var assembly in assemblies) {
            AssemblyLoader.GetComponentTypes(assembly, types);
            var engineTypes = new List<SchemaType>();
            foreach (var type in types) {
                var schemaType = CreateSchemaType(type, typeStore, schemaTypes);
                engineTypes.Add(schemaType);
            }
            dependants.Add(new EngineDependant (assembly, engineTypes));
        }
        Console.WriteLine(assemblyLoader);
        return new EntitySchema(dependants, schemaTypes);
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Not called for NativeAOT")]
    internal static SchemaType CreateSchemaType(Type type, TypeStore typeStore, SchemaTypes schemaTypes)
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        
        if (type.IsValueType) {
            if (typeof(ITag).IsAssignableFrom(type))
            {
                // type: ITag
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
                // type: IComponent
                var structIndex     = schemaTypes.components.Count + 1;
                var createParams    = new object[] { typeStore, structIndex };
                var method          = typeof(SchemaUtils).GetMethod(nameof(CreateComponentType), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var componentType   = (ComponentType)genericMethod.Invoke(null, createParams);
                schemaTypes.components.Add(componentType);
                return componentType;
            }
        } else {
            if (type.IsSubclassOf(typeof(Script)))
            {
                // type: Script
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
        var componentKey = GetComponentKey(typeof(T));
        return new ComponentType<T>(componentKey, structIndex, typeStore);
    }
    
    internal static ScriptType CreateScriptType<T>(TypeStore typeStore, int scriptIndex)
        where T : Script, new()
    {
        var scriptKey = GetComponentKey(typeof(T));
        return new ScriptType<T>(scriptKey, scriptIndex, typeStore);
    }
    
    private static string GetComponentKey(Type type)
    {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(ComponentKeyAttribute)) {
                continue;
            }
            var arg = attr.ConstructorArguments;
            return (string) arg[0].Value;
        }
        return type.Name;
    }
    
    internal static void GetComponentSymbol(Type type, out string name, out SymbolColor? color)
    {
        name    = null;
        color   = default;
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(ComponentSymbolAttribute)) {
                continue;
            }
            var arg = attr.ConstructorArguments;
            if (arg.Count > 0) {
                name    = (string)arg[0].Value;
            }
            if (arg.Count > 1) {
                color = ParseColor((string)arg[1].Value);
            }
        }
        name = GetSymbolName(name, type);
    }
    
    private static string GetSymbolName(string name, Type type)
    {
        if (name == null) {
            return type.Name.Substring(0, 1);
        }
        name = name.Substring(0, Math.Min(3, name.Length));
        name = name.Trim();
        if (name.Length == 0) {
            return type.Name.Substring(0, 1);
        }
        return name;
    } 
    
    private static SymbolColor? ParseColor(string color)
    {
        if (color == null) {
            return null;
        }
        var colors = color.Split(',');
        if (colors.Length != 3) {
            return default;
        }
        if (byte.TryParse(colors[0], out byte r) &&
            byte.TryParse(colors[1], out byte g) &&
            byte.TryParse(colors[2], out byte b))
        {
            return new SymbolColor(r, g, b);
        }
        return null;
    }
    
    /// <remarks>
    /// <see cref="TagInfo{T}.Index"/> must be assigned here.<br/>
    /// Unity initializes static fields of generic types already when creating a instance of that type.  
    /// </remarks>
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
