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
    public      ParallelJobRunner   JobRunner               { get => GetRunner(); set => SetRunner(value); }
    public      int                 MinParallelChunkLength  { get => minParallel; set => SetMinParallel(value); }
    #endregion
    
#region internal fields
    internal    ParallelJobRunner   jobRunner;          //  4
    internal    int                 minParallel = 1000; //  4
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

    internal static int GetSectionSize(int chunkLength, int taskCount, int align512)
    {
        var size = (chunkLength + taskCount - 1) / taskCount;
        if (align512 == 0) {
            return size;
        }
        return ((size + align512 - 1) / align512) * align512;
    }
    
    #endregion
}