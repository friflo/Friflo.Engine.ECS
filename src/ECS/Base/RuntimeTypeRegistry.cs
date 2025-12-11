using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Provides methods to register <see cref="IComponent"/>, <see cref="ITag"/>, and <see cref="Script"/> types at runtime.<br/>
/// <br/>
/// Types can be registered either before or after an <see cref="EntityStore"/> is created.
/// When registering after the schema is created, the new types will be added dynamically.
/// </summary>
/// <remarks>
/// Usage example:
/// <code>
/// // Register types - can be done at any time
/// RuntimeTypeRegistry.RegisterType(typeof(MyComponent));
/// RuntimeTypeRegistry.RegisterType(typeof(MyTag));
/// 
/// // Create or use EntityStore
/// var store = new EntityStore();
/// 
/// // Can also register more types after EntityStore is created
/// RuntimeTypeRegistry.RegisterType(typeof(AnotherComponent));
/// </code>
/// </remarks>
public static class RuntimeTypeRegistry
{
    private static readonly object                  LockObject      = new();
    private static readonly List<AssemblyType>      PendingTypes    = new();
    private static readonly HashSet<Type>           TypeSet         = new();
    private static          bool                    SchemaCreated;
    
    /// <summary>
    /// Register a type implementing <see cref="IComponent"/>, <see cref="ITag"/>, or extending <see cref="Script"/>
    /// for inclusion in the entity schema.<br/>
    /// <br/>
    /// Can be called at any time - before or after <see cref="EntityStore"/> creation.
    /// </summary>
    /// <param name="type">The type to register. Must implement IComponent, ITag, or extend Script.</param>
    /// <exception cref="ArgumentNullException">Thrown if type is null.</exception>
    /// <exception cref="ArgumentException">Thrown if type doesn't implement IComponent, ITag, or extend Script.</exception>
    public static void RegisterType(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        
        var kind = GetSchemaTypeKind(type);
        RegisterTypeInternal(type, kind);
    }
    
    /// <summary>
    /// Register a component type at runtime.
    /// </summary>
    /// <typeparam name="T">The component type implementing <see cref="IComponent"/>.</typeparam>
    public static void RegisterComponent<T>() where T : struct, IComponent
    {
        RegisterTypeInternal(typeof(T), SchemaTypeKind.Component);
    }
    
    /// <summary>
    /// Register a tag type at runtime.
    /// </summary>
    /// <typeparam name="T">The tag type implementing <see cref="ITag"/>.</typeparam>
    public static void RegisterTag<T>() where T : struct, ITag
    {
        RegisterTypeInternal(typeof(T), SchemaTypeKind.Tag);
    }
    
    /// <summary>
    /// Register a script type at runtime.
    /// </summary>
    /// <typeparam name="T">The script type extending <see cref="Script"/>.</typeparam>
    public static void RegisterScript<T>() where T : Script, new()
    {
        RegisterTypeInternal(typeof(T), SchemaTypeKind.Script);
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Dynamic registration uses reflection")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Dynamic registration uses reflection")]
    private static void RegisterTypeInternal(Type type, SchemaTypeKind kind)
    {
        lock (LockObject)
        {
            if (!TypeSet.Add(type))
                return; // Already registered
            
            if (SchemaCreated)
            {
                // Schema already exists - add type dynamically
                var schema = EntityStoreBase.Static.EntitySchema;
                schema.AddTypeRuntime(type, kind);
            }
            else
            {
                // Schema not yet created - add to pending list
                var assembly = type.Assembly;
                var assemblyType = new AssemblyType(type, kind, GetAssemblyIndex(assembly));
                PendingTypes.Add(assemblyType);
            }
        }
    }
    
    private static SchemaTypeKind GetSchemaTypeKind(Type type)
    {
        if (typeof(ITag).IsAssignableFrom(type))
            return SchemaTypeKind.Tag;
        
        if (typeof(IComponent).IsAssignableFrom(type))
            return SchemaTypeKind.Component;
        
        if (typeof(Script).IsAssignableFrom(type))
            return SchemaTypeKind.Script;
        
        throw new ArgumentException(
            $"Type '{type.FullName}' must implement IComponent, ITag, or extend Script.", 
            nameof(type));
    }
    
    private static readonly Dictionary<Assembly, int>   AssemblyMap     = new();
    private static readonly List<Assembly>              AssemblyList    = new();
    
    private static int GetAssemblyIndex(Assembly assembly)
    {
        if (AssemblyMap.TryGetValue(assembly, out int index))
            return index;
        
        // Use negative indices for runtime-registered assemblies
        // These will be mapped properly when the schema is created
        index = -AssemblyList.Count - 1;
        AssemblyMap.Add(assembly, index);
        AssemblyList.Add(assembly);
        return index;
    }
    
    /// <summary>
    /// Gets all types registered via <see cref="RegisterType"/> before schema creation.
    /// Called internally when creating the EntitySchema.
    /// </summary>
    internal static IReadOnlyList<AssemblyType> GetPendingTypes()
    {
        lock (LockObject)
        {
            return PendingTypes.ToArray();
        }
    }
    
    /// <summary>
    /// Gets the assemblies that contain runtime-registered types.
    /// </summary>
    internal static IReadOnlyList<Assembly> GetRegisteredAssemblies()
    {
        lock (LockObject)
        {
            return AssemblyList.ToArray();
        }
    }
    
    /// <summary>
    /// Marks the schema as created. After this point, new type registrations
    /// will be added dynamically to the existing schema.
    /// Called internally when the EntitySchema is created.
    /// </summary>
    internal static void MarkSchemaCreated()
    {
        lock (LockObject)
        {
            SchemaCreated = true;
        }
    }
    
    /// <summary>
    /// Returns true if the schema has been created.
    /// </summary>
    internal static bool IsSchemaCreated
    {
        get
        {
            lock (LockObject)
            {
                return SchemaCreated;
            }
        }
    }
    
    /// <summary>
    /// Checks if a type has been registered (either pending or in the schema).
    /// </summary>
    internal static bool IsTypeRegistered(Type type)
    {
        lock (LockObject)
        {
            return TypeSet.Contains(type);
        }
    }
    
    /// <summary>
    /// Marks a type as registered (called when schema adds a type from assembly scanning).
    /// </summary>
    internal static void MarkTypeRegistered(Type type)
    {
        lock (LockObject)
        {
            TypeSet.Add(type);
        }
    }
    
    /// <summary>
    /// Resets the registry for testing purposes only.
    /// </summary>
    internal static void Reset()
    {
        lock (LockObject)
        {
            PendingTypes.Clear();
            TypeSet.Clear();
            AssemblyMap.Clear();
            AssemblyList.Clear();
            SchemaCreated = false;
        }
    }
}
