// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable MergeIntoPattern
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// A <see cref="QueryJob"/> enables <see cref="JobExecution.Parallel"/> query execution using multiple threads
/// to reduce execution time of large queries.<br/>
/// They are created by the <c>ArchetypeQuery.ForEach()</c> methods.
/// </summary>
/// <remarks>
/// To execute a query job <see cref="JobExecution.Sequential"/> use the <see cref="Run"/> method.<br/>
/// To execute a query job <see cref="JobExecution.Parallel"/> use the <see cref="RunParallel"/> method.
/// </remarks>
public abstract class QueryJob
{
#region public properties
    /// <summary> The job runner used to execute a query <see cref="JobExecution.Parallel"/>. </summary>
    public      ParallelJobRunner   JobRunner               { get => GetRunner(); set => SetRunner(value);      }
    
    /// <summary>
    /// The minimum number of <see cref="Chunk{T}"/> components per thread required to execute a query <see cref="JobExecution.Parallel"/>.<br/>
    /// Default: 1000.
    /// </summary>
    /// <remarks>
    /// Parallel query execution adds an overhead of 1 to 2 micro seconds per query for thread synchronization.<br/>
    /// Execution of a simple computation like <c>health.value++</c> on a single component takes 0.5 to 1 nano seconds.<br/>
    /// <br/>
    /// E.g. processing a chunk with 100 components will take 50 to 100 nano seconds.<br/>
    /// So the chunk components are executed <see cref="JobExecution.Sequential"/> to avoid the parallelization overhead.<br/>
    /// <br/>
    /// For more complex computations <see cref="MinParallelChunkLength"/> can be reduced to execute a query
    /// <see cref="JobExecution.Parallel"/> when dealing with a lower number of components.
    /// </remarks>
    public      int                 MinParallelChunkLength  { get => minParallel; set => SetMinParallel(value); }
    #endregion
    
#region internal fields
    [Browse(Never)] internal    ParallelJobRunner   jobRunner;          //  4
    [Browse(Never)] private     int                 minParallel = 1000; //  4
    #endregion
    
    /// <summary>
    /// Execute the query <see cref="JobExecution.Sequential"/>.
    /// </summary>
    public      abstract    void Run();
    /// <summary>Execute the query.
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#parallel-query-job">Example.</a>.<br/>
    /// All chunks having at least <see cref="QueryJob.MinParallelChunkLength"/> * <see cref="ParallelJobRunner.ThreadCount"/>
    /// components are executed <see cref="JobExecution.Parallel"/>. 
    /// </summary>
    /// <remarks>
    /// Requires an <see cref="ParallelJobRunner"/>.<br/>
    /// A runner can be assigned to <see cref="JobRunner"/> or to the <see cref="EntityStore"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     If the <see cref="JobRunner"/> is not set.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     If a nested <see cref="RunParallel"/> is using the same <see cref="JobRunner"/> as the enclosing job. 
    /// </exception>
    public    abstract  void    RunParallel();
    
    /// <summary>
    /// The <see cref="ParallelComponentMultiple"/> is used to align the <see cref="Chunk{T}"/> components length 
    /// of a <see cref="JobExecution.Parallel"/> executed component chunks.
    /// </summary>
    /// <remarks>
    /// This enables vectorization of the components without a remainder loop using<br/>
    /// <see cref="Chunk{T}.AsSpan128{TTo}"/>, <see cref="Chunk{T}.AsSpan256{TTo}"/> or <see cref="Chunk{T}.AsSpan512{TTo}"/>.
    /// </remarks>
    public    abstract  int     ParallelComponentMultiple { get; }

#region methods
    private ParallelJobRunner GetRunner() {
        return jobRunner;
    }
    
    private void  SetRunner(ParallelJobRunner jobRunner) {
        if (jobRunner == null)      throw new ArgumentNullException(nameof(jobRunner));
        if (jobRunner.IsDisposed)   throw new ArgumentException($"{nameof(ParallelJobRunner)} is disposed");
        this.jobRunner = jobRunner;
    }
    
    private void  SetMinParallel(int value) {
        if (value < 1) throw new ArgumentException($"{nameof(MinParallelChunkLength)} must be > 0");
        minParallel = value;
    }
    
    internal static InvalidOperationException JobRunnerIsNullException() {
        return new InvalidOperationException($"{nameof(QueryJob)} requires a {nameof(JobRunner)}");
    }
    
    internal bool ExecuteSequential(int taskCount, int chunkLength) {
        return taskCount <= 1 || (chunkLength + taskCount - 1) / taskCount < minParallel;
    }
    
    /// <remarks>
    /// The return size which is applied to every section <see cref="Chunk{T}"/>.<br/>
    /// <br/>
    /// In case <paramref name="multiple"/> != 0 the returned size ensures the number of bytes required for
    /// section size components is a multiple of 64 bytes.<br/>
    /// This enables vectorization using Vector128, Vector256 or Vector512 without a remainder loop.<br/>
    /// See <see cref="Chunk{T}.AsSpan128{TTo}"/>, <see cref="Chunk{T}.AsSpan256{TTo}"/> an <see cref="Chunk{T}.AsSpan512{TTo}"/>.  
    /// </remarks>
    internal static int GetSectionSize(int chunkLength, int taskCount, int multiple)
    {
        var size = (chunkLength + taskCount - 1) / taskCount;
        if (multiple == 0) {
            return size;
        }
        return ((size + multiple - 1) / multiple) * multiple;
    }
    
    internal static int GetSectionLength(int chunkLength, int start, int sectionSize)
    {
        var remaining = chunkLength - start;
        return remaining < sectionSize ? remaining : sectionSize;
    }

    private static int GreatestCommonDivider(int a, int b)
    {
        // used loop instead of recursive call below
        while (true)
        {
            if (b == 0) {
                return a;
            }
            var a1 = a;
            a = b;
            b = a1 % b;
        }
        /* same as
        if(b == 0) {
            return a;
        }
        return GreatestCommonDivider(b, a % b); */
    }
    
    internal static int LeastComponentMultiple(int a, int b)
    {
        var lcm = LeastCommonMultiple(a, b);
        if (lcm <= ArchetypeUtils.MaxComponentMultiple) {
            return lcm;
        }
        return 0;
    }

    internal static int LeastCommonMultiple(int a, int b)
    {
        if (a == 0 || b == 0) {
            return 0;
        }
        var divider = GreatestCommonDivider(a, b);
        if(a > b) {
            return a / divider * b;
        }
        return b / divider * a;  
    }
    #endregion
}