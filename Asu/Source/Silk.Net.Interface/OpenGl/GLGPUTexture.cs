using System.Net;
using Modedlus.Systems.RenderInterface;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Silk.Net.Interface.OpenGl;

public class GLGPUTexture : GLGPUObjectBase, IDisposable, GPUTexture
{
    public Modedlus.Systems.RenderInterface.Texture source { get; private set; }

    private uint Handel;

    private bool loaded = false;
 
    public unsafe GLGPUTexture(OpenGLRenderInterface GLInterface, Modedlus.Systems.RenderInterface.Texture texture)
    {
        if(loaded is true)
        {
            throw new Exception("Canot setup gpu texture multible times");
            return;
        }
        loaded = true;

        if(texture.FileData is null)
        {
            throw new NullReferenceException();    
        }

        if (GLInterface == null) throw new ArgumentNullException(nameof(GLInterface));
        this.GLInterface = GLInterface;

        Handel = OpenGL.GenTexture();
        OpenGL.ActiveTexture(TextureUnit.Texture0);
        OpenGL.BindTexture(TextureTarget.Texture2D, Handel);

        //ImageResult.FromMemory reads the bytes of the file data file and returns all its information!
        ImageResult result = ImageResult.FromMemory(texture.FileData, ColorComponents.RedGreenBlueAlpha);

        // Define a pointer to the image data
        fixed (byte* ptr = result.Data)
            // Here we use "result.Width" and "result.Height" to tell OpenGL about how big our texture is.
            OpenGL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
                (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        
        OpenGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
        OpenGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
        OpenGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest); // <- change here!
        OpenGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);

        OpenGL.GenerateMipmap(TextureTarget.Texture2D);
    }
    
    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        //When we bind a texture we can choose which textureslot we can bind it to.
        OpenGL.ActiveTexture(textureSlot);
        OpenGL.BindTexture(TextureTarget.Texture2D, Handel);
    }

    public void Dispose()
    {
        // Dispose logic here
        OpenGL.DeleteTexture(Handel);
    }
}