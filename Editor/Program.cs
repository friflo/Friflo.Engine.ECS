// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Friflo.Editor.UI;

// ReSharper disable ConvertToLocalFunction
namespace Friflo.Editor;

public static class Program
{
    internal static Stopwatch startTime;
    
    
    [STAThread]
    public static void Main(string[] args)
    {
		startTime = new Stopwatch();
        startTime.Start();
        /*
        var graphicsClosed      = new ManualResetEvent(false);
        var graphicsWindow      = OpenGLTest.Init(args);
        graphicsWindow.Position = new Vector2D<int>(1500, 500);
        graphicsWindow.Size     = new Vector2D<int>(1000, 1000);
        var loop                = new OpenGLTest.EventLoop();
        var thread = new Thread(() => {
            graphicsWindow.Initialize();
            loop.RunEventLoop(graphicsWindow);
            graphicsWindow.Dispose();
            graphicsClosed.Set();
        });
        thread.Start();
        */
        
        // var editor = new Editor();
        // editor.Init(args).Wait();
        
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        AppBuilder builder = BuildAvaloniaApp();

        builder.StartWithClassicDesktopLifetime(args);

        // loop.Stop();
        // graphicsClosed.WaitOne();

        // editor.Run();
        // editor.Shutdown();
    }
    
    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        Editor          editor              = new Editor();
        Func<Window>    createMainWindow    = () => new MainWindow { Editor = editor };
        AppEvents.Init(editor, createMainWindow);

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            // Silk.NET / OpenGL integration from: https://github.com/kekekeks/Avalonia-Silk.NET-Example
            // .With(new Win32PlatformOptions { UseWgl = true })  used in Avalonia-Silk.NET-Example
            // https://github.com/kekekeks/Avalonia-Silk.NET-Example/blob/cbf69a79e20e340084f17ef04867411fe9a3876b/AvaloniaSilkExample/Program.cs#L21
            // was removed in Avalonia: SHA-1: 29d3c7670be216f704f001ee6e28efc0adbe2e83   - Restructure Win32PlatformOptions options
            .With(new Win32PlatformOptions {
                RenderingMode = new [] { Win32RenderingMode.Wgl },
                OverlayPopups = true    // Enable transparent rounded corners for MenuFlyout. Otherwise use style: editorMenuFlyout 
            })
            .With(new MacOSPlatformOptions())
            .WithInterFont()
            .LogToTrace();
    }
}