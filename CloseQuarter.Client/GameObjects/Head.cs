using Silk.NET.OpenGL;
using System.Numerics;

using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models;

public class Head : IDisposable
{
    private Cube Skull;
    private Cube Nose;

    private MyTexture? _headTexture;

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public Vector3 PivotPoint { get; set; } = new Vector3(0.0f, -0.15f, 0.0f);

    public Head(GL gl)
    {
        Skull = new Cube(gl, 0.30f, 0.30f, 0.30f, onlyFrontTexture: false);
        Nose = new Cube(gl, 0.10f, 0.10f, 0.10f, onlyFrontTexture: false);
    }

    public void LoadTextures(GL gl, string headTexturePath)
    {
        _headTexture = new MyTexture(gl, headTexturePath);
    }

    public void Render(MyShader shader, Matrix4x4 parentMatrix, Matrix4x4 view, Matrix4x4 projection)
    {
        _headTexture?.Bind(TextureUnit.Texture0);

        float rotX = Rotation.X * (MathF.PI / 180.0f);
        float rotY = Rotation.Y * (MathF.PI / 180.0f);
        float rotZ = Rotation.Z * (MathF.PI / 180.0f);

        Matrix4x4 headMatrix = Matrix4x4.CreateTranslation(-PivotPoint) *
                               Matrix4x4.CreateRotationX(rotX) *
                               Matrix4x4.CreateRotationY(rotY) *
                               Matrix4x4.CreateRotationZ(rotZ) *
                               Matrix4x4.CreateTranslation(PivotPoint) *
                               Matrix4x4.CreateTranslation(Position) * parentMatrix;

        RenderPiece(Skull, headMatrix, shader, view, projection);

        Matrix4x4 noseMatrix = Matrix4x4.CreateTranslation(0.0f, 0.0f, 0.20f) * headMatrix;
        RenderPiece(Nose, noseMatrix, shader, view, projection);
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
        Skull.Dispose();
        Nose.Dispose();
        _headTexture?.Dispose();
    }
}