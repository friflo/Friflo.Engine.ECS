// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable UseRawString
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

// ReSharper disable once InconsistentNaming
public static class NativeAOT
{
    private static readonly StaticAot AOT = new StaticAot();
    
    private class StaticAot {
        internal            EntitySchema        entitySchema;
        internal            bool                engineTypeRegistered;
        
        internal readonly   List<SchemaType>    types       = new();
        internal readonly   TypeStore           typeStore   = new TypeStore();
        internal readonly   SchemaTypes         schemaTypes = new();
    }
    
    internal static EntitySchema GetSchema()
    {
        Console.WriteLine("NativeAOT.GetSchema()");
        return AOT.entitySchema;
        if (AOT.entitySchema == null) {
            var msg =
@"EntitySchema not created. 
1. Register types with: NativeAOT.Register...() methods. 
2. Finish with:         NativeAOT.CreateSchema() on startup.";
            throw new InvalidOperationException(msg);
        }
        return AOT.entitySchema;
    }
    
    public static EntitySchema CreateSchema()
    {
        RegisterEngineTypes();
        Console.WriteLine("NativeAOT.CreateSchema() - begin");
        
        var dependant       = new EngineDependant (null, AOT.types);
        var dependants      = new List<EngineDependant> { dependant };
        AOT.entitySchema   = new EntitySchema(dependants, AOT.schemaTypes);
        Console.WriteLine("NativeAOT.CreateSchema() - end");
        return AOT.entitySchema;
    }
    
    private static void RegisterEngineTypes()
    {
        if (AOT.entitySchema != null) {
            throw new InvalidOperationException("EntitySchema already created");
        }
        if (AOT.engineTypeRegistered) {
            return;
        }
        AOT.engineTypeRegistered = true;

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
    
    public static void RegisterComponent<T>() where T : struct, IComponent 
    {
        RegisterEngineTypes();
        var components      = AOT.schemaTypes.components;
        var structIndex     = components.Count + 1;
        var componentType   = SchemaUtils.CreateComponentType<T>(AOT.typeStore, structIndex);
        components.Add(componentType);
        AOT.types.Add(componentType);
    }
    
    public static void RegisterTag<T>()  where T : struct, ITag 
    {
        RegisterEngineTypes();
        var tags            = AOT.schemaTypes.tags;
        var tagIndex        = tags.Count + 1;
        var tagType         = SchemaUtils.CreateTagType<T>(tagIndex);
        tags.Add(tagType);
        AOT.types.Add(tagType);
    }
    
    public static void RegisterScript<T>()  where T : Script, new()
    {
        RegisterEngineTypes();
        var scripts         = AOT.schemaTypes.scripts;
        var scriptIndex     = scripts.Count + 1;
        var scriptType      = SchemaUtils.CreateScriptType<T>(AOT.typeStore, scriptIndex);
        scripts.Add(scriptType);
        AOT.types.Add(scriptType);
    }
}