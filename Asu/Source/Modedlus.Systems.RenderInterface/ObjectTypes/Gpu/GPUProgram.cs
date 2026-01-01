using System.Numerics;

namespace Modedlus.Systems.RenderInterface;

public interface GPUProgram : IDisposable
{
    /// <summary>
    /// Stores the shader witch is loaded form
    /// WARNING it is not garanteed that this Shader is the same as the one in the gpu memory
    /// </summary>
    public Shader source { get; }   

    public void SetUniform(string name, float value);
    public void SetUniform(string name, int value);
    public void SetUniform(string name, Matrix4x4 value);
    public void SetUniform(string name, Matrix3x2 value);
    public void SetUniform(string name, GPUTexture value);
}