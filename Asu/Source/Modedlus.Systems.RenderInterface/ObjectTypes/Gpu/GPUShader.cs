namespace Modedlus.Systems.RenderInterface;

public interface GPUShader
{
    /// <summary>
    /// Stores the shader witch is loaded form
    /// WARNING it is not garanteed that this Shader is the same as the one in the gpu memory
    /// </summary>
    public Shader source { get; }   
}