using System;
using Silk.NET.OpenGL;

namespace Friflo.Engine.OpenGL
{
    public class GlErrorException : Exception
    {
        public GlErrorException(string message) : base (message){ }

        public static void ThrowIfError(GL gl) {
            GLEnum error = gl.GetError();
            if (error != GLEnum.NoError) {
                throw new GlErrorException(error.ToString());
            }
        }
    }
}
