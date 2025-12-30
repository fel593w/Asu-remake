using Modedlus.Systems.RenderInterface;
using Silk.NET.OpenGL;

namespace Silk.Net.Interface.OpenGl;

public class GLGPUShader : GLGPUObjectBase, IDisposable, GPUShader
{
    public Modedlus.Systems.RenderInterface.Shader source { get; private set; }
    
    public uint ProgramID;
    
    public GLGPUShader(OpenGLRenderInterface GLInterface, Modedlus.Systems.RenderInterface.Shader shader)
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
}