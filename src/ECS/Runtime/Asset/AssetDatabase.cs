// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Runtime;

/// <summary>
/// <b>API in Friflo.Engine.Runtime is WIP and may change</b><br/>
/// </summary>
public static class AssetDatabase
{
    public static void RegisterAssetLoader<TAsset>(IAssetLoader<TAsset> loader) {
        Asset<TAsset>.RegisterLoader(loader);
    }
    
    public static void RegisterInstanceFactory<TInstance>(IInstanceFactory<TInstance> factory) {
        Handle<TInstance>.RegisterFactory(factory);
    }
}