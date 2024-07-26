using System;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Friflo.Engine.OpenGL;

// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable InconsistentNaming
namespace Friflo.Editor.OpenGL
{
    public class SilkOpenGLControl : OpenGlControlBase
    {
        internal            Action      OpenGlReady;
        private             DrawTest    test;

        protected override void OnOpenGlInit(GlInterface gl)
        {
            base.OnOpenGlInit(gl);
            // Console.WriteLine($"--- SilkOpenGLControl.OnOpenGlInit() - startup {Program.startTime.ElapsedMilliseconds} ms");
            Func<string,nint> procAddress = gl.GetProcAddress;
            test = new DrawTest();
            test.OpenGlInit(procAddress);

            Dispatcher.UIThread.Post(OpenGlReady, DispatcherPriority.ApplicationIdle);
        }

        protected override void OnOpenGlDeinit(GlInterface gl)
        {
            test.OpenGlDeinit();
            base.OnOpenGlDeinit(gl);
        }

        protected override void OnOpenGlRender(GlInterface gl, int fb)
        {
            var width   = Bounds.Width;
            var height  = Bounds.Height;
            test.OpenGlRender(width, height);

            Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
        }
    }
}