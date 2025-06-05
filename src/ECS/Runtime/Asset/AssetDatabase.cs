// ReSharper disable once CheckNamespace

using System.IO;

namespace Friflo.Engine.Assets;

/// <summary>
/// <b>API in Friflo.Engine.Runtime is WIP and may change</b><br/>
/// </summary>
public static class AssetDatabase
{
    public  static  string  Folder { get; private set; } = Directory.GetCurrentDirectory();

    public static void SetAssetFolder(string folder) {
        folder = Path.GetFullPath(folder);
        Folder = folder;
    }
    
    public static string ToAssetPath(string path) {
        if (!path.StartsWith(Folder)) {
            return null;
        }
        return $"res://{path.Substring(Folder.Length + 1)}";
    }
    
    public static string FromAssetPath(string path) {
        if (!path.StartsWith("res://")) {
            return null;
        }
        return $"{Folder}/{path.Substring("res://".Length)}";
    }
    
    public static void RegisterAssetLoader<TAsset>(IAssetLoader<TAsset> loader) {
        Asset<TAsset>.RegisterLoader(loader);
    }
    
    public static void RegisterInstanceFactory<TInstance>(IInstanceFactory<TInstance> factory) {
        Handle<TInstance>.RegisterFactory(factory);
    }
}