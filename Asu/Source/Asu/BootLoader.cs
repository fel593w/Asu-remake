using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Modedlus.Math3D;
using Modedlus.Systems.LoopSystems;
using Modedlus.Systems.RenderInterface;
using Modedlus.Systems.WindowInterface;
using Silk.Net.Interface;

namespace Asu;

public static class BootLoader
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
    const string fragmentCodeA = @"
            #version 330 core

            // Receive the input from the vertex shader in an attribute
            in vec2 frag_texCoords;
            in vec3 frag_normal;

            out vec4 out_color;

            uniform sampler2D uTexture;
            //uniform sampler2D uWhat;
            uniform float sickColors;

            void main()
            {
                // This will allow us to see the texture coordinates in action!
                //out_color = vec4(frag_texCoords.x, frag_texCoords.y, 0.0, 1.0);
                //out_color = vec4(gl_FragCoord.x/800, gl_FragCoord.y /600, gl_FragCoord.z, 1.0);
                //out_color = vec4(gl_FragCoord.z, gl_FragCoord.z, gl_FragCoord.z, 1.0);
                
                // lighting
                float dotProduct = dot(normalize(vec3(1.0, 1.0, -1.0)), normalize(frag_normal));
                float LightAmount = dotProduct*0.8+0.4;
                float productA = (floor(clamp(LightAmount, 0, 1)*4)/3*0.85+0.15)*0.5;
                float productB = clamp(LightAmount, 0, 1)*0.5;
                float productC = productB + productA;
                float product = productC * sickColors;

                
                //out_color = vec4(frag_normal.x, frag_normal.y, frag_normal.z, 1.0);
                out_color = texture(uTexture, frag_texCoords*2)*vec4(product, product, product, 1);
                //out_color = vec4(product, product, product, 1);
            }
            ";

        const string fragmentCodeB = @"
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
    
    
    static GPUTexture GsneOs;
    static GPUTexture GErrorText;
    static GPUProgram GprogA;
    static GPUProgram GprogB;
    static GPUMesh Gmonkey;
    static GPUMesh Gball;
    static SilkWindow silkInterface;
    static Window window;
    static Texture sneOs;
    static Texture ErrorText;
    static Shader shaderA;
    static Shader shaderB;
    static RenderInterface renderInterface;
    static string assemblyFolder;

    static float time;
    static Transform transformA;
    static Transform transformB;
    static PrespectiveCamera camera;

    public static void Boot()
    {
        assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        silkInterface = new SilkWindow(1280, 720, WindowAPI.OpenGL);
        window = silkInterface;
        window.Title = "Åsu!!";

        sneOs = new Texture { FileData = File.ReadAllBytes(assemblyFolder + "/SneOs.png")};
        ErrorText = new Texture { FileData = File.ReadAllBytes(assemblyFolder + "/ErrorText.png")};
        shaderA = new Shader { VertexShader = vertexCode, FragmentShader = fragmentCodeA };
        shaderB = new Shader { VertexShader = vertexCode, FragmentShader = fragmentCodeB };

        renderInterface = (RenderInterface)silkInterface._SilkRenderInterface;

        if(renderInterface == null)
        {
            Console.WriteLine("FAIL: Render Interface Was not pressent");
            return;
        }

        transformA = new LocalTransform{ Position = new Vector3(-1, 0, 0), Scale = new Vector3(0.5f, 0.5f, 0.5f)};
        transformB = new LocalTransform{ Position = new Vector3(1, 0, 0), Scale = new Vector3(0.8f, 0.8f, 0.8f)};
        
        camera = new PrespectiveCamera { location = new LocalTransform { Position = new Vector3(0, 0, -4f), Rotation = Quaternion.Identity, Scale = new Vector3(1f, 1f, 1f) }};
        camera.SetAspectRatio(720, 1280);
        //camera.ModifyZoom(90);

        renderInterface.RenderLoop.OnLoop += Render;
    }

    #region rendering

    static void LoadRender()
    {
        GsneOs = renderInterface.LoadTexture(sneOs, new TextureProperties());
        GErrorText = renderInterface.LoadTexture(ErrorText, new TextureProperties());
        GprogA = renderInterface.LoadShaderProgram(shaderA);
        GprogB = renderInterface.LoadShaderProgram(shaderB);
        Gmonkey = renderInterface.LoadMesh(LoadObj(assemblyFolder + @"/Monkey.obj"));
        Gball = renderInterface.LoadMesh(LoadObj(assemblyFolder + @"/Ball.obj"));
        GprogA.SetUniform("uTexture", GsneOs);
        GprogB.SetUniform("uTexture", GsneOs);
        GprogB.SetUniform("uText", GErrorText);
    }

    static bool isLoaded = false;

    static void Render(object sender, LoopEventArgs _args)
    {
        time += _args.DeltaTime;

        if(renderInterface == null)
        {
            Console.WriteLine("FAIL: Render Interface Was not pressent");
            return;
        }

        // Setups  
        if(!isLoaded)
            LoadRender();
        isLoaded = true;

        float Brightness = 1 - ((float)MathF.Sin(time) *0.5f +0.5f) * 0.8f;
        GprogA.SetUniform("sickColors", 1f);
        //transform.Position = new Vector3((float)MathF.Sin(time*1.8346578f), 0, 0);
        transformA.Rotation = Quaternion.CreateFromYawPitchRoll(time, 0, 0);
        //camera.location.Rotation += Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1);
        GprogA.SetUniform("uModel", transformA.GetTransformMatrix(transformA));
        GprogA.SetUniform("uProjection", camera.GetProjectionMatrix());
        GprogA.SetUniform("uView", camera.GetViewMatrix());
        renderInterface.BindProgram(GprogA);
        renderInterface.DrawMesh(Gmonkey);

        //transform.Position = new Vector3((float)MathF.Sin(time*1.8346578f), 0, 0);
        transformB.Rotation = Quaternion.CreateFromYawPitchRoll(time, 0, 0);
        //camera.location.Rotation += Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1);
        GprogB.SetUniform("uModel", transformB.GetTransformMatrix(transformB));
        GprogB.SetUniform("uProjection", camera.GetProjectionMatrix());
        GprogB.SetUniform("uView", camera.GetViewMatrix());
        GprogB.SetUniform("uTime", time);
        renderInterface.BindProgram(GprogB);
        renderInterface.DrawMesh(Gball);
    }

    #endregion

    static Mesh LoadObj(string path)
    {
        Console.WriteLine($"Loading Mesh at {path}");

        var positions = new List<Vector3>();
        var uvs = new List<Vector2>();
        var normals = new List<Vector3>();

        var vbo = new List<float>();
        var ibo = new List<uint>();
        uint index = 0;
        
        if(!File.Exists(path))
        {
            Console.WriteLine($"File {path} does not exsist");
            return null; }
        

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

        Mesh mesh = new Mesh();

        mesh.Vertices = vbo.ToArray();
        mesh.Indices = ibo.ToArray();

        return mesh;
    }


}