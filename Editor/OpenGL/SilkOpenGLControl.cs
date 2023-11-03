using System;
using System.Drawing;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Silk.NET.OpenGL;

// ReSharper disable InconsistentNaming
namespace Friflo.Fliox.Editor.OpenGL
{
    public class SilkOpenGLControl : OpenGlControlBase
    {
        internal Action OpenGlReady;
            
        private GL Gl;
        private BufferObject<float> Vbo;
        private BufferObject<uint> Ebo;
        private VertexArrayObject<float, uint> Vao;
        private Shader Shader;

        private static readonly float[] Vertices =
        {
            //X    Y      Z     R  G  B  A
            0.5f,  0.5f, 0.0f, 1, 0, 0, 1,
            0.5f, -0.5f, 0.0f, 0, 0, 0, 1,
            -0.5f, -0.5f, 0.0f, 0, 0, 1, 1,
            -0.5f,  0.5f, 0.5f, 0, 0, 0, 1
        };

        private static readonly uint[] Indices =
        {
            0, 1, 3,
            1, 2, 3
        };



        protected override void OnOpenGlInit(GlInterface gl)
        {
            base.OnOpenGlInit(gl);
            // Console.WriteLine($"--- SilkOpenGLControl.OnOpenGlInit() - startup {Program.startTime.ElapsedMilliseconds} ms");
            Gl = GL.GetApi(gl.GetProcAddress);
            

            //Instantiating our new abstractions
            Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
            Vbo = new BufferObject<float>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
            Vao = new VertexArrayObject<float, uint>(Gl, Vbo, Ebo);

            //Telling the VAO object how to lay out the attribute pointers
            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 7, 0);
            Vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 7, 3);

            Shader = new Shader(Gl, "OpenGL/Shader/shader.vert", "OpenGL/Shader/shader.frag");
            Dispatcher.UIThread.Post(OpenGlReady, DispatcherPriority.ApplicationIdle);
        }
       

        protected override void OnOpenGlDeinit(GlInterface gl)
        {
            Vbo.Dispose();
            Ebo.Dispose();
            Vao.Dispose();
            Shader.Dispose();
            base.OnOpenGlDeinit(gl);
        }

        protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
        {
            Gl.ClearColor(Color.Gray);
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Gl.Enable(EnableCap.DepthTest);
            Gl.Viewport(0,0, (uint)Bounds.Width, (uint)Bounds.Height);
            
            Ebo.Bind();
            Vbo.Bind();
            Vao.Bind();
            Shader.Use();
            Shader.SetUniform("uBlue", (float) Math.Sin(DateTime.Now.Millisecond / 1000f * Math.PI));

            Gl.DrawElements(PrimitiveType.Triangles, (uint) Indices.Length, DrawElementsType.UnsignedInt, null);
            Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
        }
    }
}