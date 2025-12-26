using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Modedlus.Systems.WindowInterface;

namespace Silk.Net.Interface;

public class SilkWindow : Modedlus.Systems.WindowInterface.Window
{
    public uint Width { get {return (uint)SilkWindowInterface.Size.X;} set { int Width = (int)value; SilkWindowInterface.Size = new Vector2D<int>(Width, SilkWindowInterface.Size.Y); } }
    public uint Height { get {return (uint)SilkWindowInterface.Size.Y;} set { int Height = (int)value; SilkWindowInterface.Size = new Vector2D<int>(SilkWindowInterface.Size.X, Height); } }
    public string Title { get {return SilkWindowInterface.Title;} set { SilkWindowInterface.Title = value; } }
    public uint FrameRate { get {return (uint)SilkWindowInterface.FramesPerSecond;} set {  SilkWindowInterface.FramesPerSecond = value; } }

    private static IWindow SilkWindowInterface;

    private static Thread WindowThread;

    public SilkWindow(int width, int height)
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

        // Starts the window in its own thread
        WindowThread = new Thread(() => SilkWindowInterface.Run());
        WindowThread.Start();
    }
}

