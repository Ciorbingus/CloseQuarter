using Silk.NET.OpenGL;
using System.Numerics;

using CloseQuarter.Client.Graphics;
using CloseQuarter.Shared.Models;

namespace CloseQuarter.Client.Models;

public abstract class GameMap : IDisposable
{
    protected GL Gl;

    public Background? Background { get; protected set; }
    public Ground? Ground { get; protected set; }
    public Ring? Ring { get; protected set; }
    
    public MyTexture? RingTexture { get; protected set; }
    public MyTexture? GroundTexture { get; protected set; }
    public MyShader? MapShader { get; protected set; } 

    public List<Model3D> DecorObjects { get; protected set; } = new();

    public string MapName { get; protected set; } = "Base Map";
    public float RingRadius { get; protected set; } = 7.0f;

    protected GameMap(GL gl)
    {
        Gl = gl;
    }

    public abstract void LoadResources();

    public virtual void Render(Matrix4x4 view, Matrix4x4 projection)
    {
        Background?.Render();

        Gl.Enable(EnableCap.DepthTest);

        if (MapShader != null)
        {
            if (Ground != null && GroundTexture != null)
            {
                GroundTexture.Bind(TextureUnit.Texture0);
                Ground.Render(MapShader, view, projection);
            }

            if (Ring != null && RingTexture != null)
            {
                RingTexture.Bind(TextureUnit.Texture0);
                Ring.Render(MapShader, view, projection);
            }
        }
    }

    public virtual void Dispose()
    {
        Background?.Dispose();
        Ground?.Dispose();
        Ring?.Dispose();
        MapShader?.Dispose();
        RingTexture?.Dispose();
        GroundTexture?.Dispose();

        
    }
}