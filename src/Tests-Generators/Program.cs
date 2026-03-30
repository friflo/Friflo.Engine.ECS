using System.Reflection;
using BenchmarkDotNet.Running;

// BenchmarkRunner.Run<Bench_Vector3>();
BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);

// CLI examples
//      dotnet run -c Release
//      dotnet run -c Release --filter *Vector3*
//      dotnet run -c Release --filter *Vector3_MultiplyAdd*
