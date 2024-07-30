// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable UseCollectionExpression
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class SchemaUtils
{
    [ExcludeFromCodeCoverage]
    private static bool RegisterComponentTypesByReflection() {
        if (Platform.IsUnityRuntime) {
            return true;
        }
        return System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeCompiled;
    }
    
    [ExcludeFromCodeCoverage]
    internal static EntitySchema RegisterSchemaTypes(TypeStore typeStore)
    {
        if (!RegisterComponentTypesByReflection()) {
            return NativeAOT.GetSchema();
        }
        return RegisterTypes(typeStore);
    }
    
    private static EntitySchema RegisterTypes(TypeStore typeStore)
    {
        var assemblyLoader  = new AssemblyLoader();
        var assemblies      = assemblyLoader.GetEngineDependants();
        
        var schemaTypes     = new SchemaTypes();
        var types           = new List<AssemblyType>();
        for (int n = 0; n < assemblies.Length; n++) {
            var assembly = assemblies[n];
            AssemblyLoader.GetComponentTypes(assembly, n, types);
            foreach (var type in types) {
                schemaTypes.AddSchemaType(type);
            }
        }
        var dependants = schemaTypes.CreateSchemaTypes(typeStore, assemblies);
        foreach (var dependant in dependants) {
            assemblyLoader.dependants.Add(dependant);
        }
        Console.WriteLine(assemblyLoader);
        return new EntitySchema(dependants, schemaTypes);
    }
    
    internal static ComponentType CreateComponentType<T>(TypeStore typeStore, int structIndex, Type indexType, Type indexValueType, Type relationType, Type keyType)
        where T : struct, IComponent
    {
        string componentKey;
        var type = typeof(T);
        if (type.IsGenericType) {
            componentKey = GetGenericComponentKey(type);
        } else {
            componentKey = GetComponentKey(type);
        }
        return new ComponentType<T>(componentKey, structIndex, indexType, indexValueType, typeStore, relationType, keyType);
    }
    
    internal static ScriptType CreateScriptType<T>(TypeStore typeStore, int scriptIndex)
        where T : Script, new()
    {
        var scriptKey   = GetComponentKey(typeof(T));
        var isBlittable = SchemaType.GetBlittableType(typeof(T)) == BlittableType.Blittable;
        CloneScript cloneScript = null;
        if (isBlittable) {
            cloneScript = CreateCloneScriptDelegate();
        }
        return new ScriptType<T>(scriptKey, scriptIndex, typeStore, isBlittable, cloneScript);
    }
    
    private static CloneScript CreateCloneScriptDelegate()
    {
        var flags           = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var methodInfo      = typeof(object).GetMethod("MemberwiseClone", flags);
        // Console.WriteLine($"ScriptType - methodInfo  {methodInfo}");
        // Create a delegate representing an 'open instance method'.
        // An instance must be passed when the delegate is invoked.
        // See: https://learn.microsoft.com/en-us/dotnet/api/system.delegate.createdelegate
        var cloneDelegate   = Delegate.CreateDelegate(typeof(CloneScript), null, methodInfo!);
        return (CloneScript)cloneDelegate;
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
    
    internal static GenericInstanceType[] GetGenericInstanceTypes(Type type)
    {
        var list = new List<GenericInstanceType>();
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(GenericInstanceTypeAttribute)) {
                continue;
            }
            var args = attr.ConstructorArguments;
            GenericInstanceType.Add(list, args);
        }
        return list.ToArray();
    }
    
    internal static string GetGenericComponentKey(Type type)
    {
        var genericInstanceTypes    = GetGenericInstanceTypes(type);
        var findTypes               = type.GenericTypeArguments;
        foreach (var genericType in genericInstanceTypes) {
            if (findTypes.SequenceEqual(genericType.types)) {
                return genericType.key;
            }
        }
        return null; // cannot be reached - only in internal tests
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
        string tagName;
        var type = typeof(T);
        if (type.IsGenericType) {
            tagName = GetGenericComponentKey(type);            
        } else {
            tagName = GetTagName(type);
        }
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

internal readonly struct GenericInstanceType
{
    internal readonly string key;
    internal readonly Type[] types;
    
    private GenericInstanceType(string key, Type[] types) {
        this.key    = key;
        this.types  = types;
    }
    
    internal static void Add(List<GenericInstanceType> list, IList<CustomAttributeTypedArgument> args)
    {
        switch (args.Count) {
            case 2: list.Add(new GenericInstanceType((string)args[0].Value, new [] { (Type)args[1].Value                                           })); break;
            case 3: list.Add(new GenericInstanceType((string)args[0].Value, new [] { (Type)args[1].Value, (Type)args[2].Value                      })); break;
            case 4: list.Add(new GenericInstanceType((string)args[0].Value, new [] { (Type)args[1].Value, (Type)args[2].Value, (Type)args[3].Value })); break;
        }
    }
} 
