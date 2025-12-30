using Silk.NET.OpenGL;

namespace Silk.Net.Interface.OpenGl;

public abstract class GLGPUObjectBase
{
    public GL OpenGL => GLInterface.OpenGL;
    public OpenGLRenderInterface GLInterface;
}