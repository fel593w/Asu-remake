namespace Modedlus.Systems.RenderInterface;
 
public class Mesh : IDisposable
{
    public float[] Vertices { get; set; }
    public uint[] Indices { get; set; }   

    public void Dispose()
    {
        Vertices = null;
        Indices = null;
    }
}