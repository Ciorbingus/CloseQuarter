using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Input;
using System.Drawing;

namespace CloseQuarter.Client;

class Program
{
    private static IWindow _window = null!;
    private static GL _gl = null!;

    static void Main(string[] args)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "CloseQuarter - Client";
        options.VSync = true; 

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += OnResize;

        _window.Run();
    }

    private static void OnLoad()
    {
        _gl = _window.CreateOpenGL();

        _gl.ClearColor(Color.FromArgb(255, 25, 25, 35));

        _gl.Enable(EnableCap.DepthTest);

        Console.WriteLine($"Hello!");
    }

    private static void OnUpdate(double deltaTime)
    {
        
    }

    private static void OnRender(double deltaTime)
    {
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

    }

    private static void OnResize(Vector2D<int> newSize)
    {
        _gl.Viewport(newSize);
    }
}