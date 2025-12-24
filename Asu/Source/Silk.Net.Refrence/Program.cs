using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;
using StbImageSharp;
using System.Numerics;
using System.Collections.Generic;
using System.Globalization;

namespace MySilkProgram;

public class Program
{
    private static IWindow _window;
    private static GL _gl;
    private static uint _vao;
    private static uint _vbo;
    private static uint _ebo;
    private static uint _program;

    private static uint _texture;

    public static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "ModedlusRenderingTest";

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;

        _window.Run();
    }

    private static unsafe void OnLoad()
    {
        Console.WriteLine("Load!");

        IInputContext input = _window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++){
            input.Keyboards[i].KeyDown += KeyDown;
        }

        // Buffers

        _gl = _window.CreateOpenGL();
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        // Meshes 
        LoadObj("Monkey.obj", out float[] vertices, out uint[] indices);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        fixed (float* buf = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        fixed (uint* buf = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

        // Shaders

        const string vertexCode = @"
            #version 330 core

            layout (location = 0) in vec3 aPosition;
            // Add a new input attribute for the texture coordinates
            layout (location = 1) in vec2 aTextureCoord;

            layout (location = 2) in vec3 aNormal;

            // Add an output variable to pass the texture coordinate to the fragment shader
            // This variable stores the data that we want to be received by the fragment
            out vec2 frag_texCoords;
            out vec3 frag_normal;

            void main()
            {   
                vec3 p = aPosition * 0.6 + vec3(0.0, 0.0, 0);
                gl_Position = vec4(p, 1.0);

                // Assigin the texture coordinates without any modification to be recived in the fragment
                frag_texCoords = aTextureCoord;
                frag_normal = aNormal;
            }
            ";

        const string fragmentCode = @"
            #version 330 core

            // Receive the input from the vertex shader in an attribute
            in vec2 frag_texCoords;
            in vec3 frag_normal;

            out vec4 out_color;

            uniform sampler2D uTexture;

            void main()
            {
                // This will allow us to see the texture coordinates in action!
                //out_color = vec4(frag_texCoords.x, frag_texCoords.y, 0.0, 1.0);
                //out_color = vec4(gl_FragCoord.x/800, gl_FragCoord.y /600, gl_FragCoord.z, 1.0);
                //out_color = vec4(gl_FragCoord.z, gl_FragCoord.z, gl_FragCoord.z, 1.0);
                
                // lighting
                float product = dot(normalize(vec3(1.0, 1.0, 1.0)), normalize(frag_normal));
                product = clamp((product+0.65)*0.6, 0, 1);
                
                //out_color = vec4(frag_normal.x, frag_normal.y, frag_normal.z, 1.0);
                out_color = texture(uTexture, frag_texCoords*16);//*product;
            }
            ";

        // Shader Compiling

        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexCode);

        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));

        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentCode);
        
        _gl.CompileShader(fragmentShader);
        
        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));

        _program = _gl.CreateProgram();

        // Program Manegment

        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);

        _gl.LinkProgram(_program);

        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));

        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        // Atribute Manegement

        const uint VertexLength = 8;

        const uint positionLoc = 0;
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, VertexLength * sizeof(float), (void*) 0);
        const uint texCoordLoc = 1;
        _gl.EnableVertexAttribArray(texCoordLoc);
        _gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, VertexLength * sizeof(float), (void*)(3 * sizeof(float)));
        const uint normalLoc = 2;
        _gl.EnableVertexAttribArray(normalLoc);
        _gl.VertexAttribPointer(normalLoc, 3, VertexAttribPointerType.Float, false, VertexLength * sizeof(float), (void*)(5 * sizeof(float)));

        // Clean up

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        // Textures

        _texture = _gl.GenTexture();
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture);

        // ImageResult.FromMemory reads the bytes of the .png file and returns all its information!
        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes("SneOs.png"), ColorComponents.RedGreenBlueAlpha);

        // Define a pointer to the image data
        fixed (byte* ptr = result.Data)
            // Here we use "result.Width" and "result.Height" to tell OpenGL about how big our texture is.
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
                (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);

        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.Nearest);
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);
        
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest); // <- change here!
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);

        _gl.GenerateMipmap(TextureTarget.Texture2D);

        _gl.BindTexture(TextureTarget.Texture2D, 0);
        //_gl.BindTexture(TextureTarget.Texture2D, _texture)

        int location = _gl.GetUniformLocation(_program, "uTexture");
        _gl.Uniform1(location, 0);  
    }

    // These two methods are unused for this tutorial, aside from the logging we added earlier.
    private static void OnUpdate(double deltaTime)
    {
        
    }

    private static unsafe void OnRender(double deltaTime)
    {
        _gl.ClearColor(Color.CornflowerBlue);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _gl.Enable(EnableCap.CullFace);
        _gl.CullFace(GLEnum.Back);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _gl.BindVertexArray(_vao);
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture);
        _gl.UseProgram(_program);

        LoadObj("Monkey.obj", out float[] vertices, out uint[] indices);
        _gl.DrawElements(PrimitiveType.Triangles, (uint)indices.Length, DrawElementsType.UnsignedInt, (void*) 0);
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
            _window.Close();
    }

    public static void LoadObj(
        string path,
        out float[] vertices,
        out uint[] indices)
    {
        var positions = new List<Vector3>();
        var uvs = new List<Vector2>();
        var normals = new List<Vector3>();

        var vbo = new List<float>();
        var ibo = new List<uint>();
        uint index = 0;

        foreach (var line in File.ReadLines(path))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0])
            {
                case "v":
                    positions.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)));
                    break;

                case "vt":
                    uvs.Add(new Vector2(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        1.0f - float.Parse(parts[2], CultureInfo.InvariantCulture))); // flip V
                    break;

                case "vn":
                    normals.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)));
                    break;

                case "f":
                    // assumes triangulated faces
                    for (int i = 1; i <= 3; i++)
                    {
                        var idx = parts[i].Split('/');
                        int v = int.Parse(idx[0]) - 1;
                        int vt = int.Parse(idx[1]) - 1;
                        int vn = int.Parse(idx[2]) - 1;

                        var pos = positions[v];
                        var uv = uvs[vt];
                        var nrm = normals[vn];

                        // add interleaved vertex
                        vbo.Add(pos.X);
                        vbo.Add(pos.Y);
                        vbo.Add(pos.Z);
                        vbo.Add(uv.X);
                        vbo.Add(uv.Y);
                        vbo.Add(nrm.X);
                        vbo.Add(nrm.Y);
                        vbo.Add(nrm.Z);

                        ibo.Add(index++);
                    }
                    break;
            }
        }

        vertices = vbo.ToArray();
        indices = ibo.ToArray();
    }

}