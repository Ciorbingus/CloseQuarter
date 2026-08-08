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

    public Vector3 UpperArmPivot { get; set; } = new Vector3(0.0f, 0.225f, 0.0f); 
    public Vector3 LowerArmPivot { get; set; } = new Vector3(0.0f, 0.20f, 0.0f);  
    public Vector3 FistPivot { get; set; } = new Vector3(0.0f, 0.10f, 0.0f);     

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

    private Matrix4x4 CreateTransformWithPivot(Vector3 rotation, Vector3 pivot, Vector3 translation)
    {
        float rotX = rotation.X * (MathF.PI / 180.0f);
        float rotY = rotation.Y * (MathF.PI / 180.0f);
        float rotZ = rotation.Z * (MathF.PI / 180.0f);

        return Matrix4x4.CreateTranslation(-pivot) *
               Matrix4x4.CreateRotationX(rotX) *
               Matrix4x4.CreateRotationY(rotY) *
               Matrix4x4.CreateRotationZ(rotZ) *
               Matrix4x4.CreateTranslation(pivot) *
               Matrix4x4.CreateTranslation(translation);
    }

    public void Render(MyShader shader, Matrix4x4 parentMatrix, Matrix4x4 view, Matrix4x4 projection)
    {
        _bodyTexture?.Bind(TextureUnit.Texture0);

        float armRotX = Rotation.X * (MathF.PI / 180.0f);
        float armRotY = Rotation.Y * (MathF.PI / 180.0f);
        float armRotZ = Rotation.Z * (MathF.PI / 180.0f);

        Matrix4x4 armBase = Matrix4x4.CreateTranslation(-PivotPoint) *
                            Matrix4x4.CreateRotationX(armRotX) *
                            Matrix4x4.CreateRotationY(armRotY) *
                            Matrix4x4.CreateRotationZ(armRotZ) *
                            Matrix4x4.CreateTranslation(PivotPoint) *
                            Matrix4x4.CreateTranslation(Position) * parentMatrix;

        Matrix4x4 shoulderMatrix = CreateTransformWithPivot(ShoulderRotation, Vector3.Zero, Vector3.Zero) * armBase;
        RenderPiece(Shoulder, shoulderMatrix, shader, view, projection);

        Matrix4x4 upperArmMatrix = CreateTransformWithPivot(UpperArmRotation, UpperArmPivot, new Vector3(0.0f, -0.30f, 0.0f)) * shoulderMatrix;
        RenderPiece(UpperArm, upperArmMatrix, shader, view, projection);

        Matrix4x4 lowerArmMatrix = CreateTransformWithPivot(LowerArmRotation, LowerArmPivot, new Vector3(0.0f, -0.40f, 0.0f)) * upperArmMatrix;
        RenderPiece(LowerArm, lowerArmMatrix, shader, view, projection);

        Matrix4x4 fistMatrix = CreateTransformWithPivot(FistRotation, FistPivot, new Vector3(0.0f, -0.28f, 0.0f)) * lowerArmMatrix;
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