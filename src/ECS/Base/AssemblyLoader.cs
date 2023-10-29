// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

// ReSharper disable UnusedMember.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal sealed class AssemblyLoader
{
    private readonly    HashSet<Assembly>   checkedAssemblies   = new HashSet<Assembly>();
    private readonly    HashSet<Assembly>   engineDependants    = new HashSet<Assembly>();
    private readonly    SortedSet<string>   loadedAssemblies    = new SortedSet<string>();

    private readonly    string              engineFullName;
    private             long                duration;
    
    internal AssemblyLoader()
    {
        var engineAssembly = typeof(Utils).Assembly;
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
        foreach (var assembly in engineDependants) {
            sb.Append(assembly.ManifestModule.Name);
            sb.Append(", ");
        }
        sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }

    // --------------------------- query all component, behavior and tag types ---------------------------
    internal Assembly[] GetEngineDependants()
    {
        var stopwatch = new Stopwatch(); 
        stopwatch.Start();
        // LoadAssemblies(); // used only for debugging
        
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

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
        return engineDependants.ToArray();
    }
    
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
            var referencedAssembly = LoadAssembly(referencedAssemblyName);
            if (referencedAssembly == null) {
                continue;
            }
            CheckAssembly(referencedAssembly);
        }
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
            // Console.WriteLine(name);
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
   
    internal static void AddComponentTypes(List<Type> componentTypes, Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            if (type.IsGenericType) {
                continue;
            }
            bool isValueType    = type.IsValueType;
            bool isClass        = type.IsClass;
            if (!isValueType && !isClass) {
                continue;
            }
            if (isValueType && typeof(IEntityTag).IsAssignableFrom(type)) {
                componentTypes.Add(type);
                continue;
            }
            AddComponentType(componentTypes, type);
        }
    }
    
    private static void AddComponentType(List<Type> componentTypes, Type type)
    {
        foreach (var attr in type.CustomAttributes)
        {
            if (type.IsValueType) {
                var attributeType = attr.AttributeType;
                if (attributeType == typeof(ComponentAttribute)) {
                    componentTypes.Add(type);
                }
            }
            if (type.IsClass) {
                var attributeType = attr.AttributeType;
                if (attributeType == typeof(BehaviorAttribute))
                {
                    componentTypes.Add(type);
                }
            }
        }
    }
}