// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable MergeIntoPattern
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public abstract class QueryJob
{
#region public properties
    /// <summary> The job runner used to execute a query <see cref="JobExecution.Parallel"/>. </summary>
    public      ParallelJobRunner   JobRunner               { get => GetRunner(); set => SetRunner(value);      }
    
    /// <summary>
    /// The minimum number of <see cref="Chunk{T}"/> components per thread required to execute the query <see cref="JobExecution.Parallel"/>.
    /// Default: 1000.
    /// </summary>
    /// <remarks>
    /// Parallel query execution adds an overhead of 1 to 2 micro seconds per query for thread synchronization.<br/>
    /// Execution of a simple computation like <c>health.value++</c> on a single component takes 0.5 to 1 nano seconds.<br/>
    /// <br/>
    /// E.g. processing a chunk with 100 components will take 50 to 100 nano seconds.<br/>
    /// So the chunk components are executed <see cref="JobExecution.Sequential"/> to avoid the parallelization overhead.<br/>
    /// <br/>
    /// For more complex computations <see cref="MinParallelChunkLength"/> can be reduces to execute a query
    /// <see cref="JobExecution.Parallel"/> when dealing with a lower number of components.
    /// </remarks>
    public      int                 MinParallelChunkLength  { get => minParallel; set => SetMinParallel(value); }
    #endregion
    
#region internal fields
    internal    ParallelJobRunner   jobRunner;          //  4
    private     int                 minParallel = 1000; //  4
    #endregion
    
    public      abstract    void Run();
    /// <summary>Executes the query job.</summary>
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
    public    abstract    void RunParallel();

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
    /// In case <paramref name="align512"/> != 0 the returned size ensures the number of bytes required for
    /// section size components is a multiple of 64 bytes.<br/>
    /// This enables vectorization using Vector128, Vector256 or Vector512 without a remainder loop.<br/>
    /// See <see cref="Chunk{T}.AsSpan128{TTo}"/>, <see cref="Chunk{T}.AsSpan256{TTo}"/> an <see cref="Chunk{T}.AsSpan512{TTo}"/>.  
    /// </remarks>
    internal static int GetSectionSize512(int chunkLength, int taskCount, int align512)
    {
        var size = (chunkLength + taskCount - 1) / taskCount;
        if (align512 == 0) {
            return size;
        }
        return ((size + align512 - 1) / align512) * align512;
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

    internal static int LeastCommonMultiple(int a, int b)
    {
        if(a > b) {
            return a / GreatestCommonDivider(a, b) * b;
        }
        return b / GreatestCommonDivider(a, b) * a;  
    }
    #endregion
}