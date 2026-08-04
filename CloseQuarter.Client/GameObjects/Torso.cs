using Silk.NET.OpenGL;
using System.Numerics;

using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models;

public class Torso : IDisposable
{
    private Cube Neck;
    private Cube Chest;
    private Cube Abdomen;

    private MyTexture? _bodyTexture;

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public Vector3 ChestRotation { get; set; } = Vector3.Zero;
    public Vector3 AbdomenRotation { get; set; } = Vector3.Zero;

    public Vector3 PivotPoint { get; set; } = Vector3.Zero;


    public Torso(GL gl)
    {
        Neck = new Cube(gl, 0.20f, 0.15f, 0.20f, onlyFrontTexture: false);
        Chest = new Cube(gl, 0.60f, 0.50f, 0.25f, onlyFrontTexture: false);
        Abdomen = new Cube(gl, 0.50f, 0.45f, 0.25f, onlyFrontTexture: false);
    }

    public void LoadTextures(GL gl, string bodyTexturePath)
    {
        _bodyTexture = new MyTexture(gl, bodyTexturePath);
    }

    public Matrix4x4 Render(MyShader shader, Matrix4x4 parentMatrix, Matrix4x4 view, Matrix4x4 projection)
    {
        _bodyTexture?.Bind(TextureUnit.Texture0);

        Matrix4x4 torsoBase = Matrix4x4.CreateRotationX(Rotation.X) *
                              Matrix4x4.CreateRotationY(Rotation.Y) *
                              Matrix4x4.CreateRotationZ(Rotation.Z) *
                              Matrix4x4.CreateTranslation(Position) * parentMatrix;

        Matrix4x4 abdomenMatrix = Matrix4x4.CreateRotationX(AbdomenRotation.X) *
                                  Matrix4x4.CreateRotationY(AbdomenRotation.Y) *
                                  Matrix4x4.CreateRotationZ(AbdomenRotation.Z) * torsoBase;
        RenderPiece(Abdomen, abdomenMatrix, shader, view, projection);

        Matrix4x4 chestMatrix = Matrix4x4.CreateRotationX(ChestRotation.X) *
                                Matrix4x4.CreateRotationY(ChestRotation.Y) *
                                Matrix4x4.CreateRotationZ(ChestRotation.Z) *
                                Matrix4x4.CreateTranslation(0.0f, 0.45f, 0.0f) * abdomenMatrix;
        RenderPiece(Chest, chestMatrix, shader, view, projection);

        Matrix4x4 neckMatrix = Matrix4x4.CreateTranslation(0.0f, 0.30f, 0.0f) * chestMatrix;
        RenderPiece(Neck, neckMatrix, shader, view, projection);

        return chestMatrix;
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
        Neck.Dispose();
        Chest.Dispose();
        Abdomen.Dispose();
        _bodyTexture?.Dispose();
    }
}