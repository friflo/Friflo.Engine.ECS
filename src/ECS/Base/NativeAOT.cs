// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable UseRawString
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

// ReSharper disable once InconsistentNaming
public sealed class NativeAOT
{
    private             EntitySchema        entitySchema;
    private             bool                engineTypeRegistered;
        
    private readonly    List<SchemaType>    types       = new();
    private readonly    TypeStore           typeStore   = new TypeStore();
    private readonly    SchemaTypes         schemaTypes = new();
    
    private static          NativeAOT       Instance;
    
    internal static EntitySchema GetSchema()
    {
        Console.WriteLine("NativeAOT.GetSchema()");
        var schema = Instance?.entitySchema; 
        if (schema == null) {
            var msg =
@"EntitySchema not created.
NativeAOT requires schema creation on startup:
1. Create NativeAOT instance:   var aot = new NativeAOT();
2. Register types with:         aot.Register...(); 
3. Finish with:                 aot.CreateSchema();";
            throw new InvalidOperationException(msg);
        }
        return schema;
    }
    
    public EntitySchema CreateSchema()
    {
        RegisterEngineTypes();
        Console.WriteLine("NativeAOT.CreateSchema() - begin");
        
        var dependant   = new EngineDependant (null, types);
        var dependants  = new List<EngineDependant> { dependant };
        entitySchema    = new EntitySchema(dependants, schemaTypes);
        Console.WriteLine("NativeAOT.CreateSchema() - end");
        
        Instance = this;
        return entitySchema;
    }
    
    private void RegisterEngineTypes()
    {
        if (entitySchema != null) {
            throw new InvalidOperationException("EntitySchema already created");
        }
        if (engineTypeRegistered) {
            return;
        }
        engineTypeRegistered = true;

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
    
    public void RegisterComponent<T>() where T : struct, IComponent 
    {
        RegisterEngineTypes();
        var components      = schemaTypes.components;
        var structIndex     = components.Count + 1;
        var componentType   = SchemaUtils.CreateComponentType<T>(typeStore, structIndex);
        components.Add(componentType);
        types.Add(componentType);
    }
    
    public void RegisterTag<T>()  where T : struct, ITag 
    {
        RegisterEngineTypes();
        var tags            = schemaTypes.tags;
        var tagIndex        = tags.Count + 1;
        var tagType         = SchemaUtils.CreateTagType<T>(tagIndex);
        tags.Add(tagType);
        types.Add(tagType);
    }
    
    public void RegisterScript<T>()  where T : Script, new()
    {
        RegisterEngineTypes();
        var scripts         = schemaTypes.scripts;
        var scriptIndex     = scripts.Count + 1;
        var scriptType      = SchemaUtils.CreateScriptType<T>(typeStore, scriptIndex);
        scripts.Add(scriptType);
        types.Add(scriptType);
    }
}