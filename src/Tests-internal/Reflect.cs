// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Friflo.Engine.ECS;

namespace Internal {

public static class Reflect
{
    [ExcludeFromCodeCoverage]
    public static Type EcsType(string name) {
        return typeof(EntityStoreBase).Assembly.GetType(name);
    }
    
    [ExcludeFromCodeCoverage]
    public static object GetInternalField(this object obj, string name) {
        var type    = obj.GetType();
        var field   = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null) {
            throw new InvalidOperationException($"field not found. type: {type}, name: {name}");
        }
        return field.GetValue(obj);
    }
    
    [ExcludeFromCodeCoverage]
    public static void SetInternalField(this object obj, string name, object value) {
        var type    = obj.GetType();
        var field   = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null) {
            throw new InvalidOperationException($"field not found. type: {type}, name: {name}");
        }
        field.SetValue(obj, value);
    }
    
    [ExcludeFromCodeCoverage]
    public static object InvokeInternalMethod(this object obj, string name, object[] parameters) {
        var type    = obj.GetType();
        var method  = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null) {
            throw new InvalidOperationException($"method not found. type: {type}, name: {name}");
        }
        try {
            return method.Invoke(obj, parameters);
        } catch (TargetInvocationException e) {
            var inner = e.InnerException;
            if (inner != null) {
                throw inner;
            }
            throw;
        }
    } 
    
    [ExcludeFromCodeCoverage]
    public static object InvokeConstructor<T>(object[] parameters) {
        return InvokeConstructor(typeof(T), parameters);        
    }
    
    [ExcludeFromCodeCoverage]
    private static object InvokeConstructor(this Type type, object[] parameters)
    {
        if (parameters == null) {
            return Activator.CreateInstance(type);
        }
        var constructor = FindConstructor(type, parameters);
        if (constructor == null) {
            throw new InvalidOperationException($"constructor not found. type: {type.Namespace}.{type.Name}");
        }
        try {
            return constructor.Invoke(parameters);
        }
        catch (TargetInvocationException e)
        {
            var inner = e.InnerException;
            if (inner != null) {
                throw inner;
            }
            throw;
        }
    }
    
    [ExcludeFromCodeCoverage]
    private static  ConstructorInfo FindConstructor(Type type, object[] parameters)
    {
        var constructors    = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
        var paramLength     = parameters.Length;
        var paramTypes      = new Type[paramLength];
        var paramRefTypes   = new Type[paramLength];
        for (int n = 0; n < paramLength; n++)
        {
            var paramType       = parameters[n].GetType();
            paramTypes[n]       = paramType;
            paramRefTypes[n]    = paramType.MakeByRefType(); 
        }
        foreach (ConstructorInfo constructor in constructors)
        {
            var ctorParameters = constructor.GetParameters();
            if (ctorParameters.Length != paramLength) {
                continue;
            }
            for (int n = 0; n < ctorParameters.Length; n++)
            {
                var ctorParamType = ctorParameters[n].ParameterType;
                if (ctorParamType == paramTypes[n] ||
                    ctorParamType == paramRefTypes[n]) {
                    return constructor;
                }
            }
        }
        return null;
    }
}

}