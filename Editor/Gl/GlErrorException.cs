using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaSilkExample.Gl {
    public class GlErrorException : Exception {
        public GlErrorException(string message) : base (message){ }

        public static void ThrowIfError(GL Gl) {
            GLEnum error = Gl.GetError();
            if (error != GLEnum.NoError) {
                throw new GlErrorException(error.ToString());
            }
        }
    }
}
