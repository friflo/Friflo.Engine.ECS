using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

// BenchmarkRunner.Run<Bench_Vector3>();


ManualConfig customConfig = DefaultConfig.Instance
    .WithOption(ConfigOptions.JoinSummary, true)
//  .WithArtifactsPath(@"../Artifacts")
    .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest))
    .AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory)
//  .WithSummaryStyle(SummaryStyle.Default.WithTimeUnit(TimeUnit.Microsecond))
    .AddDiagnoser(MemoryDiagnoser.Default);                     // adds column: Allocated
/*  .HideColumns(
        "Method", "Error", "StdDev", "Median",                  // removed to reduce noise
        "RatioSD",                                              // added by using: [Benchmark(Baseline = true)]
        "InvocationCount", "IterationCount", "UnrollFactor",    // added by using: [InvocationCount()] & [IterationCount()]
        "Job", "LaunchCount", "WarmupCount",                    // added by using: [IterationSetup] & [IterationCleanup]
        "Gen0", "Gen1", "Gen2", "Alloc Ratio");                 // removing last column "Alloc Ratio" makes Markdown table valid
*/

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args, customConfig);

// CLI examples
//      dotnet run -c Release
//      dotnet run -c Release --job Short
//      dotnet run -c Release --filter *Vector3*
//      dotnet run -c Release --filter *Vector3_MultiplyAdd*
