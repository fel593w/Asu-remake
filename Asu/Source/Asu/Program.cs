using Modedlus.Systems.WindowInterface;
using Silk.Net.Interface;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

Window windowA = new SilkWindow(800, 600, WindowAPI.OpenGL);
windowA.Width = 1280;
windowA.Height = 720;
windowA.Title = "Åsu!!";
windowA.FrameRate = 120;
