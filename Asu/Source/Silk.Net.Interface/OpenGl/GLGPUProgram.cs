using System.Numerics;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Modedlus.Systems.RenderInterface;
using Silk.NET.OpenGL;

namespace Silk.Net.Interface.OpenGl;

public class GLGPUProgram : GLGPUObjectBase, IDisposable, GPUProgram
{
    public Modedlus.Systems.RenderInterface.Shader source { get; private set; }
    
    public uint ProgramID;
    
    public GLGPUProgram(OpenGLRenderInterface GLInterface, Modedlus.Systems.RenderInterface.Shader shader)
    {
        if (GLInterface == null) throw new ArgumentNullException(nameof(GLInterface));
        this.GLInterface = GLInterface;

        //Load the individual shaders.
        
        string vertexCode = shader.VertexShader;

        string fragmentCode = shader.FragmentShader;

        // Shader Compilation

        uint vertexShader = OpenGL.CreateShader(ShaderType.VertexShader);
        OpenGL.ShaderSource(vertexShader, vertexCode);

        OpenGL.CompileShader(vertexShader);

        OpenGL.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + OpenGL.GetShaderInfoLog(vertexShader));

        uint fragmentShader = OpenGL.CreateShader(ShaderType.FragmentShader);
        OpenGL.ShaderSource(fragmentShader, fragmentCode);
        
        OpenGL.CompileShader(fragmentShader);
        
        OpenGL.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + OpenGL.GetShaderInfoLog(fragmentShader));

        ProgramID = OpenGL.CreateProgram();

        // Program Manegment

        OpenGL.AttachShader(ProgramID, vertexShader);
        OpenGL.AttachShader(ProgramID, fragmentShader);

        OpenGL.LinkProgram(ProgramID);

        OpenGL.GetProgram(ProgramID, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + OpenGL.GetProgramInfoLog(ProgramID));

        OpenGL.DetachShader(ProgramID, vertexShader);
        OpenGL.DetachShader(ProgramID, fragmentShader);
        OpenGL.DeleteShader(vertexShader);
        OpenGL.DeleteShader(fragmentShader);
    }
    
    public void Bind()
    {
        //Using the program
        OpenGL.UseProgram(ProgramID);
    }

    public void Dispose()
    {
        // Dispose logic here
    }

    private Dictionary<string, int> bindedPropetes = new Dictionary<string, int>();
    private Dictionary<string, int> bindedTextures = new Dictionary<string, int>();

    public void SetUniform(string name, float value) => BindPropete(name, value, RenderPropType._float);
    public void SetUniform(string name, int value) => BindPropete(name, value, RenderPropType._int);
    public void SetUniform(string name, Matrix4x4 value) => BindPropete(name, value, RenderPropType._M4x4);
    public void SetUniform(string name, Matrix3x2 value) => BindPropete(name, value, RenderPropType._M3x2);
    public void SetUniform(string name, GPUTexture value) => BindPropete(name, value, RenderPropType._textures);

    public unsafe void BindPropete(string name, object value, RenderPropType type)
    {
        OpenGL.UseProgram(ProgramID);

        if (value is null || type is RenderPropType._null)
            throw new NullReferenceException();
        
        if(!bindedPropetes.ContainsKey(name)){
            // Creates new slot with the name
            int location = OpenGL.GetUniformLocation(ProgramID, name);

            // Succsesfull
            Console.WriteLine($"Creating new uniform {name} with slot {location}");
            bindedPropetes.Add(name, location);

            // Checks for valid location
            if (location == -1)
                throw new Exception($"{name} uniform not found on shader.");
        }

        // Set the value
        if (bindedPropetes.TryGetValue(name, out int slot))
        {
            switch (type)
            {
                case RenderPropType._float:
                    OpenGL.Uniform1(slot, (float)value);
                    break;

                case RenderPropType._int:
                    OpenGL.Uniform1(slot, (int)value);
                    break;

                case RenderPropType._M4x4:
                    Matrix4x4 matrix4X4 = (Matrix4x4)value;
                    OpenGL.UniformMatrix4(slot, 1, false, (float*) &matrix4X4);
                    break;  

                case RenderPropType._M3x2:
                    Matrix3x2 matrix3X2 = (Matrix3x2)value;
                    OpenGL.UniformMatrix3x2(slot, 1, false, (float*) &matrix3X2);
                    break;  

                case RenderPropType._textures:
                    GLGPUTexture texture = (GLGPUTexture)value;
                    int textureLoc = BindTexture(name, texture);
                    if(textureLoc < 0)
                        throw new Exception("Was unable to bind texture");
                    OpenGL.Uniform1(slot, textureLoc);
                    break;  
            }
        }
    }

    public int BindTexture(string name, GLGPUTexture texture)
    {
        Console.WriteLine("Binding texture");

        if(texture is null)
            return -1;
        if(!bindedTextures.ContainsKey(name)){
            // Creates new slot with the name
            int location = bindedTextures.Count;

            // Succsesfull
            Console.WriteLine($"Creating new texture {name} with slot {location}");
            bindedTextures.Add(name, location);
        }

        if (bindedTextures.TryGetValue(name, out int slot))
        {
            texture.Bind(TextureUnit.Texture0 + slot);
            return slot;
        }

        return -2;
    }
}