using Silk.NET.OpenGL;
using System.Numerics;

using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models;

public class Leg : IDisposable
{
    private Cube UpperLeg;
    private Cube LowerLeg;
    private Cube Foot;

    private MyTexture? _legTexture;

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public Vector3 UpperLegRotation { get; set; } = Vector3.Zero;
    public Vector3 LowerLegRotation { get; set; } = Vector3.Zero;
    public Vector3 FootRotation { get; set; } = Vector3.Zero;

    public Vector3 PivotPoint { get; set; } = Vector3.Zero;


    public Leg(GL gl)
    {
        UpperLeg = new Cube(gl, 0.22f, 0.45f, 0.22f, onlyFrontTexture: false);
        LowerLeg = new Cube(gl, 0.20f, 0.40f, 0.20f, onlyFrontTexture: false);
        Foot = new Cube(gl, 0.22f, 0.12f, 0.35f, onlyFrontTexture: false);
    }

    public void LoadTextures(GL gl, string legTexturePath)
    {
        _legTexture = new MyTexture(gl, legTexturePath);
    }

    public void Render(MyShader shader, Matrix4x4 parentMatrix, Matrix4x4 view, Matrix4x4 projection)
    {
        _legTexture?.Bind(TextureUnit.Texture0);

        Matrix4x4 legBase = Matrix4x4.CreateRotationX(Rotation.X) *
                            Matrix4x4.CreateRotationY(Rotation.Y) *
                            Matrix4x4.CreateRotationZ(Rotation.Z) *
                            Matrix4x4.CreateTranslation(Position) * parentMatrix;

        Matrix4x4 upperLegMatrix = Matrix4x4.CreateRotationX(UpperLegRotation.X) *
                                   Matrix4x4.CreateRotationY(UpperLegRotation.Y) *
                                   Matrix4x4.CreateRotationZ(UpperLegRotation.Z) * legBase;
        RenderPiece(UpperLeg, upperLegMatrix, shader, view, projection);

        Matrix4x4 lowerLegMatrix = Matrix4x4.CreateRotationX(LowerLegRotation.X) *
                                   Matrix4x4.CreateRotationY(LowerLegRotation.Y) *
                                   Matrix4x4.CreateRotationZ(LowerLegRotation.Z) *
                                   Matrix4x4.CreateTranslation(0.0f, -0.40f, 0.0f) * upperLegMatrix;
        RenderPiece(LowerLeg, lowerLegMatrix, shader, view, projection);

        Matrix4x4 footMatrix = Matrix4x4.CreateRotationX(FootRotation.X) *
                               Matrix4x4.CreateRotationY(FootRotation.Y) *
                               Matrix4x4.CreateRotationZ(FootRotation.Z) *
                               Matrix4x4.CreateTranslation(0.0f, -0.22f, 0.08f) * lowerLegMatrix;
        RenderPiece(Foot, footMatrix, shader, view, projection);
    }

    private void RenderPiece(Cube piece, Matrix4x4 modelMatrix, MyShader shader, Matrix4x4 view, Matrix4x4 projection)
    {
        Matrix4x4 finalModelMatrix = Matrix4x4.CreateScale(piece.Scale) * modelMatrix;
        shader.Use();
        shader.SetUniform("uModel", finalModelMatrix);
        shader.SetUniform("uView", view);
        shader.SetUniform("uProjection", projection);
        piece.Render(shader, view, projection, finalModelMatrix);
    }

    public void Dispose()
    {
        UpperLeg.Dispose();
        LowerLeg.Dispose();
        Foot.Dispose();
        _legTexture?.Dispose();
    }
}