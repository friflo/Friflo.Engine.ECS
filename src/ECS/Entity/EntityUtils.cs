// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Friflo.Engine.ECS.Serialize;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Used to provide additional debug information for an <see cref="Entity"/>:<br/>
/// <see cref="Entity.Pid"/>                <br/>
/// <see cref="Entity.Enabled"/>            <br/>
/// <see cref="Entity.DebugJSON"/>          <br/>
/// <see cref="Entity.DebugEventHandlers"/> <br/>
/// </summary>
internal readonly struct EntityInfo
{
#region properties
    internal            long                Pid             => entity.Pid;
    internal            bool                Enabled         => entity.Enabled;
    internal            string              JSON            => EntityUtils.EntityToJSON(entity);
    internal            DebugEventHandlers  EventHandlers   => EntityStore.GetEventHandlers(entity.store, entity.Id);
    public   override   string              ToString()      => "";
    #endregion

    [Browse(Never)] private readonly Entity entity;
    
    internal EntityInfo(Entity entity) {
        this.entity = entity;
    }
}

/// <summary>
/// Used to check if two <see cref="Entity"/> instances are the same entity by comparing their <see cref="Entity.Id"/>'s. 
/// </summary>
public sealed class EntityEqualityComparer : IEqualityComparer<Entity>
{
    public  bool    Equals(Entity left, Entity right)   => left.Id == right.Id;
    public  int     GetHashCode(Entity entity)          => entity.Id;
}

// ReSharper disable once UnusedType.Global
internal static class EntityExtensions { } // Engine.ECS must not have an Entity extension class to avoid confusion

public static class EntityUtils
{
    public static  readonly EntityEqualityComparer EqualityComparer = new ();
 
    // ------------------------------------------- public methods -------------------------------------------
#region non generic component - methods
    /// <summary>
    /// Returns a copy of the entity component as an object.<br/>
    /// The returned <see cref="IComponent"/> is a boxed struct.<br/>
    /// So avoid using this method whenever possible. Use <see cref="Entity.GetComponent{T}"/> instead.
    /// </summary>
    public static  IComponent   GetEntityComponent    (Entity entity, ComponentType componentType) {
        return entity.archetype.heapMap[componentType.StructIndex].GetComponentDebug(entity.compIndex);
    }

    public static  bool RemoveEntityComponent (Entity entity, ComponentType componentType)
    {
        return componentType.RemoveEntityComponent(entity);
    }
    
    public static  bool AddEntityComponent    (Entity entity, ComponentType componentType) {
        return componentType.AddEntityComponent(entity);
    }
    
    public static  bool AddEntityComponentValue(Entity entity, ComponentType componentType, object value) {
        return componentType.AddEntityComponentValue(entity, value);
    }
    #endregion
    
#region non generic script - methods
    public static   Script      GetEntityScript    (Entity entity, ScriptType scriptType) => GetScript       (entity, scriptType.Type);
    
    public static   Script      RemoveEntityScript (Entity entity, ScriptType scriptType) => RemoveScriptType(entity, scriptType);
    
    public static   Script      AddNewEntityScript (Entity entity, ScriptType scriptType) => AddNewScript    (entity, scriptType);
    
    public static   Script      AddEntityScript    (Entity entity, Script script)         => AddScript       (entity, script);

    #endregion
    
    // ------------------------------------------- internal methods -------------------------------------------
#region internal - methods
    internal static int ComponentCount (this Entity entity) {
        return entity.archetype.componentCount + entity.Scripts.Length;
    }
    
    internal static Exception NotImplemented(int id, string use) {
        var msg = $"to avoid excessive boxing. Use {use} or {nameof(EntityUtils)}.{nameof(EqualityComparer)}. id: {id}";
        return new NotImplementedException(msg);
    }
    
    internal static string EntityToString(Entity entity) {
        if (entity.store == null) {
            return "null";
        }
        return EntityToString(entity.Id, entity.archetype, new StringBuilder());
    }
    
    internal static string EntityToString(int id, Archetype archetype, StringBuilder sb)
    {
        sb.Append("id: ");
        sb.Append(id);
        if (archetype == null) {
            sb.Append("  (detached)");
            return sb.ToString();
        }
        var entity = new Entity(archetype.entityStore, id);
        if (entity.HasName) {
            var name = entity.Name.value;
            if (name != null) {
                sb.Append("  \"");
                sb.Append(name);
                sb.Append('\"');
            }
        }
        var typeCount = archetype.componentCount + archetype.tags.Count + entity.Scripts.Length; 
        if (typeCount == 0) {
            sb.Append("  []");
        } else {
            sb.Append("  [");
            foreach (var heap in archetype.Heaps()) {
                sb.Append(heap.StructType.Name);
                sb.Append(", ");
            }
            foreach (var tag in archetype.Tags) {
                sb.Append('#');
                sb.Append(tag.Name);
                sb.Append(", ");
            }
            var scripts = GetScripts(entity);
            foreach (var script in scripts) {
                sb.Append('*');
                sb.Append(script.GetType().Name);
                sb.Append(", ");
            }
            sb.Length -= 2;
            sb.Append(']');
        }
        return sb.ToString();
    }

    private static readonly EntitySerializer EntitySerializer   = new EntitySerializer();
    
    internal static string EntityToJSON(Entity entity)
    {
        var serializer = EntitySerializer;
        lock (serializer) {
            return serializer.WriteEntity(entity);
        }
    }
    
    /// <remarks> The "id" in the passed JSON <paramref name="value"/> is ignored. </remarks>
    internal static void JsonToEntity(Entity entity, string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        var serializer = EntitySerializer;
        lock (serializer) {
            var jsonValue = new JsonValue(value);
            var error = serializer.ReadIntoEntity(entity, jsonValue);
            if (error == null) {
                return;
            }
            throw new ArgumentException(error);
        }
    }
    
    private static readonly DataEntitySerializer DataEntitySerializer = new DataEntitySerializer();
    
    internal static string DataEntityToJSON(DataEntity dataEntity)
    {
        var serializer = DataEntitySerializer;
        lock (serializer) {
            var json = serializer.WriteDataEntity(dataEntity, out string error);
            if (json == null) {
                return error;
            }
            return json;
        }
    }
    
    // ---------------------------------- Script utils ----------------------------------
    private  static readonly Script[]                       EmptyScripts        = Array.Empty<Script>();
    internal const  int                                     NoScripts           = 0;
    internal static readonly Tags                           Disabled            = Tags.Get<Disabled>();
    private  static readonly ScriptType[]                   ScriptTypes         = EntityStoreBase.Static.EntitySchema.scripts;
    private  static readonly Dictionary<Type, ScriptType>   ScriptTypeByType    = EntityStoreBase.Static.EntitySchema.scriptTypeByType;
    
    internal static Script[] GetScripts(Entity entity) {
        if (entity.scriptIndex == NoScripts) {
            return EmptyScripts;
        }
        return EntityStore.GetScripts(entity); 
    }
    
    internal static Script GetScript(Entity entity, Type scriptType)
    {
        if (entity.scriptIndex == NoScripts) {
            return null;
        }
        return EntityStore.GetScript(entity, scriptType);
    }
    
    internal static Script AddScript(Entity entity, int scriptIndex, Script script)
    {
        var scriptType = ScriptTypes[scriptIndex];
        return AddScriptInternal(entity, script, scriptType);
    }
    
    private static Script AddNewScript(Entity entity, ScriptType scriptType)
    {
        var script = scriptType.CreateScript();
        return AddScriptInternal(entity, script, scriptType);
    }
    
    internal static Script AddScript (Entity entity, Script script) {
        var scriptType = ScriptTypeByType[script.GetType()];
        return entity.archetype.entityStore.AddScript(entity, script, scriptType);
    }
    
    private static  Script AddScriptInternal(Entity entity, Script script, ScriptType scriptType)
    {
        if (!script.entity.IsNull) {
            throw new InvalidOperationException($"script already added to an entity. current entity id: {script.entity.Id}");
        }
        return entity.archetype.entityStore.AddScript(entity, script, scriptType);
    }
    
    internal static Script RemoveScript(Entity entity, int scriptIndex) {
        if (entity.scriptIndex == NoScripts) {
            return null;
        }
        var scriptType  = ScriptTypes[scriptIndex];
        return entity.archetype.entityStore.RemoveScript(entity, scriptType);
    }
    
    private static Script RemoveScriptType(Entity entity, ScriptType scriptType) {
        if (entity.scriptIndex == NoScripts) {
            return null;
        }
        return entity.archetype.entityStore.RemoveScript(entity, scriptType);
    }
    
    internal static void AddTreeTags(Entity entity, in Tags tags)
    {
        var list = entity.store.GetEntityList();
        list.Clear();
        list.AddTree(entity);
        try {
            list.ApplyAddTags(tags);
        } finally {
            entity.store.ReturnEntityList(list);
        }
    }
    
    internal static void RemoveTreeTags(Entity entity, in Tags tags)
    {
        var list = entity.store.GetEntityList();
        list.Clear();
        list.AddTree(entity);
        try {
            list.ApplyRemoveTags(tags);
        } finally {
            entity.store.ReturnEntityList(list);
        }
    }
    #endregion
}