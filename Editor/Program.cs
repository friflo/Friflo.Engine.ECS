using System;
using System.Threading;
using Avalonia;
using Friflo.Fliox.Editor.OpenGL;
using Friflo.Fliox.Editor.UI;
using Silk.NET.Maths;

namespace Friflo.Fliox.Editor;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var graphicsClosed      = new ManualResetEvent(false);
        var graphicsWindow      = OpenGLTest.Init(args);
        graphicsWindow.Position = new Vector2D<int>(1500, 500);
        graphicsWindow.Size     = new Vector2D<int>(1000, 1000);
        var loop                = new OpenGLTest.EventLoop();
        var thread = new Thread(() => {
            loop.RunEventLoop(graphicsWindow);
            graphicsWindow.Dispose();
            graphicsClosed.Set();
        });
        thread.Start();
        
        var editor = new Editor();
        editor.Init(args).Wait();
        
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        AppBuilder builder = BuildAvaloniaApp();
        builder.StartWithClassicDesktopLifetime(args);

        loop.stop = true;
        graphicsClosed.WaitOne();

        // editor.Run();
        editor.Shutdown();
    }
    
    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}