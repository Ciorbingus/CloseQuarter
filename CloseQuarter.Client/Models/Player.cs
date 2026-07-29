using Silk.NET.OpenGL;
using System.Numerics;
using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models;

public class Player : IDisposable
{
    private Cube _head;
    private Cube _torso;
    private Cube _leftArm;
    private Cube _rightArm;
    private Cube _leftLeg;
    private Cube _rightLeg;

    private MyTexture? _bodyTexture;
    private MyTexture? _headTexture;

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public Vector3 RightArmRotation { get; set; } = Vector3.Zero;
    public Vector3 LeftLegRotation { get; set; } = Vector3.Zero;

    public Player(GL gl)
    {
        _torso = new Cube(gl, 0.60f, 1.10f, 0.25f, onlyFrontTexture: false);
        _head = new Cube(gl, 0.50f, 0.50f, 0.50f, onlyFrontTexture: true);
        _leftArm = new Cube(gl, 0.20f, 0.90f, 0.20f, onlyFrontTexture: false);
        _rightArm = new Cube(gl, 0.20f, 0.90f, 0.20f, onlyFrontTexture: false);
        _leftLeg = new Cube(gl, 0.22f, 1.00f, 0.22f, onlyFrontTexture: false);
        _rightLeg = new Cube(gl, 0.22f, 1.00f, 0.22f, onlyFrontTexture: false);
    }

    public void LoadTextures(GL gl, string bodyTexturePath, string? headTexturePath = null)
    {
        _bodyTexture = new MyTexture(gl, bodyTexturePath);

        if (!string.IsNullOrEmpty(headTexturePath))
        {
            _headTexture = new MyTexture(gl, headTexturePath);
        }
    }

    public Matrix4x4 GetBodyMatrix()
    {
        return Matrix4x4.CreateScale(1.0f) *
               Matrix4x4.CreateRotationX(Rotation.X) *
               Matrix4x4.CreateRotationY(Rotation.Y) *
               Matrix4x4.CreateRotationZ(Rotation.Z) *
               Matrix4x4.CreateTranslation(Position);
    }

    public void Render(MyShader shader, Matrix4x4 view, Matrix4x4 projection)
    {
        Matrix4x4 bodyMatrix = GetBodyMatrix();

        _bodyTexture?.Bind(TextureUnit.Texture0);

        Matrix4x4 torsoMatrix = Matrix4x4.CreateTranslation(0.0f, 1.2f, 0.0f) * bodyMatrix;
        RenderPiece(_torso, torsoMatrix, shader, view, projection);

        if (_headTexture != null)
        {
            _headTexture.Bind(TextureUnit.Texture0);
        }
        else
        {
            _bodyTexture?.Bind(TextureUnit.Texture0);
        }

        Matrix4x4 headMatrix = Matrix4x4.CreateTranslation(0.0f, 0.85f, 0.0f) * torsoMatrix;
        RenderPiece(_head, headMatrix, shader, view, projection);

        _bodyTexture?.Bind(TextureUnit.Texture0);

        Matrix4x4 rightArmLocal = Matrix4x4.CreateRotationX(RightArmRotation.X) *
                                  Matrix4x4.CreateRotationY(RightArmRotation.Y) *
                                  Matrix4x4.CreateRotationZ(RightArmRotation.Z) *
                                  Matrix4x4.CreateTranslation(0.52f, 0.0f, 0.0f);

        Matrix4x4 rightArmMatrix = rightArmLocal * torsoMatrix;
        RenderPiece(_rightArm, rightArmMatrix, shader, view, projection);

        Matrix4x4 leftArmMatrix = Matrix4x4.CreateTranslation(-0.52f, 0.0f, 0.0f) * torsoMatrix;
        RenderPiece(_leftArm, leftArmMatrix, shader, view, projection);

        Matrix4x4 leftLegMatrix = Matrix4x4.CreateTranslation(-0.20f, -1.0f, 0.0f) * torsoMatrix;
        RenderPiece(_leftLeg, leftLegMatrix, shader, view, projection);

        Matrix4x4 rightLegMatrix = Matrix4x4.CreateTranslation(0.20f, -1.0f, 0.0f) * torsoMatrix;
        RenderPiece(_rightLeg, rightLegMatrix, shader, view, projection);
    }

    private void RenderPiece(Cube piece, Matrix4x4 parentMatrix, MyShader shader, Matrix4x4 view, Matrix4x4 projection)
    {
        Matrix4x4 finalModelMatrix = Matrix4x4.CreateScale(piece.Scale) * parentMatrix;

        shader.Use();
        shader.SetUniform("uModel", finalModelMatrix);
        shader.SetUniform("uView", view);
        shader.SetUniform("uProjection", projection);

        piece.Render(shader, view, projection, finalModelMatrix);
    }
    public void Dispose()
    {
        _bodyTexture?.Dispose();
        _headTexture?.Dispose();

        _head.Dispose();
        _torso.Dispose();
        _leftArm.Dispose();
        _rightArm.Dispose();
        _leftLeg.Dispose();
        _rightLeg.Dispose();
    }

    public void MoveForward(float distance)
    {
        Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1), Matrix4x4.CreateRotationY(Rotation.Y));
        Position += forward * distance;
    }

    public void MoveBackward(float distance)
    {
        Vector3 backward = Vector3.Transform(new Vector3(0, 0, -1), Matrix4x4.CreateRotationY(Rotation.Y));
        Position += backward * distance;
    }

    public void MoveLeft(float distance)
    {
        Vector3 left = Vector3.Transform(new Vector3(1, 0, 0), Matrix4x4.CreateRotationY(Rotation.Y));
        Position += left * distance;
    }

    public void MoveRight(float distance)
    {
        Vector3 right = Vector3.Transform(new Vector3(-1, 0, 0), Matrix4x4.CreateRotationY(Rotation.Y));
        Position += right * distance;
    }

    public void LookAt(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - Position;
        Rotation = new Vector3(Rotation.X, MathF.Atan2(direction.X, direction.Z), Rotation.Z);
    }

    public void Rotate(float yaw, float pitch)
    {
        Vector3 currentRotation = Rotation;

        currentRotation.Y += yaw;
        currentRotation.X += pitch;
        currentRotation.X = Math.Clamp(currentRotation.X, -MathF.PI / 2.0f, MathF.PI / 2.0f);

        Rotation = currentRotation;
    }
}