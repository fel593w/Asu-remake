using Modedlus.Systems.RenderInterface;
using Silk.Net.Interface.OpenGl;

namespace Silk.Net.Interface.OpenGl;

public class GLGPUMesh : GLGPUComonMeshObject, GPUMesh
{
    public Mesh source { get; private set; }


}