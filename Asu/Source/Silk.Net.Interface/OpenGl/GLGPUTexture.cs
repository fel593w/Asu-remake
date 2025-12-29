
using Modedlus.Systems.RenderInterface;

namespace Silk.Net.Interface.OpenGl;

public class GLGPUTexture : GLGPUObjectBase, IDisposable, GPUTexture
{
    public Texture source { get; private set; }
 
    public void Dispose()
    {
        // Dispose logic here
    }
}