
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace CloseQuarter.Client.Screens;

public abstract class Screen
{
    protected GL Gl { get; private set; } = null!;
    protected IWindow Window { get; private set; } = null!;

    public virtual void OnLoad(GL gl, IWindow window)
    {
        Gl = gl;
        Window = window;
    }

    public abstract void OnUpdate(double deltaTime);

    public abstract void OnRender(double deltaTime);

    public virtual void OnResize(Silk.NET.Maths.Vector2D<int> newSize) { }

    public abstract void OnUnload();
}