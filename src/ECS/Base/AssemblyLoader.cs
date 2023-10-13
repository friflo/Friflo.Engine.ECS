// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable UnusedMember.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public class AssemblyLoader
{
    private readonly    HashSet<Assembly>   checkedAssemblies   = new HashSet<Assembly>();
    private readonly    HashSet<Assembly>   dependencies        = new HashSet<Assembly>();
    private readonly    SortedSet<string>   loadedAssemblies    = new SortedSet<string>();

    private readonly    string              engineFullName;
    private             long                duration;
    
    internal AssemblyLoader()
    {
        var engineAssembly = typeof(Utils).Assembly;
        engineFullName  = engineAssembly.FullName;
        dependencies.Add(engineAssembly);
    }

    public override string ToString() {
        return $"Assemblies loaded: {loadedAssemblies.Count}, dependencies: {dependencies.Count}, duration: {duration} ms";
    }

    // --------------------------- query all struct / class component types ---------------------------
    internal Assembly[] GetAssemblies()
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
        return dependencies.ToArray();
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
                dependencies.Add(assembly);
            }
            var name = referencedAssemblyName.FullName;
            if (!loadedAssemblies.Add(name)) {
                continue;
            }
            var referencedAssembly = Assembly.Load(name);
            // Console.WriteLine(name);
            CheckAssembly(referencedAssembly);
        }
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
        var types       = assembly.GetTypes();
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
            foreach (var attr in type.CustomAttributes)
            {
                if (isValueType) {
                    var attributeType = attr.AttributeType;
                    if (attributeType == typeof(StructComponentAttribute)) {
                        componentTypes.Add(type);
                    }
                }
                if (isClass) {
                    var attributeType = attr.AttributeType;
                    if (attributeType == typeof(ClassComponentAttribute))
                    {
                        componentTypes.Add(type);
                    }
                }
            }
        }
    }
}