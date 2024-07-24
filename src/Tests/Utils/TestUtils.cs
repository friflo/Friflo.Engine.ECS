using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Friflo.Engine.ECS;

// ReSharper disable UseMethodAny.2
namespace Tests.Utils {

public static class TestUtils
{
    private static bool IsInUnitTest { get; }
    
    public static string GetBasePath(string folder = "")
    {
#if UNITY_5_3_OR_NEWER
        var baseDir = UnityUtils.GetProjectFolder();
#else
        // remove folder like ".bin/Debug/net6.0" which is added when running unit tests
        var projectFolder   = IsInUnitTest ?  "/../../../" : "/";
        string baseDir      = Directory.GetCurrentDirectory() + projectFolder;
#endif
        baseDir = Path.GetFullPath(baseDir + folder);
        return baseDir;
    }
        
    static TestUtils()
    {
        var testAssemblyName    = "nunit.framework";
        var assemblies          = AppDomain.CurrentDomain.GetAssemblies();
        IsInUnitTest            = assemblies.Any(a => a.FullName!.StartsWith(testAssemblyName));
    }
    
    public static double StopwatchMillis(Stopwatch stopwatch) {
        return stopwatch.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
    }
    
    public static string Debug(this QueryEntities entities)
    {
        if (entities.Count == 0) return "{ }";
        var sb = new StringBuilder();
        sb.Append("{ ");
        foreach (var entity in entities) {
            if (sb.Length > 2) sb.Append(", ");
            sb.Append(entity.Id);
        }
        sb.Append(" }");
        return sb.ToString();
    }
    
    public static string Debug(this ReadOnlySpan<int> entities)
    {
        if (entities.Length == 0) return "{ }";
        var sb = new StringBuilder();
        sb.Append("{ ");
        foreach (var id in entities) {
            if (sb.Length > 2) sb.Append(", ");
            sb.Append(id);
        }
        sb.Append(" }");
        return sb.ToString();
    }
    
    private static readonly double StopwatchPeriodMs = 1 / (Stopwatch.Frequency / 1000d);

    public static long GetTimestamp() {
        return Stopwatch.GetTimestamp();
    }
    
    public static string DurationMs(long start) {
        var duration = (float)((Stopwatch.GetTimestamp() - start) * StopwatchPeriodMs);
        var result = $"{duration,9:0.00}";
        return result.Replace(',', '.');
    }
    
}

}