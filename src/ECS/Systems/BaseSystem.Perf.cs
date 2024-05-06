// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Friflo.Json.Fliox;
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
        public          int     UpdateCount => updateCount;
        public          double  LastMs      => lastTicks >= 0 ? lastTicks * StopwatchPeriodMs : -1;
        public          double  SumMs       => sumTicks * StopwatchPeriodMs;

        public override string  ToString() => $"updates: {UpdateCount} last: {LastMs:0.###} sum: {SumMs:0.###}";

        [Ignore]    [Browse(Never)] internal            int     updateCount;
        [Ignore]    [Browse(Never)] internal            long    lastTicks;
        [Ignore]    [Browse(Never)] internal readonly   long[]  history;
        [Ignore]    [Browse(Never)] internal            long    sumTicks;
        
        internal SystemPerf(long[] history) {
            this.history = history;
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