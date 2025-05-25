using System;
using System.Collections.Generic;
using System.Diagnostics;
using Friflo.Json.Fliox;

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Runtime;

/// <summary>
/// <b>API in Friflo.Engine.Runtime is WIP and may change</b><br/>
/// </summary>
public interface IAssetLoader<out TAsset> {
    TAsset LoadAsset(string path, AssetSchema assetSchema);
}


/// <summary>
/// <b>API in Friflo.Engine.Runtime is WIP and may change</b><br/>
/// An immutable asset identified by its path.
/// </summary>
/// <typeparam name="TAsset">The type on an asset</typeparam>
[TypeMapper(typeof(AssetMatcher))]
public class Asset<TAsset> : AbstractAsset
{
    public              TAsset  Resource {      get => resource;
                                    internal    set => resource = value; // setter is required. Otherwise, MemberPath.setter == null
                                }
    internal static IReadOnlyDictionary<string, Asset<TAsset>> Map => AssetMap;
    
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private             TAsset  resource;   // must not be made readonly. Otherwise, MemberPath.setter == null

    protected override  bool    IsResourceNull()    => resource == null;
    internal  override  Type    GetAssetType()      => typeof(TAsset);

    private static              IAssetLoader<TAsset>                _loader;
    private static readonly     Dictionary<string, Asset<TAsset>>   AssetMap = new();


    protected Asset(string path, AssetSchema assetSchema) : base(path, assetSchema) { }

    
    internal static void RegisterLoader(IAssetLoader<TAsset> loader) {
        _loader = loader;
    }
    
    public static Asset<TAsset> Get(string path)
    {
        if (string.IsNullOrEmpty(path)) {
            return null;
        }
        if (AssetMap.TryGetValue(path, out var asset)) {
            return asset;
        }
        var assetSchema = GetAssetSchema(path);
        asset = new Asset<TAsset>(path, assetSchema);
        switch (assetSchema) {
            case AssetSchema.RES:
                LoadFileAsset(asset);
                break;
            case AssetSchema.RT:
                break;
            default:
                asset.error = AssetError.INVALID_SCHEMA;
                break;
        }
        AssetMap.Add(path, asset);
        return asset;
    }
    
    private static void LoadFileAsset(Asset<TAsset> asset)
    {
        if (!asset.GetFilePath(out var filePath)) {
            asset.error         = AssetError.FILE_NOT_FOUND;
            asset.errorMessage  = $"path: {filePath}";
            return;
        }
        if (_loader == null) {
            asset.error = AssetError.MISSING_ASSET_LOADER;
            return;
        }
        try {
            asset.resource = _loader.LoadAsset(filePath, asset.schema);
        } catch(Exception exception) {
            asset.error         = AssetError.LOAD_ERROR;
            asset.errorMessage  = GetExceptionError(filePath, exception);
        }
    }
    
    public static Asset<TAsset> Set(string path, TAsset resource)
    {
        var assetSchema = GetAssetSchema(path);
        var asset = new Asset<TAsset>(path, assetSchema);
        switch (assetSchema) {
            case AssetSchema.RES:
            case AssetSchema.RT:
                asset.resource = resource;
                break;
            default:
                asset.error = AssetError.INVALID_SCHEMA;
                break;
        }
        AssetMap.Add(path, asset);
        return asset;
    }
}
