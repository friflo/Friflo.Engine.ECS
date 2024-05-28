// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Local
namespace GameTest;

public static class Program
{
    internal static Stopwatch startTime;
    
    public static void Main(string[] args) => ParallelQuery.Query_ForEach(args);
    
    // public static async Task Main(string[] args) => await RunGame();
    
    private static async Task RunGame()
    {
        startTime = new Stopwatch();
        startTime.Start();
        var game = new Game();
        await game.Init();
    }
}