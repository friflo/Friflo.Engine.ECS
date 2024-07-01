// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static Friflo.Engine.ECS.Systems.SystemExtensions;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems;

/// <summary>
/// Provide performance statistics of system execution via the system property <see cref="BaseSystem.Perf"/>.
/// </summary>
public struct SystemPerf
{
#region properties
    /// <summary> Return the number of system executions. </summary>
                    public  int     UpdateCount => updateCount;
    
    /// <summary> Return the duration of the last execution in milliseconds. <br/>
    /// Can be 0 in case execution time was below <see cref="Stopwatch.Frequency"/> precision. </summary>
                    public  float   LastMs      => lastTicks >= 0 ? (float)(lastTicks * StopwatchPeriodMs) : -1;
    
    /// <summary> Return the sum of all execution times in milliseconds. </summary>
                    public  float   SumMs       => (float)(sumTicks * StopwatchPeriodMs);
    
    /// <summary>Return the duration of the last execution in timer ticks. <br/>
    /// Can be 0 in case execution time was below <see cref="Stopwatch.Frequency"/> precision. </summary>
    [Browse(Never)] public  long    LastTicks   => lastTicks;
    
    /// <summary> Return the sum of all execution times in timer ticks. </summary>
    [Browse(Never)] public  long    SumTicks    => sumTicks;
    
    /// <summary> Return the memory allocations of the last execution in bytes. </summary>
    [Browse(Never)] public  long    LastMemory  => lastMemory;
    
    /// <summary> Return the sum of memory allocations of all executions in bytes. </summary>
    [Browse(Never)] public  long    SumMemory   => sumMemory;
    
    public override         string  ToString()  => $"updates: {UpdateCount}  last: {LastMs:0.###} ms  sum: {SumMs:0.###} ms";
    #endregion

#region fields
    [Browse(Never)] internal            int     updateCount;
    [Browse(Never)] internal            long    lastTicks;
    [Browse(Never)] internal            long    sumTicks;
    [Browse(Never)] internal            long    lastMemory;
    [Browse(Never)] internal            long    sumMemory;
                    internal readonly   long[]  history;
    #endregion

#region methods
    internal SystemPerf(long[] history) {
        this.history    = history;
        lastTicks       = -1;
    }
    
    /// <summary>
    /// Return the average duration of the last <paramref name="count"/> executions in milliseconds.
    /// </summary>
    public float LastAvgMs(int count)
    {
        var ticks   = history;
        var length  = ticks.Length;
        count       = Math.Min(updateCount, Math.Min(length, count));
        if (count == 0) {
            return -1;
        }
        var sum     = 0L;
        for (int n = updateCount - count; n < updateCount; n++) {
            sum += ticks[n % length];
        }
        sum /= count;
        return (float)(sum * StopwatchPeriodMs);
    }
    #endregion
}

internal readonly struct PerfResource
{
    internal readonly long time;
    internal readonly long memory;
    
    public PerfResource () {
        time    = Stopwatch.GetTimestamp();
        memory  = GetAllocatedBytes();
    }
    
    [ExcludeFromCodeCoverage]
    internal static long GetAllocatedBytes() {
        return Platform.IsUnityRuntime ? 0 : GC.GetAllocatedBytesForCurrentThread(); 
    }
}

