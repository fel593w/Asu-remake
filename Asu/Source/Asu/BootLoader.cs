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

            // Add an output variable to pass the texture coordinate to the fragment shader
            // This variable stores the data that we want to be received by the fragment
            out vec2 frag_texCoords;
            out vec3 frag_normal;

            void main()
            {  
                gl_Position = uModel * vec4(aPosition, 1.0);

                // Assigin the texture coordinates without any modification to be recived in the fragment
                frag_texCoords = aTextureCoord;
                vec4 transformNormal = normalize(transpose(inverse(uModel))* vec4(aNormal, 1.0));
                frag_normal = vec3(transformNormal.x, transformNormal.y, transformNormal.z);
            }
            ";
    const string fragmentCode = @"
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
                //float productA = (floor(clamp((dotProduct+0.65)*0.6, 0, 1)*6)/5*0.85+0.15)*0.7;
                float productB = clamp((dotProduct+0.65)*0.6, 0, 1)*1;
                float productC = productB;
                float product = productC * sickColors;

                
                //out_color = vec4(frag_normal.x, frag_normal.y, frag_normal.z, 1.0);
                out_color = texture(uTexture, frag_texCoords*2)*vec4(product, product, product, 1);
                //out_color = vec4(product, product, product, 1);
            }
            ";
    
    
    static GPUTexture GsneOs;
    static GPUProgram Gprog;
    static GPUMesh Gmonkey;
    static SilkWindow silkInterface;
    static Window window;
    static Texture sneOs;
    static Shader shader;
    static RenderInterface renderInterface;
    static string assemblyFolder;

    static float time;
    static Transform transform;

    public static void Boot()
    {
        assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        silkInterface = new SilkWindow(800, 600, WindowAPI.OpenGL);
        window = silkInterface;
        window.Width = 1280;
        window.Height = 720;
        window.Title = "Åsu!!";

        sneOs = new Texture { FileData = File.ReadAllBytes(assemblyFolder + "/SneOs.png")};
        shader = new Shader { VertexShader = vertexCode, FragmentShader = fragmentCode };

        renderInterface = (RenderInterface)silkInterface._SilkRenderInterface;

        if(renderInterface == null)
        {
            Console.WriteLine("FAIL: Render Interface Was not pressent");
            return;
        }

        transform = new LocalTransform{ Position = new Vector3(0.5f, 0, 0), Scale = new Vector3(0.5f, 0.5f, 0.5f)};

        renderInterface.RenderLoop.OnLoop += Render;
    }

    #region rendering

    static void LoadRender()
    {
        GsneOs = renderInterface.LoadTexture(sneOs, new TextureProperties());
        Gprog = renderInterface.LoadShaderProgram(shader);
        Gprog.SetUniform("uTexture", GsneOs);
        Gmonkey = renderInterface.LoadMesh(LoadObj(assemblyFolder + @"/Monkey.obj"));
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
        Gprog.SetUniform("sickColors", Brightness);
        transform.Position = new Vector3((float)MathF.Sin(time*1.8346578f), 0, 0);
        transform.Rotation = new Vector3(0, time, 0);
        Gprog.SetUniform("uModel", transform.GetTransformMatrix(transform));
        renderInterface.BindProgram(Gprog);
        renderInterface.DrawMesh(Gmonkey);
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