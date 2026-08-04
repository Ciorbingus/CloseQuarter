using Silk.NET.OpenGL;

using CloseQuarter.Client.Graphics;
using CloseQuarter.Client.Models;

namespace CloseQuarter.Client.Maps;

public class TestMap : GameMap
{
    public TestMap(GL gl) : base(gl)
    {
        MapName = "Test Arena";
        RingRadius = 7.0f;
    }

    public override void LoadResources()
    {
        Background = new Background(Gl);
        Background.Initialize("Textures/sky.jpeg");

        GroundTexture = new MyTexture(Gl, "Textures/grass.jpg"); 
        RingTexture = new MyTexture(Gl, "Textures/ring.jpeg");   

        MapShader = MyShader.FromFiles(Gl, "Shaders/ring.vert", "Shaders/ring.frag");

        Ground = new Ground(Gl);
        Ground.Initialize(160.0f);

        Ring = new Ring(Gl);
        Ring.Initialize(radius: RingRadius, segments: 64);
        
    }
}