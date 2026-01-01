using Silk.Net.Interface.OpenGl;
using Silk.NET.OpenGL;

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

            uniform sampler2D uTexture;
            uniform sampler2D uText;
            uniform float uTime;

            void main()
            {   
                // lighting
                float dotProduct = dot(normalize(vec3(1.0, 1.0, -1.0)), normalize(frag_normal));
                float LightAmount = dotProduct*0.8+0.4;
                float productA = (floor(clamp(LightAmount, 0, 1)*4)/3*0.85+0.15)*0.5;
                float productB = clamp(LightAmount, 0, 1)*0.5;
                float product = productB + productA;
                vec3 lighting = vec3(product, product, product);
                
                // Texture Maping
                vec4 textColor = texture(uTexture, frag_texCoords*2);

                // Camera based texture maping
                vec2 pixPos = vec2(gl_FragCoord) / vec2(1280, 720);
                vec4 cameraRefrenceColor = texture(uText, (pixPos*vec2(12, -25))+vec2(uTime*0.5, 0));

                // Output
                vec3 modColor = vec3(textColor.x, textColor.y, textColor.z) * lighting;
                modColor = modColor * (1 - cameraRefrenceColor.w); 
                vec3 camColor = vec3(cameraRefrenceColor.x, cameraRefrenceColor.y, cameraRefrenceColor.z);
                // add multiplication if nessesary
                out_color = vec4(modColor + camColor, 1); 
            }
            ";

    public static void Compile(GL OpenGL)
    {

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

        ErrorProgramId = OpenGL.CreateProgram();

        // Program Manegment

        OpenGL.AttachShader(ErrorProgramId, vertexShader);
        OpenGL.AttachShader(ErrorProgramId, fragmentShader);

        OpenGL.LinkProgram(ErrorProgramId);

        OpenGL.GetProgram(ErrorProgramId, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + OpenGL.GetProgramInfoLog(ErrorProgramId));

        OpenGL.DetachShader(ErrorProgramId, vertexShader);
        OpenGL.DetachShader(ErrorProgramId, fragmentShader);
        OpenGL.DeleteShader(vertexShader);
        OpenGL.DeleteShader(fragmentShader);
    }

    public static uint ErrorProgramId;
    
}