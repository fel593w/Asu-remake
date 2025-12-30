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


    public GPUShader LoadShaderProgram(Modedlus.Systems.RenderInterface.Shader shader) => new GLGPUShader(this, shader);
    public void UnLoadShaderProgram(GPUShader shaderProgramId) => ((GLGPUShader)shaderProgramId).Dispose();

    public GPUMesh LoadMesh(Mesh mesh) => new GLGPUMesh(this, mesh);
    public void UnLoadMesh(GPUMesh meshId) => ((GLGPUMesh)meshId).Dispose();

    public void BindPropetes(GPUShader shader, GPUTexture[] textures) {try { _bindPropetes(shader, textures); } catch (Exception e) { Console.WriteLine(e); _bindErrorPropetes(); } }
    public void DrawMesh(GPUMesh mesh) => _drawMesh(mesh);
    public GL OpenGL;

    public OpenGLRenderInterface(IWindow iwindow)
    {

        //Setup
        RenderLoop = new RenderLoop();

        // Setup OpenGL
        OpenGL = iwindow.CreateOpenGL();

        // Adds the render update function
        iwindow.Render += Render;
    }

    private void Render(double DeltaTime)
    {
        OpenGL.ClearColor(Color.CornflowerBlue);
        OpenGL.Enable(EnableCap.CullFace);
        OpenGL.CullFace(GLEnum.Back);
        OpenGL.Enable(EnableCap.Blend);
        OpenGL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        // Runs the render loop
        if(RenderLoop?.GetType() == typeof(RenderLoop)) 
            ((RenderLoop)RenderLoop).Invoke(new LoopEventArgs { DeltaTime = (float)DeltaTime });
    }

    #region functions

    private unsafe void _bindErrorPropetes()
    {
        Console.WriteLine("Binding Error");
        throw new Exception();
    }

    private unsafe void _bindPropetes (GPUShader shader, GPUTexture[] textures)
    {
        // shader binding
        OpenGL.UseProgram(((GLGPUShader)shader).ProgramID);

        if(textures is null)
            return;

        for (int i = 0; i < textures.Length; i++)
        {
            ((GLGPUTexture)textures[i]).Bind(TextureUnit.Texture0);
        }

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