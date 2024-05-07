// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using static Friflo.Engine.ECS.Systems.SystemExtensions;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems
{
    public struct SystemPerf
    {
        /// <remarks>Can be 0 in case execution time was below <see cref="Stopwatch.Frequency"/> precision.</remarks>
                        public  int     UpdateCount => updateCount;
                        public  float   LastMs      => lastTicks >= 0 ? (float)(lastTicks * StopwatchPeriodMs) : -1;
                        public  float   SumMs       => (float)(sumTicks * StopwatchPeriodMs);
        [Browse(Never)] public  long    LastTicks   => lastTicks;
        [Browse(Never)] public  long    SumTicks    => sumTicks;

        public override string  ToString()  => $"updates: {UpdateCount}  last: {LastMs:0.###} ms  sum: {SumMs:0.###} ms";
        
        public          float   LastAvgMs(int count) => GetLastAvgMs(count);

        [Browse(Never)] internal            int     updateCount;
        [Browse(Never)] internal            long    lastTicks;
        [Browse(Never)] internal            long    sumTicks;
                        internal readonly   long[]  history;
        
        internal SystemPerf(long[] history) {
            this.history    = history;
            lastTicks       = -1;
        }
        
        private float GetLastAvgMs(int count)
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
    }
    
    internal sealed class View
    {
        public  Tick                    Tick        => system.Tick;
        public  int                     Id          => system.Id;
        public  bool                    Enabled     => system.Enabled;
        public  string                  Name        => system.Name;
        public  SystemRoot              SystemRoot  => system.SystemRoot;
        public  SystemGroup             ParentGroup => system.ParentGroup;
        public  SystemPerf              Perf        => system.perf;

        public override string          ToString()  => $"Enabled: {Enabled}  Id: {Id}";

        [Browse(Never)] private readonly BaseSystem   system;
        
        internal View(BaseSystem system) {
            this.system = system;
        }
    }
}