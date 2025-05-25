using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Friflo.Engine.ECS;
using Friflo.Engine.Runtime;
using NUnit.Framework;
using Tests.Utils;
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
    private class StringLoader : IAssetLoader<string>
    {
        public string LoadAsset(string path, AssetSchema assetSchema) {
            return File.ReadAllText(path);
        }
    }
    
    private static void RegisterAssetLoaders() {
        var folder = Directory.GetCurrentDirectory() + "../../../../";
        AssetDatabase.SetAssetFolder(folder);
        AssetDatabase.RegisterAssetLoader(new StringLoader());
    }
    
    [Test]
    public static void Test_Asset_Get()
    {
        RegisterAssetLoaders();
        var asset = Asset<string>.Get("res://assets/string_asset.txt");
        AreEqual("Hello Asset!",                    asset.Resource);
        AreEqual("res://assets/string_asset.txt",   asset.path);
        AreEqual(AssetSchema.RES,                   asset.schema);
    }
    
    [Test]
    public static void Test_Asset_errors()
    {
        RegisterAssetLoaders();
        {
            var asset = Asset<string>.Get("xxx://assets/foo");
            IsNull(asset.Resource);
            AreEqual("xxx://assets/foo",        asset.path);
            AreEqual(AssetSchema.NONE,          asset.schema);
            AreEqual(AssetError.INVALID_SCHEMA, asset.Error);
            AreEqual("IS",                      AbstractAsset.GetErrorCode(asset));
        } {
            var asset = Asset<string>.Get("res://not_found");
            IsNull(asset.Resource);
            AreEqual("res://not_found",         asset.path);
            AreEqual(AssetSchema.RES,           asset.schema);
            AreEqual(AssetError.FILE_NOT_FOUND, asset.Error);
            AreEqual("FNF",                     AbstractAsset.GetErrorCode(asset));
        }
    }
}

}
