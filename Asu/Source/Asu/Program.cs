using Modedlus.Systems.WindowInterface;
using Silk.Net.Interface;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

Window window = new SilkWindow(800, 600);
window.Width = 1280;
window.Height = 720;
window.Title = "Bert";
window.FrameRate = 120;
