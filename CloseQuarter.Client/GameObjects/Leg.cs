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

    public Vector3 UpperLegPivot { get; set; } = new Vector3(0.0f, 0.30f, 0.0f);  
    public Vector3 LowerLegPivot { get; set; } = new Vector3(0.0f, 0.275f, 0.0f); 
    public Vector3 FootPivot { get; set; } = new Vector3(0.0f, 0.06f, -0.10f);    

    public Leg(GL gl)
    {
        UpperLeg = new Cube(gl, 0.22f, 0.60f, 0.22f, onlyFrontTexture: false);
        LowerLeg = new Cube(gl, 0.20f, 0.55f, 0.20f, onlyFrontTexture: false);
        Foot = new Cube(gl, 0.22f, 0.12f, 0.35f, onlyFrontTexture: false);
    }

    public void LoadTextures(GL gl, string legTexturePath)
    {
        _legTexture = new MyTexture(gl, legTexturePath);
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
        _legTexture?.Bind(TextureUnit.Texture0);

        float legRotX = Rotation.X * (MathF.PI / 180.0f);
        float legRotY = Rotation.Y * (MathF.PI / 180.0f);
        float legRotZ = Rotation.Z * (MathF.PI / 180.0f);

        Matrix4x4 legBase = Matrix4x4.CreateTranslation(-PivotPoint) *
                            Matrix4x4.CreateRotationX(legRotX) *
                            Matrix4x4.CreateRotationY(legRotY) *
                            Matrix4x4.CreateRotationZ(legRotZ) *
                            Matrix4x4.CreateTranslation(PivotPoint) *
                            Matrix4x4.CreateTranslation(Position) * parentMatrix;

        Matrix4x4 upperLegMatrix = CreateTransformWithPivot(UpperLegRotation, UpperLegPivot, Vector3.Zero) * legBase;
        RenderPiece(UpperLeg, upperLegMatrix, shader, view, projection);

        Matrix4x4 lowerLegMatrix = CreateTransformWithPivot(LowerLegRotation, LowerLegPivot, new Vector3(0.0f, -0.55f, 0.0f)) * upperLegMatrix;
        RenderPiece(LowerLeg, lowerLegMatrix, shader, view, projection);

        Matrix4x4 footMatrix = CreateTransformWithPivot(FootRotation, FootPivot, new Vector3(0.0f, -0.30f, 0.08f)) * lowerLegMatrix;
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