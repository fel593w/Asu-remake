using Modedlus.Systems.RenderInterface;
using Silk.Net.Interface.OpenGl;
using Silk.NET.OpenGL;

namespace Silk.Net.Interface.OpenGl;

public class GLGPUMesh : GLGPUComonMeshObject, GPUMesh
{
    public Mesh source { get; private set; }

    public unsafe GLGPUMesh(OpenGLRenderInterface GLInterface, Mesh mesh)
    {
        if (mesh is null)
            throw new Exception("mesh.source is null");

        if (mesh.Indices is null or { Length: 0 })
            throw new Exception("mesh.source.Indices is null");

        if (mesh.Vertices is null or { Length: 0 })
            throw new Exception("mesh.source.Indices is null");

        if (GLInterface == null) throw new ArgumentNullException(nameof(GLInterface));
        this.GLInterface = GLInterface;

        source = mesh;

        Vao = OpenGL.GenVertexArray();
        OpenGL.BindVertexArray(Vao);

        Vbo = OpenGL.GenBuffer();
        OpenGL.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo);

        Ebo = OpenGL.GenBuffer();
        OpenGL.BindBuffer(BufferTargetARB.ElementArrayBuffer, Ebo);

        fixed (float* buf = mesh.Vertices)
            OpenGL.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (mesh.Vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        fixed (uint* buf = mesh.Indices)
            OpenGL.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (mesh.Indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

                    // Atribute Manegement

        const uint VertexLength = 8;

        const uint positionLoc = 0;
        OpenGL.EnableVertexAttribArray(positionLoc);
        OpenGL.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, VertexLength * sizeof(float), (void*) 0);
        const uint texCoordLoc = 1;
        OpenGL.EnableVertexAttribArray(texCoordLoc);
        OpenGL.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, VertexLength * sizeof(float), (void*)(3 * sizeof(float)));
        const uint normalLoc = 2;
        OpenGL.EnableVertexAttribArray(normalLoc);
        OpenGL.VertexAttribPointer(normalLoc, 3, VertexAttribPointerType.Float, false, VertexLength * sizeof(float), (void*)(5 * sizeof(float)));
    
        OpenGL.BindVertexArray(0);
        OpenGL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        Console.WriteLine($"Created Mesh binded to {Vao} (Vertex: {Vbo} Indeces: {Ebo})");
    
    }

    public void Bind()
    {
        OpenGL.BindVertexArray(Vao);
    }

}