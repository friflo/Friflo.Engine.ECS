using System;
using System.Reflection;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Tests.Generators.Benchmark;

[AttributeUsage(AttributeTargets.Method)]
public class CommentAttribute : Attribute
{
    public string Comment { get; }
    public CommentAttribute(string comment) => Comment = comment;
}

public class CommentColumn : IColumn
{
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public string Id => nameof(CommentColumn);
    public string ColumnName => "Comment"; // The header name
    public bool IsAvailable(Summary summary) => true;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Custom;
    public int PriorityInCategory => 0;
    public bool IsNumeric => false;
    public UnitType UnitType => UnitType.Dimensionless;
    public string Legend => "Custom comment for this benchmark";

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        // Look for our custom attribute on the method being benchmarked
        var attribute = benchmarkCase.Descriptor.WorkloadMethod
            .GetCustomAttribute<CommentAttribute>();
        
        return attribute?.Comment ?? "-";
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
    public override string ToString() => ColumnName;
}