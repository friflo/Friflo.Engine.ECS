// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal sealed class AssemblyLoader
{
    private  readonly   HashSet<Assembly>       checkedAssemblies   = new HashSet<Assembly>();
    private  readonly   HashSet<Assembly>       engineDependants    = new HashSet<Assembly>();
    private  readonly   SortedSet<string>       loadedAssemblies    = new SortedSet<string>();
    internal readonly   List<EngineDependant>   dependants          = new List<EngineDependant>();

    private  readonly   string                  engineFullName;
    private             long                    duration;
    
    internal AssemblyLoader()
    {
        var engineAssembly = typeof(ArrayUtils).Assembly;
        engineFullName  = engineAssembly.FullName;
        engineDependants.Add(engineAssembly);
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append("Assemblies loaded: ");
        sb.Append(loadedAssemblies.Count);
        sb.Append(", duration: ");
        sb.Append(duration);
        sb.Append(" ms");
        sb.Append(", engine-dependants: [");
        foreach (var dependant in dependants) {
            sb.Append(dependant.AssemblyName);
            sb.Append(" (");
            sb.Append(dependant.Types.Length);
            sb.Append("),  ");
        }
        sb.Length -= 3;
        sb.Append(']');
        return sb.ToString();
    }

    // --------------------------- query all component, script and tag types ---------------------------
    internal Assembly[] GetEngineDependants()
    {
        var stopwatch = new Stopwatch(); 
        stopwatch.Start();
        // LoadAssemblies(); // used only for debugging
        
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        // excluding BCL assemblies provide no performance gain
        // assemblies = BaseClassFilter.RemoveBaseClassAssemblies(assemblies);

        foreach (var assembly in assemblies) {
            CheckAssembly(assembly);
        }
        duration = stopwatch.ElapsedMilliseconds;
        /*
        // Assemblies with same name but different versions return the same reference in Assembly.Load()
        var thread = loadedAssemblies.Where(name => name.StartsWith("System.Threading.Thread,")).ToArray();
        var asm0 = Assembly.Load(thread[0]);
        var asm1 = Assembly.Load(thread[1]);
        var asm2 = Assembly.Load(thread[2]);
        */
        var result = new Assembly[engineDependants.Count];
        engineDependants.CopyTo(result);
        return result;
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Not called for NativeAOT")]
    private void CheckAssembly(Assembly assembly)
    {
        if (!checkedAssemblies.Add(assembly)) {
            return;
        }
        // if (assembly.FullName.Contains("Tests,"))           { int i = 3; }
        // if (assembly.FullName.Contains("Tests-internal,"))  { int i = 3; }
        var referencedAssemblies = assembly.GetReferencedAssemblies();
        foreach (var referencedAssemblyName in referencedAssemblies)
        {
            if (referencedAssemblyName.FullName == engineFullName) {
                engineDependants.Add(assembly);
            }
            var name = referencedAssemblyName.FullName;
            if (!loadedAssemblies.Add(name)) {
                continue;
            }
            CheckReferencedAssembly(referencedAssemblyName);
        }
    }
    
    [ExcludeFromCodeCoverage] // running tests without debugging load all assemblies successful
    private void CheckReferencedAssembly(AssemblyName assemblyName)
    {
        var referencedAssembly = LoadAssembly(assemblyName);
        if (referencedAssembly == null) {
            return; // case not reached in unit tests. Can be reached when using a debugger
        }
        CheckAssembly(referencedAssembly);
    }

    /// <summary>
    /// <see cref="Assembly.Load(string)"/> fails for assemblies loaded when debugging. These are:
    /// <code>
    ///     System.Security.Permissions
    ///     System.Threading.AccessControl
    ///     System.CodeDom
    ///     Microsoft.Win32.SystemEvents
    ///     System.Configuration.ConfigurationManager
    ///     System.Diagnostics.PerformanceCounter
    ///     System.Diagnostics.EventLog
    ///     System.IO.Ports
    ///     System.Windows.Extensions
    /// </code>
    /// </summary>
    [ExcludeFromCodeCoverage]
    private static Assembly LoadAssembly(AssemblyName assemblyName)
    {
        try {
            var assembly = Assembly.Load(assemblyName.FullName);
            // Console.WriteLine(assembly.GetName());
            return assembly;
        }
        catch (Exception) {
            Console.WriteLine($"Failed loading Assembly: {assemblyName.Name}");
        }
        return null;
    }

    // Note!: Used for debugging: Do not remove
    /* 
    private static void LoadAssemblies()
    {
        var domain              = AppDomain.CurrentDomain;
        var loadedAssemblies    = domain.GetAssemblies().ToList();
        var loadedPaths         = loadedAssemblies.Select(a => a.Location).ToArray();
        var referencedPaths     = Directory.GetFiles(domain.BaseDirectory, "*.dll");
        var toLoad              = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
        toLoad.ForEach(path => loadedAssemblies.Add(domain.Load(AssemblyName.GetAssemblyName(path))));        
    } */
   
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Not called for NativeAOT")]
    internal static void GetComponentTypes(Assembly assembly, int assemblyIndex, List<AssemblyType> componentTypes)
    {
        componentTypes.Clear();
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            bool isValueType    = type.IsValueType;
            bool isClass        = type.IsClass;
            if (!isValueType && !isClass) {
                continue;
            }
            if (isValueType) {
                if (typeof(ITag).IsAssignableFrom(type)) {
                    AddType(assemblyIndex, type, SchemaTypeKind.Tag, componentTypes);
                    continue;
                }
                if (typeof(IComponent).IsAssignableFrom(type)) {
                    AddType(assemblyIndex, type, SchemaTypeKind.Component, componentTypes);
                    continue;
                }
            }
            if (isClass && type.IsSubclassOf(typeof(Script))) {
                componentTypes.Add(new AssemblyType(type, SchemaTypeKind.Script, assemblyIndex));
            }
        }
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2055", Justification = "Not called for NativeAOT")]
    private static void AddType(int assemblyIndex, Type type, SchemaTypeKind kind, List<AssemblyType> componentTypes)
    {
        if (!type.IsGenericType) {
            componentTypes.Add(new AssemblyType(type, kind, assemblyIndex));
            return;
        }
        var genericTypes = SchemaUtils.GetGenericInstanceTypes(type);
        foreach (var genericType in genericTypes) {
            var genType = type.MakeGenericType(genericType.types);
            componentTypes.Add(new AssemblyType(genType, kind, assemblyIndex));
        }
    }
}

[ExcludeFromCodeCoverage]
internal static class BaseClassFilter
{
    private static readonly ulong[] MicrosoftPublicTokens = {
        0x_b03f5f7f11d50a3a,   // 187 Microsoft (Debug)
        0x_cc7b13ffcd2ddd51,   //  30 Microsoft (Debug)
        0x_7cec85d7bea7798e,   //   1 Microsoft - System.Private.CoreLib
    };
    
    private static ulong BytesToLong(byte[] buffer)
    {
        ulong result = 0;
        for (int n = 0; n < buffer.Length; n++) {
            result |= (ulong)buffer[n] << (56 - 8 * n); 
        }
        return result;
    }
    
    private static bool IsMicrosoftToken(ulong token)
    {
        foreach (var msToken in MicrosoftPublicTokens) {
            if (msToken == token) {
                return true;
            }
        }
        return false;
    }
    
    internal static Assembly[] RemoveBaseClassAssemblies(Assembly[] assemblies)
    {
        var result = new List<Assembly>(assemblies.Length);
        foreach (var assembly in assemblies) {
            AssemblyName name = assembly.GetName();
            byte[] tokenBytes = name.GetPublicKeyToken();
            var token = BytesToLong(tokenBytes);
            if (IsMicrosoftToken(token)) {
                continue;
            }
            // Console.WriteLine(assembly.GetName());
            result.Add(assembly);
        }
        return result.ToArray();
    }    
}