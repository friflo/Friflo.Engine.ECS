// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal delegate object CloneScript(object instance);

public abstract class ScriptType : SchemaType
{
    /// <summary>
    /// Ihe index in <see cref="EntitySchema.Scripts"/>.<br/>
    /// </summary>
    public   readonly   int             scriptIndex;    //  4
    public   readonly   bool            blittable;      //  4
    private  readonly   CloneScript     cloneScript;    //  8
    
    internal abstract   Script          CreateScript();
    internal abstract   void            ReadScript  (ObjectReader reader, JsonValue json, Entity entity);
    
    protected ScriptType(string scriptKey, int scriptIndex, Type type)
        : base (scriptKey, type, SchemaTypeKind.Script)
    {
        this.scriptIndex    = scriptIndex;
        blittable           = IsBlittableType(type);
        if (blittable) {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod;
            var methodInfo      = type.GetMethod("MemberwiseClone", flags);
            // Create a delegate representing an 'open instance method'.
            // An instance must be passed when the delegate is invoked.
            // See: https://learn.microsoft.com/en-us/dotnet/api/system.delegate.createdelegate
            var cloneDelegate   = Delegate.CreateDelegate(typeof(CloneScript), null, methodInfo!);
            cloneScript         = (CloneScript)cloneDelegate;
        }
    }
    
    internal Script CloneScript(Script original)
    {
        var clone = cloneScript(original);
        return (Script)clone;
    }
}

internal sealed class ScriptType<T> : ScriptType 
    where T : Script, new()
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"script: '{componentKey}' [*{typeof(T).Name}]";
    
    internal ScriptType(string scriptKey, int scriptIndex, TypeStore typeStore)
        : base(scriptKey, scriptIndex, typeof(T))
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override Script CreateScript() {
        return new T();
    }
    
    internal override void ReadScript(ObjectReader reader, JsonValue json, Entity entity) {
        var script = entity.GetScript<T>();
        if (script != null) { 
            reader.ReadToMapper(typeMapper, json, script, true);
            return;
        }
        script = reader.ReadMapper(typeMapper, json);
        entity.archetype.entityStore.AppendScript(entity, script);
    }
}
