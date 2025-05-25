// ReSharper disable once CheckNamespace

using System.IO;

namespace Friflo.Engine.Runtime;

/// <summary>
/// <b>API in Friflo.Engine.Runtime is WIP and may change</b><br/>
/// </summary>
public static class AssetDatabase
{
    internal static string AssetFolder = Directory.GetCurrentDirectory();
    
    public static void SetAssetFolder(string folder) {
        folder = Path.GetFullPath(folder);
        AssetFolder = folder;
    }
    
    public static void RegisterAssetLoader<TAsset>(IAssetLoader<TAsset> loader) {
        Asset<TAsset>.RegisterLoader(loader);
    }
    
    public static void RegisterInstanceFactory<TInstance>(IInstanceFactory<TInstance> factory) {
        Handle<TInstance>.RegisterFactory(factory);
    }
}