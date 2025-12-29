namespace Modedlus.Systems.RenderInterface;

public interface Texture
{
    public byte[] PixelData { get; set; }
    public int Width { get; }
    public int Height { get; }

}