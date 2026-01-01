using Modedlus.Systems.RenderInterface;
using Modedlus.Systems.LoopSystems;
using Silk.NET.OpenGL;
using System.Net.Http.Headers;
using Silk.NET.OpenCL;
using Silk.NET.Windowing;
using System.Drawing;

namespace Silk.Net.Interface.OpenGl;

public class OpenGLRenderInterface :SilkRenderInterface, RenderInterface
{
    public Loop RenderLoop { get; set; }

    public GPUTexture LoadTexture(Modedlus.Systems.RenderInterface.Texture texture, TextureProperties properties) => new GLGPUTexture(this, texture);
    public void UnLoadTexture(GPUTexture textureId) => ((GLGPUTexture)textureId).Dispose();


    public GPUProgram LoadShaderProgram(Modedlus.Systems.RenderInterface.Shader shader) => new GLGPUProgram(this, shader);
    public void UnLoadShaderProgram(GPUProgram shaderProgramId) => ((GLGPUProgram)shaderProgramId).Dispose();

    public GPUMesh LoadMesh(Mesh mesh) => new GLGPUMesh(this, mesh);
    public void UnLoadMesh(GPUMesh meshId) => ((GLGPUMesh)meshId).Dispose();

    public void BindProgram(GPUProgram program) {try { _bindProgram(program); } catch (Exception e) { Console.WriteLine(e); _bindErrorProgram(); } }
    public void DrawMesh(GPUMesh mesh) => _drawMesh(mesh);
    public GL OpenGL;

    public OpenGLRenderInterface(IWindow iwindow)
    {

        //Setup
        RenderLoop = new RenderLoop();

        // Setup OpenGL
        OpenGL = iwindow.CreateOpenGL();

        OpenGL.Enable(EnableCap.CullFace);
        OpenGL.CullFace(GLEnum.Back);
        OpenGL.Enable(EnableCap.Blend);
        OpenGL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        OpenGL.Enable(EnableCap.DepthTest);
        OpenGL.DepthFunc(DepthFunction.Less);
        //OpenGL.DepthRange(0.0, 1.0);

        // Adds the render update function
        iwindow.Render += Render;
    }

    private void Render(double DeltaTime)
    {
        // TEMPORARY
        OpenGL.ClearColor(Color.CornflowerBlue);
        OpenGL.Clear(ClearBufferMask.ColorBufferBit);
        OpenGL.ClearColor(Color.Black);
        OpenGL.Clear(ClearBufferMask.DepthBufferBit);
        //OpenGL.DrawBuffer
        //OpenGL.Clear(ClearBufferMask.DepthBufferBit);

        // Runs the render loop
        if(RenderLoop?.GetType() == typeof(RenderLoop)) 
            ((RenderLoop)RenderLoop).Invoke(new LoopEventArgs { DeltaTime = (float)DeltaTime });
    }

    #region functions

    private unsafe void _bindErrorProgram()
    {
        Console.WriteLine("Binding Error");
        throw new Exception();
    }

    private unsafe void _bindProgram (GPUProgram program)
    {
        if(program is not GLGPUProgram)
        {
            throw new Exception("The Program was null or of the wrong type");
        }

        // shader binding
        GLGPUProgram GLProgram = ((GLGPUProgram)program);
        GLProgram.Bind();

    }

    private unsafe void _drawMesh (GPUMesh mesh)
    {
        if (mesh == null)
            throw new Exception("FAIL: mesh is null");
        if (mesh.source == null)
            throw new Exception("FAIL: mesh source is null");

        ((GLGPUMesh)mesh).Bind();

        if (OpenGL == null)
            throw new Exception("FAIL: OpenGL instance is null");

        try {
        OpenGL.DrawElements(PrimitiveType.Triangles, (uint)mesh.source.Indices.Length, DrawElementsType.UnsignedInt, null);
        }catch (Exception e){ Console.WriteLine($"FAIL: {e}");}
    }
    
    #endregion

}