using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using CloseQuarter.Client.Models;

namespace CloseQuarter.Client.Managers;

public static class ScreenManager
{
    public static Screen? CurrentScreen { get; private set; }
    private static GL? _gl;
    private static IWindow? _window;

    public static void Initialize(GL gl, IWindow window)
    {
        _gl = gl;
        _window = window;
    }

    public static void ChangeScreen(Screen newScreen)
    {
        if (_gl == null || _window == null)
            throw new InvalidOperationException("ScreenManager not initialized.");

        CurrentScreen?.OnUnload();

        CurrentScreen = newScreen;
        CurrentScreen.OnLoad(_gl, _window);
    }
}