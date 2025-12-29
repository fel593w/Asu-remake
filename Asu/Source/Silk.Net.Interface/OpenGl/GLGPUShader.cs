using Modedlus.Systems.RenderInterface;

namespace Silk.Net.Interface.OpenGl;

public class GLGPUShader : GLGPUObjectBase, IDisposable, GPUShader
{
    public Shader source { get; private set; }
    
    public void Dispose()
    {
        // Dispose logic here
    }
}