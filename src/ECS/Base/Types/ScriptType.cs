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
    private  readonly   CloneScript     cloneScript;    //  8
    #endregion
    
#region methods
    internal abstract   Script          CreateScript();
    internal abstract   void            ReadScript  (ObjectReader reader, JsonValue json, Entity entity);
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "MemberwiseClone is part of BCL")]
    internal ScriptType(string scriptKey, int scriptIndex, Type type, bool isBlittable, CloneScript cloneScript)
        : base (scriptKey, type, SchemaTypeKind.Script)
    {
        ScriptIndex         = scriptIndex;
        IsBlittable         = isBlittable;
        this.cloneScript    = cloneScript;
    }
    
    internal Script CloneScript(Script original)
    {
        var clone = cloneScript(original);
        return (Script)clone;
    }
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
    /// <summary>
    /// Create <see cref="TypeMapper"/> on demand.<br/>
    /// So possible exceptions in <see cref="TypeStore.GetTypeMapper{T}"/> thrown only when using JSON serialization.
    /// </summary>
    private             TypeMapper<T>   TypeMapper => typeMapper ??= typeStore.GetTypeMapper<T>();
    public  override    string          ToString() => $"Script: [*{typeof(T).Name}]";
    #endregion

#region fields
    private             TypeMapper<T>   typeMapper;
    private readonly    TypeStore       typeStore;
    #endregion
    
#region methods
    internal ScriptType(string scriptComponentKey, int scriptIndex, TypeStore typeStore, bool isBlittable, CloneScript cloneScript)
        : base(scriptComponentKey, scriptIndex, typeof(T), isBlittable, cloneScript)
    {
        this.typeStore = typeStore;
    }
    
    internal override Script CreateScript() {
        return new T();
    }
    
    internal override void ReadScript(ObjectReader reader, JsonValue json, Entity entity) {
        var script = entity.GetScript<T>();
        if (script != null) { 
            reader.ReadToMapper(TypeMapper, json, script, true);
            return;
        }
        script = reader.ReadMapper(TypeMapper, json);
        entity.archetype.entityStore.extension.AppendScript(entity, script);
    }
    #endregion
}
