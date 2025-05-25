using System;
using Friflo.Json.Burst;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Runtime;

internal sealed class HandleMatcher : ITypeMatcher
{
    public TypeMapper MatchTypeMapper(Type type, StoreConfig config)
    {
        if (!type.IsGenericType) {
            return null;
        }
        var genericType = type.GetGenericTypeDefinition();
        if (genericType != typeof(Handle<>)) {
            return null;
        }
        Type assetType = type.GenericTypeArguments[0];
        object[] constructorParams = [config, type];
        var newInstance = TypeMapperUtils.CreateGenericInstance(typeof(HandleMatcher<>), [assetType], constructorParams);
        return (TypeMapper) newInstance;
    }        
}

internal sealed class HandleMatcher<TInstance> : TypeMapper<Handle<TInstance>>
{
    public override string  DataTypeName()          => $"Handle<{typeof(TInstance).Name}>";
    public override bool    IsNull(ref Handle<TInstance> value)  => true;
    
    public HandleMatcher(StoreConfig config, Type type) :
        base(config, type, true, false) {
    }

    public override void Write(ref Writer writer, Handle<TInstance> slot) {
        writer.WriteString(slot.path);
    }

    public override Handle<TInstance> Read(ref Reader reader, Handle<TInstance> value, out bool success) {

        if (reader.parser.Event != JsonEvent.ValueString)
            return reader.HandleEvent(this, out success);
        success     = true;
        var path    = reader.parser.value.AsString();
        value       = Handle<TInstance>.CreateInstance(path);
        return value;
    }
}