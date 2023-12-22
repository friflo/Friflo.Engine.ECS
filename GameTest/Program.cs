// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Threading.Tasks;

namespace GameTest;

public static class Program
{
    internal static Stopwatch startTime;
    
    public static async Task Main(string[] args)
    {
        startTime = new Stopwatch();
        startTime.Start();
        var game = new Game();
        await game.Init();
    }
}