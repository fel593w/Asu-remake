using Modedlus.Systems.LoopSystems;

namespace Modedlus.Systems.RenderInterface;

public interface RenderInterface
{
    public Loop RenderLoop { get; set; }

    public GPUTexture LoadTexture(Texture image, TextureProperties properties);
    public void UnLoadTexture(GPUTexture textureId);

    public GPUShader LoadShaderProgram(Shader shader);
    public void UnLoadShaderProgram(GPUShader shaderProgramId);

    public GPUMesh LoadMesh(Mesh mesh);
    public void UnLoadMesh(GPUMesh meshId);

    public void BindPropetes(GPUShader shader, params GPUTexture[] texture);
    public void DrawMesh(GPUMesh mesh);
}