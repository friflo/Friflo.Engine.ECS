using System;
using System.IO;
using System.Text;

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.Runtime;

public enum AssetSchema
{
    /// <summary>Missing or unknown schema type</summary>
    NONE,

    /// <summary><c>res://</c> Refers to resources within the project directory.</summary>
    RES,
    /// <summary><c>rt://</c> Refers to resources created at runtime.</summary>
    RT,
}

public enum AssetError
{
    NONE,
    INVALID_SCHEMA,
    MISSING_ASSET_LOADER,
    FILE_NOT_FOUND,
    LOAD_ERROR,
    ASSET_NULL,
}

/// <summary>
/// <b>API in Friflo.Engine.Runtime is WIP and may change</b><br/>
/// </summary>
public abstract class AbstractAsset
{
    public  readonly    string      path;
    public  readonly    AssetSchema schema;
    
    public override     string      ToString()  => GetString();
    
    internal            AssetError  error;
    protected           string      errorMessage;
    
    protected abstract  bool        IsResourceNull();
    internal  abstract  Type        GetAssetType();
    
    internal AbstractAsset(string path, AssetSchema assetSchema) {
        this.path   = path;
        schema      = assetSchema;
        error       = AssetError.NONE;
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append(path);
        switch (schema) {
            case AssetSchema.NONE:
                sb.Append(" - Missing or unknown asset type");
                break;
            default:
                switch (error) {
                    case AssetError.MISSING_ASSET_LOADER:
                        sb.Append(" - Missing asset loader");
                        break;
                    case AssetError.INVALID_SCHEMA:
                        sb.Append(" - Invalid schema");
                        break;
                    case AssetError.NONE:
                        if (IsResourceNull()) {
                            sb.Append(" - Asset not found");
                        }
                        break;
                }
                break;
        }
        return sb.ToString();
    }
    
    private  const string PrefixRes  = "res://";
    private  const string PrefixRt   = "rt://";
    // list of potential prefixes:
    //  res://      Refers to resources within the project directory.
    //  user://     Points to a writable directory for storing user data.
    //  file://     Used for accessing absolute file paths on the system.
    //  http://     Allows fetching resources from the web.
    //  https:// 
    //  mods://     A custom prefix that can be mapped to res://mods/
    
    internal static AssetSchema GetAssetSchema(string path)
    {
        int schemaIndex = path.IndexOf("://", StringComparison.InvariantCulture);
        if (schemaIndex == -1) {
            return AssetSchema.NONE;
        }
        if (path.StartsWith(PrefixRes)) {
            return AssetSchema.RES;
        }
        if (path.StartsWith(PrefixRt)) {
            return AssetSchema.RT;
        }
        return AssetSchema.NONE;
    }
    
    public static string GetErrorCode(AbstractAsset value)
    {
        if (value == null) {
            return "n";
        }
        switch (value.error) {
            case AssetError.MISSING_ASSET_LOADER:
                return "MAL";
            case AssetError.INVALID_SCHEMA:
                return "IS";
            case AssetError.FILE_NOT_FOUND:
                return "FNF";
            case AssetError.LOAD_ERROR:
                return "LE";
        }
        if (value.IsResourceNull()) {
            return "V=n";
        }
        return "";
    }
    
    public ReadOnlySpan<char> GetDirectoryPath()
    {
        if (error != AssetError.NONE) {
            var sb = new StringBuilder();
            AppendTooltipError(GetAssetType(), this, path, sb);
            throw new FileNotFoundException(sb.ToString());
        }
        int schemaIndex = path.IndexOf("://", StringComparison.InvariantCulture) + "://".Length;
        var folder = path.AsSpan().Slice(schemaIndex, path.Length - schemaIndex);
        folder = Path.GetDirectoryName(folder);
        return path.AsSpan().Slice(0, schemaIndex + folder.Length);
    }
    
    public static void AppendTooltipError(Type assetType, AbstractAsset asset, string memberPath, StringBuilder stringBuilder)
    {
        var typeName = assetType.Name;
        if (asset == null) {
            stringBuilder.Append($"{memberPath} = null");
            return;
        }
        switch (asset.error) {
            case AssetError.MISSING_ASSET_LOADER:
                stringBuilder.Append($"Asset<{typeName}> - Missing asset loader");
                return;
            case AssetError.INVALID_SCHEMA:
                stringBuilder.Append($"Invalid schema\nuse: res:// rt://");
                return;
            case AssetError.FILE_NOT_FOUND:
                stringBuilder.Append($"{typeName} - File not found\n{asset.errorMessage}");
                return;
            case AssetError.LOAD_ERROR:
                stringBuilder.Append($"{typeName} - Load Error\n{asset.errorMessage}");
                return;
        }
        if (asset.IsResourceNull()) {
            stringBuilder.Append($"{memberPath}.Value = null");
            return;    
        }
    }
    
    internal bool GetFilePath(out string filePath)
    {
        var assetPath   = path.Substring(PrefixRes.Length);
        filePath        = Path.Combine(AssetDatabase.AssetFolder, assetPath);
        return File.Exists(filePath);
    }
    
    internal static string GetExceptionError(string path, Exception exception)
    {
        var type = exception.GetType();
        return $"path: {path}\n{type.Namespace}.{type.Name} :\n{exception.Message}";
    }
}
