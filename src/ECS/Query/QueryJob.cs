// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal abstract class QueryJob
{
#region public properties
    internal    ParallelJobRunner   JobRunner               { get => GetRunner(); set => SetRunner(value); }
    internal    int                 MinParallelChunkLength  { get => minParallel; set => SetMinParallel(value); }
    #endregion
    
#region internal fields
    internal    ParallelJobRunner   jobRunner;      //  4
    internal    int                 minParallel;    //  4
    #endregion
    
    internal    abstract    void Run();
    internal    abstract    void RunParallel();

#region methods
    protected QueryJob() {
        jobRunner       = ParallelJobRunner.Default;
        minParallel     = 1000;
    }
    
    private ParallelJobRunner GetRunner() {
        return jobRunner;
    }
    
    private void  SetRunner(ParallelJobRunner value) {
        jobRunner = value ?? ParallelJobRunner.Default;
    }
    
    private void  SetMinParallel(int value) {
        if (value < 1) throw new ArgumentException($"{nameof(MinParallelChunkLength)} must be > 0");
        minParallel = value;
    }
    #endregion
}