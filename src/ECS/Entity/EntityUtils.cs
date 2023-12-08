// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal static class EntityExtensions
{
    internal static int ComponentCount (this Entity entity) {
        return entity.archetype.componentCount + entity.Scripts.Length;
    }
}
    
    
internal static class EntityUtils
{
    internal static string EntityToString(Entity entity) {
        if (entity.store == null) {
            return "null";
        }
        return EntityToString(entity.id, entity.archetype, new StringBuilder());
    }
    
    internal static string EntityToString(int id, Archetype archetype, StringBuilder sb)
    {
        sb.Append("id: ");
        sb.Append(id);
        if (archetype == null) {
            sb.Append("  (detached)");
            return sb.ToString();
        }
        var entity = new Entity(id, archetype.entityStore);
        if (entity.HasName) {
            var name = entity.Name.value;
            if (name != null) {
                sb.Append("  \"");
                sb.Append(name);
                sb.Append('\"');
                return sb.ToString();
            }
        }
        if (entity.ComponentCount() == 0) {
            sb.Append("  []");
        } else {
            sb.Append("  [");
            var scripts = GetScripts(entity);
            foreach (var script in scripts) {
                sb.Append('*');
                sb.Append(script.GetType().Name);
                sb.Append(", ");
            }
            foreach (var heap in archetype.Heaps) {
                sb.Append(heap.StructType.Name);
                sb.Append(", ");
            }
            sb.Length -= 2;
            sb.Append(']');
        }
        return sb.ToString();
    }

    private static readonly EntityConverter DebugConverter       = new EntityConverter();
    private static readonly DataEntity      DebugDataEntity      = new DataEntity();      
    private static readonly ObjectWriter    DebugObjectWriter    = new ObjectWriter(new TypeStore()); // todo use global TypeStore
    
    internal static string GetDebugJSON(Entity entity)
    {
        var converter       = DebugConverter;
        lock (converter) {
            var dataEntity  = DebugDataEntity;
            var writer      = DebugObjectWriter;
            converter.EntityToDataEntity(entity, dataEntity, true);
            writer.Pretty             = true;
            writer.WriteNullMembers   = false;
            return writer.Write(dataEntity);
        }
    }
    
    // ---------------------------------- Script utils ----------------------------------
    private  static readonly Script[]       EmptyScripts  = Array.Empty<Script>();
    internal const  int                     NoScripts     = 0;  
    
    internal static Script[] GetScripts(Entity entity) {
        if (entity.scriptIndex == NoScripts) {
            return EmptyScripts;
        }
        return entity.archetype.entityStore.GetScripts(entity);
    }
    
    internal static Script GetScript(Entity entity, Type scriptType)
    {
        if (entity.scriptIndex == NoScripts) {
            return null;
        }
        return entity.archetype.entityStore.GetScript(entity, scriptType);
    }
    
    internal static Script AddScript(Entity entity, int scriptIndex, Script script)
    {
        var scriptType = EntityStoreBase.Static.EntitySchema.scripts[scriptIndex];
        return AddScriptInternal(entity, script, scriptType);
    }
    
    internal static Script AddNewScript(Entity entity, ScriptType scriptType)
    {
        var script = scriptType.CreateScript();
        return AddScriptInternal(entity, script, scriptType);
    }
    
    internal static Script AddScript (Entity entity, Script script) {
        var scriptType = EntityStoreBase.Static.EntitySchema.ScriptTypeByType[script.GetType()];
        return entity.archetype.entityStore.AddScript(entity, script, scriptType);
    }
    
    private static  Script AddScriptInternal(Entity entity, Script script, ScriptType scriptType)
    {
        if (script.entity.IsNotNull) {
            throw new InvalidOperationException($"script already added to an entity. current entity id: {script.entity.id}");
        }
        return entity.archetype.entityStore.AddScript(entity, script, scriptType);
    }
    
    internal static Script RemoveScript(Entity entity, int scriptIndex) {
        if (entity.scriptIndex == NoScripts) {
            return null;
        }
        var scriptType  = EntityStoreBase.Static.EntitySchema.scripts[scriptIndex];
        return entity.archetype.entityStore.RemoveScript(entity, scriptType);
    }
    
    internal static Script RemoveScriptType(Entity entity, ScriptType scriptType) {
        if (entity.scriptIndex == NoScripts) {
            return null;
        }
        return entity.archetype.entityStore.RemoveScript(entity, scriptType);
    }
}