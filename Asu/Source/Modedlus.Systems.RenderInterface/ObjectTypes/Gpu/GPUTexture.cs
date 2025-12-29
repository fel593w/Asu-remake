namespace Modedlus.Systems.RenderInterface;

public interface GPUTexture
{
    /// <summary>
    /// Stores the texture witch is loaded form
    /// WARNING it is not garanteed that this texture is the same as the one in the gpu memory
    /// </summary>
    public Texture source { get; }
}