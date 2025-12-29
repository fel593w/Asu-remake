namespace Modedlus.Systems.RenderInterface;

public interface GPUDynamicMesh
{
    /// <summary>
    /// Stores the mesh witch is loaded form
    /// WARNING it is not garanteed that this Mesh is the same as the one in the gpu memory
    /// </summary>
    public void UpdateMeshData(Mesh mesh);
}