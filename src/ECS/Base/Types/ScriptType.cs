// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal delegate object CloneScript(object instance);

/// <summary>
/// Provide meta data for a <see cref="Script"/> class. 
/// </summary>
public abstract class ScriptType : SchemaType
{
#region fields
    /// <summary> Ihe index in <see cref="EntitySchema"/>.<see cref="EntitySchema.Scripts"/>. </summary>
    public   readonly   int             ScriptIndex;    //  4
    /// <summary> Return true if <see cref="Script"/>'s of this type can be copied. </summary>
    public   readonly   bool            IsBlittable;    //  4
    #endregion
    
#region methods
    internal abstract   Script          CreateScript();
    internal abstract   void            ReadScript  (ObjectReader reader, JsonValue json, Entity entity);
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "MemberwiseClone is part of BCL")]
    internal ScriptType(string scriptKey, int scriptIndex, Type type, bool isBlittable)
        : base (scriptKey, type, SchemaTypeKind.Script)
    {
        ScriptIndex = scriptIndex;
        IsBlittable = isBlittable;
    }
    
    internal abstract Script CloneScript(Script source);
    /* {
        var clone = cloneScript(original);
        return (Script)clone;
    }*/
    #endregion
}

/// <remarks>
/// Note:<br/>
/// Before <see cref="ScriptInfo{T}.Index"/> was a static field in <see cref="ScriptType{T}"/>. <br/> 
/// But this approach fails in Unity. Reason: <br/> 
/// Unity initializes static fields of generic types already when creating an instance of that type.
/// </remarks>
internal static class ScriptInfo<T>
{
    // Check initialization by directly calling unit test method: Test_SchemaType.Test_SchemaType_Script_Index()
    // readonly improves performance significant
    internal static readonly   int      Index = SchemaTypeUtils.GetScriptIndex(typeof(T));
}

internal sealed class ScriptType<T> : ScriptType 
    where T : Script, new()
{
#region properties
    public  override    string          ToString() => $"Script: [*{typeof(T).Name}]";
    #endregion

   
#region methods
    internal ScriptType(string scriptComponentKey, int scriptIndex, bool isBlittable)
        : base(scriptComponentKey, scriptIndex, typeof(T), isBlittable)
    {
    }
    
    internal override Script CreateScript() {
        return new T();
    }
    
    internal override Script CloneScript(Script source) {
        var clone = new T();
        var copyScript = CopyScriptUtils<T>.CopyScript;
        if (copyScript != null) {
            copyScript((T)source, clone);
            return clone;
        }
        var name = typeof(T).Name;
        var msg = $"at {typeof(T).Namespace}.{name} - expect: static void CopyScript({name} source, {name} target)";
        throw new MissingMethodException(msg);
    }
    
    internal override void ReadScript(ObjectReader reader, JsonValue json, Entity entity) {
        var mapper = (TypeMapper<T>)reader.TypeCache.GetTypeMapper(typeof(T));
        var script = entity.GetScript<T>();
        if (script != null) { 
            reader.ReadToMapper(mapper, json, script, true);
            return;
        }
        script = reader.ReadMapper(mapper, json);
        entity.archetype.entityStore.extension.AppendScript(entity, script);
    }
    #endregion
}
