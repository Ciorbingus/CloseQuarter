using Silk.NET.OpenGL;
using System.Numerics;

using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models;

public class Fighter : IDisposable
{
    public Torso Torso { get; private set; }
    public Head Head { get; private set; }
    public Arm LeftArm { get; private set; }
    public Arm RightArm { get; private set; }
    public Leg LeftLeg { get; private set; }
    public Leg RightLeg { get; private set; }

    public Fighter(GL gl)
    {
        Torso = new Torso(gl);
        Head = new Head(gl);
        LeftArm = new Arm(gl);
        RightArm = new Arm(gl);
        LeftLeg = new Leg(gl);
        RightLeg = new Leg(gl);

        LeftLeg.Position  = new Vector3(-0.16f, 0.45f, 0.0f);
        RightLeg.Position = new Vector3(0.16f, 0.45f, 0.0f);
        
        Torso.Position    = new Vector3(0.0f, 1.05f, 0.0f);
        
        LeftArm.Position  = new Vector3(-0.42f, 1.20f, 0.0f);
        RightArm.Position = new Vector3(0.42f, 1.20f, 0.0f);
        
        Head.Position     = new Vector3(0.0f, 1.55f, 0.0f);
    }

    public void LoadTextures(GL gl, string bodyTexturePath, string? headTexturePath = null)
    {
        Torso.LoadTextures(gl, bodyTexturePath);
        LeftArm.LoadTextures(gl, bodyTexturePath);
        RightArm.LoadTextures(gl, bodyTexturePath);
        LeftLeg.LoadTextures(gl, bodyTexturePath);
        RightLeg.LoadTextures(gl, bodyTexturePath);

        if (!string.IsNullOrEmpty(headTexturePath))
            Head.LoadTextures(gl, headTexturePath);
        else
            Head.LoadTextures(gl, bodyTexturePath);
    }

    public void Render(MyShader shader, Matrix4x4 playerBaseMatrix, Matrix4x4 view, Matrix4x4 projection)
    {
        Matrix4x4 chestMatrix = Torso.Render(shader, playerBaseMatrix, view, projection);

        Head.Render(shader, chestMatrix, view, projection);

        LeftArm.Render(shader, chestMatrix, view, projection);
        RightArm.Render(shader, chestMatrix, view, projection);

        LeftLeg.Render(shader, playerBaseMatrix, view, projection);
        RightLeg.Render(shader, playerBaseMatrix, view, projection);
    }

    public void Dispose()
    {
        Torso.Dispose();
        Head.Dispose();
        LeftArm.Dispose();
        RightArm.Dispose();
        LeftLeg.Dispose();
        RightLeg.Dispose();
    }
}