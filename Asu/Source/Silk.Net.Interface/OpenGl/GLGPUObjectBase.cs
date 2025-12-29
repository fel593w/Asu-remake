using Silk.NET.OpenGL;

namespace Silk.Net.Interface.OpenGl;

public abstract class GLGPUObjectBase
{
    public GL OpenGL => Interface.OpenGL;
    public OpenGLRenderInterface Interface;
}