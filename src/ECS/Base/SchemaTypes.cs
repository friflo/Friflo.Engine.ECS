// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Friflo.Engine.ECS.Index;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable RedundantJumpStatement
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal readonly struct AssemblyType
{
    internal readonly Type type;
    internal readonly int  assemblyIndex;
    
    internal AssemblyType(Type type, int  assemblyIndex) {
        this.type           = type;
        this.assemblyIndex  = assemblyIndex;
    }
}

internal sealed class SchemaTypes
{
    private  readonly   List<AssemblyType>  componentTypes  = new ();
    private  readonly   List<AssemblyType>  scriptTypes     = new ();
    private  readonly   List<AssemblyType>  tagTypes        = new ();
    
    internal readonly   List<ComponentType> components      = new ();
    internal readonly   List<ScriptType>    scripts         = new ();
    internal readonly   List<TagType>       tags            = new ();
    
    internal void AddSchemaType(Type type, int assemblyIndex)
    {
        if (type.IsValueType) {
            if (typeof(ITag).IsAssignableFrom(type)) {
                tagTypes      .Add(new AssemblyType(type, assemblyIndex));
                return;
            }
            if (typeof(IComponent).IsAssignableFrom(type)) {
                componentTypes.Add(new AssemblyType(type, assemblyIndex));
                return;
            }
        } else {
            if (type.IsSubclassOf(typeof(Script))) {
                scriptTypes   .Add(new AssemblyType(type, assemblyIndex));
                return;
            }
        }
    }

    internal EngineDependant[] CreateSchemaTypes(TypeStore typeStore, IList<Assembly> assemblies)
    {
        var assemblyCount = assemblies.Count;
        var engineTypes = new List<SchemaType>[assemblyCount];
        for (int n = 0; n < assemblyCount; n++) {
            engineTypes[n] = new List<SchemaType>();
        }
        foreach (var type in tagTypes) {
            var schemaType = CreateTagType(type.type);
            engineTypes[type.assemblyIndex].Add(schemaType);
        }
        foreach (var type in componentTypes) {
            var schemaType = CreateComponentType(type.type, typeStore);
            engineTypes[type.assemblyIndex].Add(schemaType);
        }
        foreach (var type in scriptTypes) {
            var schemaType = CreateScriptType(type.type, typeStore);
            engineTypes[type.assemblyIndex].Add(schemaType);
        }
        var dependants = new EngineDependant[assemblyCount];
        for (int n = 0; n < assemblyCount; n++) {
            dependants[n] = new EngineDependant(assemblies[n], engineTypes[n]);
        }
        return dependants;
    }

    private const BindingFlags Flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Not called for NativeAOT")]
    private SchemaType CreateTagType(Type type)
    {
        var tagIndex        = tags.Count + 1;
        var createParams    = new object[] { tagIndex };
        var method          = typeof(SchemaUtils).GetMethod(nameof(SchemaUtils.CreateTagType), Flags);
        var genericMethod   = method!.MakeGenericMethod(type);
        var tagType         = (TagType)genericMethod.Invoke(null, createParams);
        tags.Add(tagType);
        return tagType;
    }
        
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Not called for NativeAOT")]
    private SchemaType CreateComponentType(Type type, TypeStore typeStore)
    {
        var structIndex     = components.Count + 1;
        var indexType       = ComponentIndexUtils.GetIndexType(type);
        var createParams    = new object[] { typeStore, structIndex, indexType };
        var method          = typeof(SchemaUtils).GetMethod(nameof(SchemaUtils.CreateComponentType), Flags);
        var genericMethod   = method!.MakeGenericMethod(type);
        var componentType   = (ComponentType)genericMethod.Invoke(null, createParams);
        components.Add(componentType);
        return componentType;
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Not called for NativeAOT")]
    private SchemaType CreateScriptType(Type type, TypeStore typeStore)
    {
        var scriptIndex     = scripts.Count + 1;
        var createParams    = new object[] { typeStore, scriptIndex };
        var method          = typeof(SchemaUtils).GetMethod(nameof(SchemaUtils.CreateScriptType), Flags);
        var genericMethod   = method!.MakeGenericMethod(type);
        var scriptType      = (ScriptType)genericMethod.Invoke(null, createParams);
        scripts.Add(scriptType);
        return scriptType;
    }
}