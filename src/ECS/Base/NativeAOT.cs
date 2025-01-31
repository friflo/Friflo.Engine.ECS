// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Friflo.Engine.ECS.Index;
using Friflo.Engine.ECS.Relations;

// ReSharper disable UseRawString
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

// ReSharper disable once InconsistentNaming
public sealed class NativeAOT
{
    private             EntitySchema                entitySchema;
    private             bool                        engineTypesRegistered;
        
    private readonly    HashSet<Type>               typeSet     = new();
    private readonly    SchemaTypes                 schemaTypes = new();
    private readonly    Dictionary<Assembly, int>   assemblyMap = new();
    private readonly    List<Assembly>              assemblies  = new();
    
    private static      NativeAOT           Instance;
    
    [ExcludeFromCodeCoverage]
    internal static EntitySchema GetSchema()
    {
        var schema = Instance?.entitySchema;
        if (schema != null) {
            return schema;
        }
        return CreateDefaultSchema();
    }
    
    [ExcludeFromCodeCoverage]
    private static EntitySchema CreateDefaultSchema()
    {
        var schema = Instance?.entitySchema;
        if (schema != null) {
            return schema;
        }
        var msg =
@"EntitySchema not created.
NativeAOT requires schema creation on startup:
1. Create NativeAOT instance:   var aot = new NativeAOT();
2. Register types with:         aot.Register...(); 
3. Finish with:                 aot.CreateSchema();";
        Console.Error.WriteLine(msg);
        var aot = new NativeAOT();
        Console.WriteLine("Using default EntitySchema");
        return aot.CreateSchemaInternal();
/*  Return default schema instead of throwing an exception.
    By doing this subsequent access to components, tags & script result in meaningful stack traces.
    
    Throwing an exception is not helpful.
    E.g. the exception is thrown from within a constructor - like EntityStore(). In this case the exception log looks like:
        
A type initializer threw an exception. To determine which type, inspect the InnerException's StackTrace property.
   Stack Trace:
   at System.Runtime.CompilerServices.ClassConstructorRunner.EnsureClassConstructorRun(StaticClassConstructionContext*) + 0x247
   at System.Runtime.CompilerServices.ClassConstructorRunner.CheckStaticClassConstructionReturnGCStaticBase(StaticClassConstructionContext*, Object) + 0x1c
   at Friflo.Engine.ECS.EntityStoreBase.GetArchetypeConfig(EntityStoreBase) + 0x39
   at Friflo.Engine.ECS.EntityStoreBase..ctor() + 0xe1
   at Friflo.Engine.ECS.EntityStore..ctor(PidType) + 0x43
   at Friflo.Engine.ECS.EntityStore..ctor() + 0x1a
*/
    }
    
    private EntitySchema CreateSchemaInternal()
    {
        InitSchema();

        var dependants  = schemaTypes.CreateSchemaTypes(assemblies);
        entitySchema    = new EntitySchema(dependants, schemaTypes);
        Instance        = this;
        return entitySchema;
    }
    
    public EntitySchema CreateSchema()
    {
        Console.WriteLine("NativeAOT.CreateSchema()");
        return CreateSchemaInternal();
    }
    
    private void InitSchema()
    {
        if (Instance?.entitySchema != null) {
            throw new InvalidOperationException("EntitySchema already created");
        }
        if (engineTypesRegistered) {
            return;
        }
        engineTypesRegistered = true;

        RegisterComponent<EntityName>();
        RegisterComponent<Position>();
        RegisterComponent<Rotation>();
        RegisterComponent<Scale3>();
        RegisterComponent<Transform>();
        RegisterComponent<TreeNode>();
        RegisterComponent<UniqueEntity>();
        RegisterComponent<Unresolved>();

        RegisterTag<Disabled>();
    }
    
    private void AddType(Type type, SchemaTypeKind kind)
    {
        var assembly = type.Assembly;
        if (!assemblyMap.TryGetValue(assembly, out int assemblyIndex)) {
            assemblyIndex = assemblies.Count;
            assemblyMap.Add(assembly, assemblyIndex);
            assemblies.Add(assembly);
        }
        schemaTypes.AddSchemaType(new AssemblyType(type, kind, assemblyIndex));
    }
    
    public void RegisterComponent<T>() where T : struct, IComponent 
    {
        InitSchema();
        if (typeSet.Add(typeof(T))) {
            AddType(typeof(T), SchemaTypeKind.Component);
            SchemaUtils.CreateComponentType<T>(0, null, null); // dummy call to prevent trimming required type info
        }
    }
    
    public void RegisterIndexedComponentClass<T, TValue>()
        where T : struct, IIndexedComponent<TValue>
        where TValue : class
    {
        InitSchema();
        if (typeSet.Add(typeof(T)))
        {
            AddType(typeof(T), SchemaTypeKind.Component);
            SchemaUtils.CreateComponentType<T>(0, null, null);              // dummy call to prevent trimming required type info
            IndexedValueUtils.GetIndexedComponentValue<T, TValue>(default); // dummy call to prevent trimming required type info
            ComponentIndexUtils.CreateComponentIndexNativeAot[typeof(T)] = (store, componentType) => {
                return new ValueClassIndex<T, TValue>(store, componentType);
            };
        }
    }
    
    public void RegisterIndexedComponentStruct<T, TValue>()
        where T : struct, IIndexedComponent<TValue>
        where TValue : struct
    {
        InitSchema();
        if (typeSet.Add(typeof(T)))
        {
            AddType(typeof(T), SchemaTypeKind.Component);
            SchemaUtils.CreateComponentType<T>(0, null, null);              // dummy call to prevent trimming required type info
            IndexedValueUtils.GetIndexedComponentValue<T, TValue>(default); // dummy call to prevent trimming required type info
            ComponentIndexUtils.CreateComponentIndexNativeAot[typeof(T)] = (store, componentType) => {
                return new ValueStructIndex<T, TValue>(store, componentType);
            };
        }
    }
    
    public void RegisterIndexedComponentEntity<T>()
        where T : struct, ILinkComponent
    {
        InitSchema();
        if (typeSet.Add(typeof(T)))
        {
            AddType(typeof(T), SchemaTypeKind.Component);
            SchemaUtils.CreateComponentType<T>(0, null, null);              // dummy call to prevent trimming required type info
            IndexedValueUtils.GetIndexedComponentValue<T, Entity>(default); // dummy call to prevent trimming required type info
            ComponentIndexUtils.CreateComponentIndexNativeAot[typeof(T)] = (store, componentType) => {
                return new EntityIndex<T>(store, componentType);
            };
        }
    }
    
    public void RegisterRelation<T, TKey>()
        where T : struct, IRelation<TKey>
    {
        InitSchema();
        if (typeSet.Add(typeof(T)))
        {
            AddType(typeof(T), SchemaTypeKind.Component);
            RelationUtils.GetRelationKey<T,TKey>(default);          // dummy call to prevent trimming required type info
            SchemaUtils.CreateRelationType<T>(0, null, null);       // dummy call to prevent trimming required type info
            AbstractEntityRelations.CreateEntityRelationsNativeAot[typeof(T)] = (componentType, archetype, heap) => {
                return new GenericEntityRelations<T, TKey>(componentType, archetype, heap);
            };
        }
    }
    
    public void RegisterLinkRelation<T>()
        where T : struct, ILinkRelation
    {
        InitSchema();
        if (typeSet.Add(typeof(T)))
        {
            AddType(typeof(T), SchemaTypeKind.Component);
            RelationUtils.GetRelationKey<T,Entity>(default);        // dummy call to prevent trimming required type info
            SchemaUtils.CreateRelationType<T>(0, null, null);       // dummy call to prevent trimming required type info
            AbstractEntityRelations.CreateEntityRelationsNativeAot[typeof(T)] = (componentType, archetype, heap) => {
                return new EntityLinkRelations<T>(componentType, archetype, heap);
            };
        }
    }

    public void RegisterTag<T>()  where T : struct, ITag 
    {
        InitSchema();
        if (typeSet.Add(typeof(T))) {
            AddType(typeof(T), SchemaTypeKind.Tag);
            SchemaUtils.CreateTagType<T>(0);                        // dummy call to prevent trimming required type info
        }
    }
    
    public void RegisterScript<T>()  where T : Script, new()
    {
        InitSchema();
        if (typeSet.Add(typeof(T))) {
            AddType(typeof(T), SchemaTypeKind.Script);
            SchemaUtils.CreateScriptType<T>(0);          // dummy call to prevent trimming required type info
        }
    }
}