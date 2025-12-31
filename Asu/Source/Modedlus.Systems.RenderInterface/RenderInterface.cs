using Modedlus.Systems.LoopSystems;

namespace Modedlus.Systems.RenderInterface;

public interface RenderInterface
{
    public Loop RenderLoop { get; set; }

    #region Load/Unload Func

    public GPUTexture LoadTexture(Texture image, TextureProperties properties);
    public void UnLoadTexture(GPUTexture textureId);

    public GPUProgram LoadShaderProgram(Shader shader);
    public void UnLoadShaderProgram(GPUProgram shaderProgramId);

    public GPUMesh LoadMesh(Mesh mesh);
    public void UnLoadMesh(GPUMesh meshId);

    #endregion

    #region 

    public void BindProgram(GPUProgram propetes);
    public void DrawMesh(GPUMesh mesh);

    #endregion
}