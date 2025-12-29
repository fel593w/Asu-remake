using Modedlus.Systems.LoopSystems;

namespace Modedlus.Systems.RenderInterface;

public interface RenderInterface
{
    public Loop RenderLoop { get; set; }

    public unsafe uint LoadTexture(Texture image, TextureProperties properties);
    public unsafe void UnLoadTexture(uint textureId);
    public unsafe void SetActiveTexture(uint textureId, uint slot);

    public unsafe uint LoadShaderProgram(Shader shader);
    public unsafe void UnLoadShaderProgram(uint shaderProgramId);
    public unsafe void SetActiveShaderProgram(uint shaderProgramId);

    public unsafe uint LoadMesh(Mesh mesh);
    public unsafe void UnLoadMesh(uint meshId);
    public unsafe void DrawMesh(uint meshId);


}