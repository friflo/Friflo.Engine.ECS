using System;
using System.IO;
using Friflo.Engine.Runtime;
using Friflo.Json.Fliox;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable UnusedVariable
// ReSharper disable AccessToModifiedClosure
// ReSharper disable UnusedParameter.Local
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS.Relations {

public static class Test_Asset
{
    private class StringLoader : IAssetLoader<string>, IInstanceFactory<string>
    {
        public string LoadAsset(string path, AssetSchema assetSchema) {
            if (path.Contains("load_error")) {
                throw new InvalidOperationException();
            }
            return File.ReadAllText(path);
        }
        
        public string CreateInstance(string path, out AbstractAsset asset) {
            var typedAsset = Asset<string>.Get(path);
            asset = typedAsset;
            return typedAsset?.Resource;
        }
    
        public AbstractAsset GetAsset (string path) {
            return Asset<string>.Get(path);
        }
    }
    
    private static void RegisterAssetLoaders() {
        var folder = Directory.GetCurrentDirectory() + "../../../../";
        AssetDatabase.SetAssetFolder(folder);
        
        var stringLoader = new StringLoader();
        AssetDatabase.RegisterAssetLoader(stringLoader);
        AssetDatabase.RegisterInstanceFactory(stringLoader);
    }
    
    [Test]
    public static void Test_Asset_Get()
    {
        RegisterAssetLoaders();
        var asset = Asset<string>.Get("res://assets/string_asset.txt");
        AreEqual("Hello Asset!",                    asset.Resource);
        AreEqual("res://assets/string_asset.txt",   asset.path);
        AreEqual(AssetSchema.RES,                   asset.schema);
        AreEqual("res://assets/string_asset.txt",   asset.ToString());
        AreEqual("res://assets",                    asset.GetDirectoryPath().ToString());
    }
    
    [Test]
    public static void Test_Asset_null()
    {
        RegisterAssetLoaders();
        var asset = Asset<string>.Get(null);
        IsNull(asset);
    }
    
    [Test]
    public static void Test_Asset_load_error()
    {
        RegisterAssetLoaders();
        var asset = Asset<string>.Get("res://assets/load_error.txt");
        IsNull(asset.Resource);
        AreEqual("res://assets/load_error.txt",     asset.path);
        AreEqual(AssetSchema.RES,                   asset.schema);
        AreEqual(AssetError.LOAD_ERROR,             asset.Error);
        
        AreEqual("res://assets/load_error.txt",     asset.ToString());
    }
    
    [Test]
    public static void Test_Asset_errors()
    {
        RegisterAssetLoaders();
        {
            var asset = Asset<string>.Get("xxx://assets/foo");
            IsNull(asset.Resource);
            AreEqual("xxx://assets/foo",                                    asset.path);
            AreEqual(AssetSchema.NONE,                                      asset.schema);
            AreEqual(AssetError.INVALID_SCHEMA,                             asset.Error);
            AreEqual("IS",                                                  AbstractAsset.GetErrorCode(asset));
            AreEqual("xxx://assets/foo - Missing or unknown asset type",    asset.ToString());
        } {
            var asset = Asset<string>.Get("res://not_found");
            IsNull(asset.Resource);
            AreEqual("res://not_found",         asset.path);
            AreEqual(AssetSchema.RES,           asset.schema);
            AreEqual(AssetError.FILE_NOT_FOUND, asset.Error);
            AreEqual("FNF",                     AbstractAsset.GetErrorCode(asset));
            AreEqual("res://not_found",         asset.ToString());
        }
    }
    
    [Test]
    public static void Test_Asset_CreateInstance()
    {
        RegisterAssetLoaders();
        var handle = Handle<string>.CreateInstance("res://assets/string_asset.txt");
        AreEqual("Hello Asset!",                    handle.instance);
        AreEqual("res://assets/string_asset.txt",   handle.path);
        AreEqual("res://assets/string_asset.txt",   handle.ToString());
    }
    
    [Test]
    public static void Test_Asset_runtime()
    {
        Asset<string>.Set("rt://test_string", "foo");
        var asset = Asset<string>.Get("rt://test_string");
        AreEqual("foo",                     asset.Resource);
        AreEqual("rt://test_string",        asset.path);
        AreEqual(AssetSchema.RT,            asset.schema);
        AreEqual(AssetError.NONE,           asset.Error);
        AreEqual("",                        AbstractAsset.GetErrorCode(asset));
        AreEqual("rt://test_string",        asset.ToString());
    }
    
    class TestAsset
    {
        public  Asset<string>  asset;
    }
    
    [Test]
    public static void Test_Asset_serialize_asset()
    {
        RegisterAssetLoaders();
        var json = "{\"asset\":\"res://assets/string_asset.txt\"}";
        {
            var test = new TestAsset {
                asset  = Asset<string>.Get("res://assets/string_asset.txt"),
            };
            var jsonAsset = JsonSerializer.Serialize(test);
            AreEqual(json, jsonAsset);
        }
        {
            var test = JsonSerializer.Deserialize<TestAsset>(json);
            AreEqual("res://assets/string_asset.txt", test.asset.path);
        }
        
        var jsonNull = "{}";
        {
            var test = new TestAsset {
                asset  = null,
            };
            var jsonAsset = JsonSerializer.Serialize(test);
            AreEqual(jsonNull, jsonAsset);
        }
        {
            var test = JsonSerializer.Deserialize<TestAsset>(jsonNull);
            AreEqual(null, test.asset);
        }
    }
    
    class TestHandle
    {
        public  Handle<string>  handle;
    }
    
    // [Test]
    public static void Test_Asset_serialize_handle()
    {
        RegisterAssetLoaders();
        var json = "{\"handle\":\"res://assets/string_asset.txt\"}";
        {
            var test = new TestHandle {
                handle  = Handle<string>.CreateInstance("res://assets/string_asset.txt"),
            };
            var jsonAsset = JsonSerializer.Serialize(test);
            AreEqual(json, jsonAsset);
        }
        {
            var test = JsonSerializer.Deserialize<TestHandle>(json);
            AreEqual("res://assets/string_asset.txt", test.handle.path);
        }
        
    }
}

}
