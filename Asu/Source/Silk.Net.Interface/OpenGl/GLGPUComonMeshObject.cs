namespace Silk.Net.Interface.OpenGl;

public abstract class GLGPUComonMeshObject : GLGPUObjectBase, IDisposable
{

    #region Gpu pointer

    public uint Vao;
    public uint Vbo;
    public uint Ebo;

    #endregion

    #region Information

    /// <summary>
    /// Number of vertices in the mesh
    /// </summary>
    public int VertexCount;

    /// <summary>
    /// Number of indices in the mesh
    /// </summary>
    public int IndexCount;

    #endregion

    #region Dispose 

    private bool disposed = false;

    public void Dispose()
    {
        lock (this) {

        #region checks

        if(disposed)
            return;

        if(GLInterface is not null)
            return;

        if(OpenGL is not null)
            return;

        #endregion

        // Make Shure it does not run twice
        disposed = true;

        #region Dispose GPU Resources

        if(Vao != 0)
        {
            OpenGL.DeleteVertexArray(Vao);
            Vao = 0;
        }

        if(Vbo != 0)
        {
            OpenGL.DeleteBuffer(Vbo);
            Vbo = 0;
        }

        if(Ebo != 0)
        {
            OpenGL.DeleteBuffer(Ebo);
            Ebo = 0;
        }

        #endregion
        }
    }

    #endregion
}