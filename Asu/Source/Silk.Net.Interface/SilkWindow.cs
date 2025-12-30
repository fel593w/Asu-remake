using Silk.NET.Maths;
using Silk.NET.Windowing;
using Modedlus.Systems.RenderInterface;
using Modedlus.Systems.LoopSystems;
using Silk.Net.Interface.OpenGl;
using System.Diagnostics;

namespace Silk.Net.Interface;

public class SilkWindow : Modedlus.Systems.WindowInterface.Window, IDisposable
{

    private Thread WindowThread;

    public SilkWindow(int width, int height, WindowAPI RenderAPI)
    {
        Console.WriteLine("Creating window");
        // Configure Input Value
        int _Width = width;
        int _Height = height;

        Console.WriteLine("Creating window");
        // Set Base Values
        WindowOptions options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(_Width, _Height),
            Title = "Modedlus Aplication",
            FramesPerSecond = 500,
            UpdatesPerSecond = 1,
            WindowBorder = WindowBorder.Resizable,

        };

        Console.WriteLine("Creating window");
        // Create the window Interface
        SilkWindowInterface = Silk.NET.Windowing.Window.Create(options);

        Console.WriteLine("Seting Up Render Api");
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



        bool isLoaded = false;
        void Load()
        {
            isLoaded = true;
        }
        // Adds the load func
        SilkWindowInterface.Load += Load;


        Console.WriteLine("Creating Window Thread");
        // Starts the window in its own thread
        WindowThread = new Thread(new ThreadStart(SilkWindowInterface.Run));
        WindowThread.Start();
        
        // Wait until it is done
        Console.WriteLine("Wait for initalization");
        while (!isLoaded) { Thread.Sleep (100); }
        
        SilkWindowInterface.MakeCurrent();

        // it is done
        Console.WriteLine("Silk Window Finished");
        return;
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

    public SilkRenderInterface _SilkRenderInterface { get; private set; }

    #region OpenGL API

    private void SetupOpenGL()
    {
        void LoadOpenGl()
        {
            Console.WriteLine("Seting up OpenGL for silk.net");
            _SilkRenderInterface = (SilkRenderInterface)new OpenGLRenderInterface(SilkWindowInterface);
            if(_SilkRenderInterface == null)
                Console.WriteLine("silk.net OpenGL startup was not sucsessfull");
        }
        // Adds the load func
        SilkWindowInterface.Load += LoadOpenGl;
    }

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

