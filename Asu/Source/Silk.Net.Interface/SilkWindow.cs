using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Modedlus.Systems.WindowInterface;
using Modedlus.Systems.RenderInterface;
using Modedlus.Systems.LoopSystems;
using Silk.NET.Vulkan;
using Silk.NET.OpenGL;
using System.Drawing;
using Silk.NET.SPIRV;
using System.Drawing;
using StbImageSharp;
using Silk.Net.Interface.OpenGl;

namespace Silk.Net.Interface;

public class SilkWindow : Modedlus.Systems.WindowInterface.Window, IDisposable
{

    private Thread WindowThread;

    private SilkRenderInterface RenderInterface;

    public SilkWindow(int width, int height, WindowAPI RenderAPI)
    {
        // Configure Input Value
        int _Width = width;
        int _Height = height;

        // Set Base Values
        WindowOptions options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(_Width, _Height),
            Title = "Modedlus Aplication",
            FramesPerSecond = 500,
            UpdatesPerSecond = 1,
            WindowBorder = WindowBorder.Resizable,

        };

        // Create the window Interface
        SilkWindowInterface = Silk.NET.Windowing.Window.Create(options);

        // Setup Render API
        switch (RenderAPI)
        {
            case WindowAPI.OpenGL:
                SetupOpenGL();
                break;
            case WindowAPI.Vulkan:
                SetupVulkan();
                break;
            default:
                throw new NotImplementedException("The selected Window API is not implemented.");
        }

        // Starts the window in its own thread
        WindowThread = new Thread(() => SilkWindowInterface.Run());
        WindowThread.Start();
    }

    public RenderInterface SetupRenderInterface(WindowAPI RenderAPI)
    {
        // Setup Render API
        switch (RenderAPI)
        {
            case WindowAPI.OpenGL:
                return new OpenGLRenderInterface();
                break;
            case WindowAPI.Vulkan:
                return SetupVulkan();
                break;
            default:
                throw new NotImplementedException("The selected Window API is not implemented.");
        }
    }

    #region Window Properties

    public uint Width { get {return (uint)SilkWindowInterface.Size.X;} set { int Width = (int)value; SilkWindowInterface.Size = new Vector2D<int>(Width, SilkWindowInterface.Size.Y); } }
    public uint Height { get {return (uint)SilkWindowInterface.Size.Y;} set { int Height = (int)value; SilkWindowInterface.Size = new Vector2D<int>(SilkWindowInterface.Size.X, Height); } }
    public string Title { get {return SilkWindowInterface.Title;} set { SilkWindowInterface.Title = value; } }
    public uint FrameRate { get {return (uint)SilkWindowInterface.FramesPerSecond;} set {  SilkWindowInterface.FramesPerSecond = value; } }

    private IWindow SilkWindowInterface;

    public void Dispose()
    {
        SilkWindowInterface.Close();
        WindowThread.Join();
    }

    #endregion

    #region Render Interface

    public Loop RenderLoop { get; set; }

    #region OpenGL API

    private GL OpenGL;

    private uint _vao;
    private static uint _vbo;
    private uint _ebo;
    private uint _program;

    private unsafe void SetupOpenGL()
    {
        // Setup Load Event
        SilkWindowInterface.Load += OnLoad_OpenGL;

        // Render Event 
        SilkWindowInterface.Render += OnRender_OpenGL;
    }

    private unsafe void OnLoad_OpenGL()
    {
        // Create OpenGL Context
        OpenGL = SilkWindowInterface.CreateOpenGL();
        OpenGL.ClearColor(Color.CornflowerBlue);

        LoadMesh();
        LoadShaders();
    }

    private unsafe void OnRender_OpenGL(double DeltaTime)
    {
        OpenGL.Clear(ClearBufferMask.ColorBufferBit);
        OpenGL.BindVertexArray(_vao);
        OpenGL.UseProgram(_program);
        OpenGL.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, (void*) 0);
    }

    private unsafe void LoadMesh()
    {

        // Meshes
        // Get Mesh Data

        GetMesh(out float[] vertices, out uint[] indices);

        _vao = OpenGL.GenVertexArray();
        OpenGL.BindVertexArray(_vao);

        _vbo = OpenGL.GenBuffer();
        OpenGL.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* buf = vertices)
            OpenGL.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        _ebo = OpenGL.GenBuffer();
            OpenGL.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        fixed (uint* buf = indices)
            OpenGL.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

        // Vertex Atribute Manegement
        const uint positionLoc = 0;
        OpenGL.EnableVertexAttribArray(positionLoc);
        OpenGL.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*) 0);
        const uint texCoordLoc = 1;
        OpenGL.EnableVertexAttribArray(texCoordLoc);
        OpenGL.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
    
        OpenGL.BindVertexArray(0);
        OpenGL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        OpenGL.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

    }

    private unsafe void LoadShaders()
    {

        // Shaders
        // Shader Compiling

        GetShaders(out string vertexCode, out string fragmentCode);

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

        // Program
        
        _program = OpenGL.CreateProgram();

        OpenGL.AttachShader(_program, vertexShader);
        OpenGL.AttachShader(_program, fragmentShader);

        OpenGL.LinkProgram(_program);

        OpenGL.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + OpenGL.GetProgramInfoLog(_program));

        OpenGL.DetachShader(_program, vertexShader);
        OpenGL.DetachShader(_program, fragmentShader);
        OpenGL.DeleteShader(vertexShader);
        OpenGL.DeleteShader(fragmentShader);
    }

    private unsafe void GetMesh(out float[] vertices, out uint[] indices)
    {
        vertices = new float[] {
             0.0f,  0.5f, 0.0f, 1.0f, 0.0f, 
             0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f};
        indices = new uint[] { 0u, 1u, 3u };
    }

    private unsafe void GetShaders(out string vertexCode, out string fragmentCode)
    {
        vertexCode = @"
            #version 330 core

            layout (location = 0) in vec3 aPosition;

            layout (location = 1) in vec2 aTextureCoord;

            out vec2 frag_texCoords;

            void main()
            {
                gl_Position = vec4(aPosition, 1.0);
                frag_texCoords = aTextureCoord;
            }";

        fragmentCode = @"
            #version 330 core

            out vec4 out_color;

            in vec2 frag_texCoords;

            void main()
            {
                out_color = vec4(frag_texCoords.x, frag_texCoords.y, 0.0, 1.0);
            }";
    }

    public unsafe uint LoadTexture(String Path, TextureProperties properties)
    {
        uint textureId = OpenGL.GenTexture();
                // ImageResult.FromMemory reads the bytes of the .png file and returns all its information!
        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes("SneOs.png"), ColorComponents.RedGreenBlueAlpha);

        // Define a pointer to the image data
        fixed (byte* ptr = result.Data)
            // Here we use "result.Width" and "result.Height" to tell OpenGL about how big our texture is.
            OpenGL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
                (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);

        OpenGL.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
        OpenGL.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
        OpenGL.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.Nearest);
        OpenGL.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);
        
        OpenGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
        OpenGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
        OpenGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest); // <- change here!
        OpenGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);

        OpenGL.GenerateMipmap(TextureTarget.Texture2D);

        OpenGL.BindTexture(TextureTarget.Texture2D, 0);
        //_gl.BindTexture(TextureTarget.Texture2D, _texture)

        int location = OpenGL.GetUniformLocation(_program, "uTexture");
        OpenGL.Uniform1(location, 0);  
    }

    public void UnLoadTexture()
    {

    }

    public void SetTexture(uint TextureId)
    {
        
    }

    /*

    private unsafe void OnLoad_OpenGL()
    {
        Console.WriteLine("Loading OpenGl");

        // Buffers

        OpenGL = SilkWindowInterface.CreateOpenGL();
        _vao = OpenGL.GenVertexArray();
        OpenGL.BindVertexArray(_vao);

        // Meshes 
        float[] vertices =
        {
             0.5f,  0.5f, 0.0f, 
             0.5f, -0.5f, 0.0f, 
            -0.5f, -0.5f, 0.0f, 
            -0.5f,  0.5f, 0.0f
        };

        uint[] indices =
        {
            0u, 1u, 3u,
            1u, 2u, 3u
        };

        _vbo = OpenGL.GenBuffer();
        OpenGL.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        _ebo = OpenGL.GenBuffer();
        OpenGL.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        fixed (float* buf = vertices)
            OpenGL.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        fixed (uint* buf = indices)
            OpenGL.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

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

            //uniform sampler2D uTexture;

            void main()
            {
                // This will allow us to see the texture coordinates in action!
                out_color = vec4(frag_texCoords.x, frag_texCoords.y, 0.0, 1.0);
                //out_color = vec4(gl_FragCoord.x/800, gl_FragCoord.y /600, gl_FragCoord.z, 1.0);
                //out_color = vec4(gl_FragCoord.z, gl_FragCoord.z, gl_FragCoord.z, 1.0);
                
                // lighting
                float product = dot(normalize(vec3(1.0, 1.0, 1.0)), normalize(frag_normal));
                product = clamp((product+0.65)*0.6, 0, 1);
                
                //out_color = vec4(frag_normal.x, frag_normal.y, frag_normal.z, 1.0);
                //out_color = texture(uTexture, frag_texCoords*16);//*product;
            }
            ";

        // Shader Compiling

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

        _program = OpenGL.CreateProgram();

        // Program Manegment

        OpenGL.AttachShader(_program, vertexShader);
        OpenGL.AttachShader(_program, fragmentShader);

        OpenGL.LinkProgram(_program);

        OpenGL.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + OpenGL.GetProgramInfoLog(_program));

        OpenGL.DetachShader(_program, vertexShader);
        OpenGL.DetachShader(_program, fragmentShader);
        OpenGL.DeleteShader(vertexShader);
        OpenGL.DeleteShader(fragmentShader);

        // Atribute Manegement

        const uint VertexLength = 3;

        const uint positionLoc = 0;
        OpenGL.EnableVertexAttribArray(positionLoc);
        OpenGL.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, VertexLength * sizeof(float), (void*) 0);
        // const uint texCoordLoc = 1;
        // OpenGL.EnableVertexAttribArray(texCoordLoc);
        // OpenGL.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, VertexLength * sizeof(float), (void*)(3 * sizeof(float)));
        // const uint normalLoc = 2;
        // OpenGL.EnableVertexAttribArray(normalLoc);
        // OpenGL.VertexAttribPointer(normalLoc, 3, VertexAttribPointerType.Float, false, VertexLength * sizeof(float), (void*)(5 * sizeof(float)));

        // Clean up

        OpenGL.BindVertexArray(0);
        OpenGL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        OpenGL.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        // Textures

        //_texture = OpenGL.GenTexture();
        //OpenGL.ActiveTexture(TextureUnit.Texture0);
        //OpenGL.BindTexture(TextureTarget.Texture2D, _texture);

        // ImageResult.FromMemory reads the bytes of the .png file and returns all its information!
        //ImageResult result = ImageResult.FromMemory(File.ReadAllBytes("SneOs.png"), ColorComponents.RedGreenBlueAlpha);

        // Define a pointer to the image data
        //fixed (byte* ptr = result.Data)
        //    // Here we use "result.Width" and "result.Height" to tell OpenGL about how big our texture is.
        //    OpenGL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
        //        (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);

        //OpenGL.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
        //OpenGL.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
        //OpenGL.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.Nearest);
        //OpenGL.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);
        
        //OpenGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
        //OpenGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
        //OpenGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest); // <- change here!
        //OpenGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);

        //OpenGL.GenerateMipmap(TextureTarget.Texture2D);

        //OpenGL.BindTexture(TextureTarget.Texture2D, 0);
        //_gl.BindTexture(TextureTarget.Texture2D, _texture)

        //int location = OpenGL.GetUniformLocation(_program, "uTexture");
        //OpenGL.Uniform1(location, 0);  
    }

    private unsafe void OnRender_OpenGL(double DeltaTime) {

        Console.WriteLine("Rendering OpenGL Frame");

        OpenGL.ClearColor(Color.CornflowerBlue);
        OpenGL.Clear(ClearBufferMask.ColorBufferBit);
        OpenGL.Enable(EnableCap.CullFace);
        OpenGL.CullFace(GLEnum.Back);
        OpenGL.Enable(EnableCap.Blend);
        OpenGL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
  
        OpenGL.BindVertexArray(_vao);
        OpenGL.UseProgram(_program);

        // Meshes 
        float[] vertices =
        {
             0.5f,  0.5f, 0.0f, 
             0.5f, -0.5f, 0.0f, 
            -0.5f, -0.5f, 0.0f, 
            -0.5f,  0.5f, 0.0f
        };

        uint[] indices =
        {
            0u, 1u, 3u,
            1u, 2u, 3u
        };


        OpenGL.DrawElements(PrimitiveType.Triangles, (uint)indices.Length, DrawElementsType.UnsignedInt, (void*) 0);
    }

    */

    #endregion

    #region Vulkan API

    private void SetupVulkan()
    {
        Console.WriteLine("Vulkan Setup Not Implemented Yet");
        Console.WriteLine("ERROR: Vulkan Setup Not Implemented Yet");
    }

    #endregion

    #endregion
}

public enum WindowAPI
{
    OpenGL,
    Vulkan,
}

