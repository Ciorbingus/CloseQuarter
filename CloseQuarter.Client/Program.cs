using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Input;
using CloseQuarter.Client.Models;
using CloseQuarter.Client.Managers;


namespace CloseQuarter.Client;

class Program
{
    private static IWindow _window = null!;
    private static GL _gl = null!;
    public static ImGuiController ImGuiController { get; private set; } = null!;
    private static IInputContext _inputContext = null!;

    static void Main(string[] args)
    {
        try
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1280, 720);
            options.Title = "Joc Boss Faraonic";
            options.VSync = true;
            options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Default, new APIVersion(3, 3));

            _window = Window.Create(options);

            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.FramebufferResize += OnResize;
            _window.Closing += OnClosing;

            Console.WriteLine("[System] Starting Close Quarter Client...");
            _window.Run();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[Critical Error]: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
    }

    private static void OnLoad()
    {
        _gl = _window.CreateOpenGL();
        _gl.Enable(EnableCap.DepthTest);

        _inputContext = _window.CreateInput();

        ImGuiController = new ImGuiController(_gl, _window, _inputContext);

        ScreenManager.Initialize(_gl, _window);
        ScreenManager.ChangeScreen(new TestScreen());

        AudioManager.Initialize();
    }

    private static void OnUpdate(double deltaTime)
    {
        ImGuiController.Update((float)deltaTime);

        ScreenManager.CurrentScreen?.OnUpdate(deltaTime);
    }

    private static void OnRender(double deltaTime)
    {
        ScreenManager.CurrentScreen?.OnRender(deltaTime);

        ImGuiController.Render();
    }

    private static void OnResize(Vector2D<int> newSize)
    {
        if (_gl != null && newSize.X > 0 && newSize.Y > 0)
        {
            _gl.Viewport(newSize);
            ScreenManager.CurrentScreen?.OnResize(newSize);
        }
    }

    private static void OnClosing()
    {
        ScreenManager.CurrentScreen?.OnUnload();
        
        ImGuiController?.Dispose();
        _inputContext?.Dispose();

        AudioManager.Shutdown();
    }
}