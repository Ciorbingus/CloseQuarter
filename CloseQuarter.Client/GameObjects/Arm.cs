using Silk.NET.OpenGL;
using System.Numerics;

using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models;

public class Arm : IDisposable
{
    private Cube Shoulder;
    private Cube UpperArm;
    private Cube LowerArm;
    private Cube Fist;

    private MyTexture? _bodyTexture;

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public Vector3 ShoulderRotation { get; set; } = Vector3.Zero;
    public Vector3 UpperArmRotation { get; set; } = Vector3.Zero;
    public Vector3 LowerArmRotation { get; set; } = Vector3.Zero;
    public Vector3 FistRotation { get; set; } = Vector3.Zero;


    public Vector3 PivotPoint { get; set; } = Vector3.Zero;



    public Arm(GL gl)
    {
        Shoulder = new Cube(gl, 0.22f, 0.22f, 0.22f, onlyFrontTexture: false);
        UpperArm = new Cube(gl, 0.20f, 0.45f, 0.20f, onlyFrontTexture: false);
        LowerArm = new Cube(gl, 0.18f, 0.40f, 0.18f, onlyFrontTexture: false);
        Fist = new Cube(gl, 0.20f, 0.20f, 0.20f, onlyFrontTexture: false);
    }

    public void LoadTextures(GL gl, string bodyTexturePath)
    {
        _bodyTexture = new MyTexture(gl, bodyTexturePath);
    }

    public Matrix4x4 GetArmMatrix()
    {
        return Matrix4x4.CreateScale(1.0f) *
               Matrix4x4.CreateRotationX(Rotation.X) *
               Matrix4x4.CreateRotationY(Rotation.Y) *
               Matrix4x4.CreateRotationZ(Rotation.Z) *
               Matrix4x4.CreateTranslation(Position);
    }

    public void Render(MyShader shader, Matrix4x4 parentMatrix, Matrix4x4 view, Matrix4x4 projection)
    {
        _bodyTexture?.Bind(TextureUnit.Texture0);

        Matrix4x4 armBase = GetArmMatrix() * parentMatrix;

        Matrix4x4 shoulderMatrix = Matrix4x4.CreateRotationX(ShoulderRotation.X) *
                                   Matrix4x4.CreateRotationY(ShoulderRotation.Y) *
                                   Matrix4x4.CreateRotationZ(ShoulderRotation.Z) * armBase;
        RenderPiece(Shoulder, shoulderMatrix, shader, view, projection);

        Matrix4x4 upperArmMatrix = Matrix4x4.CreateRotationX(UpperArmRotation.X) *
                                   Matrix4x4.CreateRotationY(UpperArmRotation.Y) *
                                   Matrix4x4.CreateRotationZ(UpperArmRotation.Z) *
                                   Matrix4x4.CreateTranslation(0.0f, -0.30f, 0.0f) * shoulderMatrix;
        RenderPiece(UpperArm, upperArmMatrix, shader, view, projection);

        Matrix4x4 lowerArmMatrix = Matrix4x4.CreateRotationX(LowerArmRotation.X) *
                                   Matrix4x4.CreateRotationY(LowerArmRotation.Y) *
                                   Matrix4x4.CreateRotationZ(LowerArmRotation.Z) *
                                   Matrix4x4.CreateTranslation(0.0f, -0.40f, 0.0f) * upperArmMatrix;
        RenderPiece(LowerArm, lowerArmMatrix, shader, view, projection);

        Matrix4x4 fistMatrix = Matrix4x4.CreateRotationX(FistRotation.X) *
                               Matrix4x4.CreateRotationY(FistRotation.Y) *
                               Matrix4x4.CreateRotationZ(FistRotation.Z) *
                               Matrix4x4.CreateTranslation(0.0f, -0.28f, 0.0f) * lowerArmMatrix;
        RenderPiece(Fist, fistMatrix, shader, view, projection);
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
        Shoulder.Dispose();
        UpperArm.Dispose();
        LowerArm.Dispose();
        Fist.Dispose();
        _bodyTexture?.Dispose();
    }
}