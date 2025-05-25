
using Friflo.Json.Fliox;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Runtime;


/// <summary>
/// A factory used to create in instances of an <see cref="Asset{TAsset}"/>.<br/>
/// Recommendation: The <typeparamref name="TInstance"/> type should be the same as the type used in <see cref="Asset{TAsset}"/>. 
/// </summary>
/// <typeparam name="TInstance">Instance type of an <see cref="Asset{TAsset}"/></typeparam>
public interface IInstanceFactory<out TInstance>
{
    /// <summary>
    /// Create an instance identified by the given path.<br/>
    /// </summary>
    TInstance       CreateInstance(string path, out AbstractAsset asset);
    AbstractAsset   GetAsset      (string path);
}

/// <summary>
/// <b>API in Friflo.Engine.Runtime is WIP and may change</b><br/>
/// </summary>
[TypeMapper(typeof(HandleMatcher))]
public readonly struct Handle<TInstance>
{
    public readonly TInstance   instance;
    public readonly string      path;

    public override string      ToString() => path ?? "null";

    private static IInstanceFactory<TInstance> _factory;
    
    private Handle(TInstance instance, string path) {
        this.instance   = instance;
        this.path       = path;
    }
    
    internal static void RegisterFactory(IInstanceFactory<TInstance> factory) {
        _factory = factory;
    }
    
    public static Handle<TInstance> CreateInstance(string path)
    {
        if (string.IsNullOrEmpty(path)) {
            return default;
        }
        var instance = _factory!.CreateInstance(path, out var asset);
        return new Handle<TInstance>(instance, asset?.path ?? path);
    }
    
    public static AbstractAsset GetAsset(string path) => _factory!.GetAsset(path);
}