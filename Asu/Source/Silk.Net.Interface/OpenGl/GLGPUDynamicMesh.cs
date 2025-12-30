using Modedlus.Systems.RenderInterface;
using Silk.Net.Interface.OpenGl;

namespace Silk.Net.Interface.OpenGl;

public class GLGPUDynamicMesh : GLGPUComonMeshObject, GPUDynamicMesh, GPUMesh
{
    public Mesh source { get; private set; }

    public void UpdateMeshData(Mesh mesh)
    {
        // Update the GPU mesh data with the new mesh data
        //this.source = mesh;
        // Additional logic to update the GPU buffers would go here
        throw new NotImplementedException();
    }

    public GLGPUDynamicMesh(OpenGLRenderInterface GLInterface, Mesh mesh)
    {
        throw new NotImplementedException();
        this.GLInterface = GLInterface;
    }


}