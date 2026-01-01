using Silk.Net.Interface.OpenGl;
using Silk.NET.OpenGL;
using StbImageSharp;

public static class ErrorShader
{
    const string vertexCode = @"
            #version 330 core

            layout (location = 0) in vec3 aPosition;
            // Add a new input attribute for the texture coordinates
            layout (location = 1) in vec2 aTextureCoord;

            layout (location = 2) in vec3 aNormal;

            uniform mat4 uModel;
            uniform mat4 uView;
            uniform mat4 uProjection;

            // Add an output variable to pass the texture coordinate to the fragment shader
            // This variable stores the data that we want to be received by the fragment
            out vec2 frag_texCoords;
            out vec3 frag_normal;

            void main()
            {  
                gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);

                // Assigin the texture coordinates without any modification to be recived in the fragment
                frag_texCoords = aTextureCoord;
                vec3 transformNormal = normalize(mat3(transpose(inverse(uModel)))* aNormal);
                frag_normal = transformNormal;
            }
            ";

    const string fragmentCode = @"
            #version 330 core

            // Receive the input from the vertex shader in an attribute
            in vec2 frag_texCoords;
            in vec3 frag_normal;

            out vec4 out_color;

            uniform sampler2D uText;

            void main()
            {   
                // Camera based texture maping
                vec2 pixPos = vec2(gl_FragCoord) / vec2(1280, 720);
                vec4 cameraRefrenceColor = texture(uText, (pixPos*vec2(12, -25)));

                // Output
                vec3 modColor = vec3(0.7, 0, 1);
                modColor = modColor * (1 - cameraRefrenceColor.w); 
                vec3 camColor = vec3(cameraRefrenceColor.x, cameraRefrenceColor.y, cameraRefrenceColor.z);
                // add multiplication if nessesary
                out_color = vec4(modColor + camColor, 1); 
            }
            ";

    private static  uint vertexShader;
    private static uint fragmentShader;

    private static uint errorTextTexture;
    private static bool errorTextLoaded = false;

    public static unsafe void Compile(GL OpenGL)
    {
        Console.WriteLine("Compiling \"Error Shaders\"");

        // Shader Compilation
        vertexShader = OpenGL.CreateShader(ShaderType.VertexShader);
        OpenGL.ShaderSource(vertexShader, vertexCode);

        OpenGL.CompileShader(vertexShader);

        OpenGL.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + OpenGL.GetShaderInfoLog(vertexShader));

        fragmentShader = OpenGL.CreateShader(ShaderType.FragmentShader);
        OpenGL.ShaderSource(fragmentShader, fragmentCode);
        
        OpenGL.CompileShader(fragmentShader);
        
        OpenGL.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + OpenGL.GetShaderInfoLog(fragmentShader));

        Console.WriteLine("Compiled \"Error Shaders\"");

        Console.WriteLine("Trying To Load \"Error Show Texture\"");

        try
        {
            errorTextTexture = OpenGL.GenTexture();
            OpenGL.ActiveTexture(TextureUnit.Texture0);
            OpenGL.BindTexture(TextureTarget.Texture2D, errorTextTexture);

            // ImageResult.FromMemory reads the bytes of the .png file and returns all its information!
            ImageResult result = ImageResult.FromMemory(File.ReadAllBytes("SneOs.png"), ColorComponents.RedGreenBlueAlpha);

            // Define a pointer to the image data
            fixed (byte* ptr = result.Data)
                // Here we use "result.Width" and "result.Height" to tell OpenGL about how big our texture is.
                OpenGL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
                    (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);

            errorTextLoaded = true;
            Console.WriteLine("Loaded \"Error Show Texture\" Succesfully");
        }catch(Exception e)
        {
            Console.WriteLine($"Was Unable to load \"Error Show Texture\" reson: {e}");
        }

    }

    public static uint GetProgram(GL OpenGL)
    {
        Console.WriteLine("Give Error Program");

        uint ErrorProgramId = OpenGL.CreateProgram();

        // Program Manegment

        OpenGL.AttachShader(ErrorProgramId, vertexShader);
        OpenGL.AttachShader(ErrorProgramId, fragmentShader);

        OpenGL.LinkProgram(ErrorProgramId);

        OpenGL.GetProgram(ErrorProgramId, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + OpenGL.GetProgramInfoLog(ErrorProgramId));


        OpenGL.UseProgram(ErrorProgramId);

        if(errorTextLoaded){
            Console.WriteLine("Trying To bind \"Error Show Texture\"");
            try
            {
                OpenGL.ActiveTexture(GLEnum.Texture0);
                OpenGL.BindTexture(TextureTarget.Texture2D, errorTextTexture);
                int location = OpenGL.GetUniformLocation(ErrorProgramId, "uText");
                OpenGL.Uniform1(location, 0);
                Console.WriteLine("Binded \"Error Show Texture\" Succesfully");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Was Unable to bind \"Error Show Texture\"");
            }
        }

        return ErrorProgramId;
    }
    
}