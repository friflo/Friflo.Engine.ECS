using System;
using Friflo.Json.Burst;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Runtime;

internal sealed class AssetMatcher : ITypeMatcher
{
    public TypeMapper MatchTypeMapper(Type type, StoreConfig config)
    {
        if (!type.IsGenericType) {
            return null;
        }
        var genericType = type.GetGenericTypeDefinition();
        if (genericType != typeof(Asset<>)) {
            return null;
        }
        Type assetType = type.GenericTypeArguments[0];
        object[] constructorParams = [config, type];
        var newInstance = TypeMapperUtils.CreateGenericInstance(typeof(AssetMapper<>), [assetType], constructorParams);
        return (TypeMapper) newInstance;
    }        
}

internal sealed class AssetMapper<T> : TypeMapper<Asset<T>>
{
    public override string  DataTypeName()              => $"Asset<{typeof(T).Name}>";
    public override bool    IsNull(ref Asset<T> value)  => value.Resource == null;
    
    public AssetMapper(StoreConfig config, Type type) :
        base(config, type, true, false) {
    }

    public override void Write(ref Writer writer, Asset<T> slot) {
        writer.WriteString(slot.path);
    }

    public override Asset<T> Read(ref Reader reader, Asset<T> value, out bool success) {

        if (reader.parser.Event != JsonEvent.ValueString)
            return reader.HandleEvent(this, out success);
        success     = true;
        var path    = reader.parser.value.AsString();
        value = Asset<T>.Get(path);
        return value;
    }
}