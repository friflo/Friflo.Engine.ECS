using System;
using Silk.NET.OpenGL;

namespace Friflo.Engine.OpenGL
{
    public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        private readonly uint handle;
        private readonly GL gl;

        public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
        {
            this.gl = gl;

            handle = this.gl.GenVertexArray();
            Bind();
            vbo.Bind();
            ebo.Bind();
        }

        public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
        {
            gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint) sizeof(TVertexType), (void*) (offSet * sizeof(TVertexType)));
            gl.EnableVertexAttribArray(index);
        }

        public void Bind()
        {
            gl.BindVertexArray(handle);
        }

        public void Dispose()
        {
            gl.DeleteVertexArray(handle);
        }
    }
}
