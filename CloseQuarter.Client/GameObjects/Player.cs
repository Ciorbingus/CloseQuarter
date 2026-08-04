using Silk.NET.OpenGL;
using System.Numerics;
using ImGuiNET;

using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models;

public enum PlayerState
{
    Idle,
    Walking,
    Running,
    Jumping,
    Falling,
    Rising,
    Crouching,
    SidestepLeft,
    SidestepRight,
    Attacking,
    Defending,
    Downed,
    KnockedOut
}

public class Player : IDisposable
{
    public Fighter FighterModel { get; private set; }

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public float VelocityY { get; set; } = 0.0f;
    public bool IsGrounded { get; set; } = true;
    public bool IsRunning { get; set; } = false;
    public float RunSpeedMultiplier { get; set; } = 1.8f;

    public PlayerState CurrentState { get; set; } = PlayerState.Idle;
    public PlayerState PreviousState { get; set; } = PlayerState.Idle;

    private Vector3 _sidestepVelocity = Vector3.Zero;
    public bool IsSidestepping => _sidestepVelocity.LengthSquared() > 0.01f;

    public ImGuiKey ForwardKey { get; private set; } = ImGuiKey.RightArrow;
    public ImGuiKey BackwardKey { get; private set; } = ImGuiKey.LeftArrow;
    public ImGuiKey LeftKey { get; private set; } = ImGuiKey.UpArrow;
    public ImGuiKey RightKey { get; private set; } = ImGuiKey.DownArrow;

    public ImGuiKey PunchKey { get; set; } = ImGuiKey.B;

    public Player(GL gl)
    {
        FighterModel = new Fighter(gl);
    }

    public void LoadTextures(GL gl, string bodyTexturePath, string? headTexturePath = null)
    {
        FighterModel.LoadTextures(gl, bodyTexturePath, headTexturePath);
    }

    public void UpdateDynamicKeys(float directionFactor)
    {
        if (directionFactor >= 0f)
        {
            ForwardKey = ImGuiKey.RightArrow;
            BackwardKey = ImGuiKey.LeftArrow;
            LeftKey = ImGuiKey.UpArrow;
            RightKey = ImGuiKey.DownArrow;
        }
        else
        {
            ForwardKey = ImGuiKey.LeftArrow;
            BackwardKey = ImGuiKey.RightArrow;
            LeftKey = ImGuiKey.DownArrow;
            RightKey = ImGuiKey.UpArrow;
        }
    }

    public Matrix4x4 GetBodyMatrix()
    {
        float rotX = Rotation.X * (MathF.PI / 180.0f);
        float rotY = Rotation.Y * (MathF.PI / 180.0f);
        float rotZ = Rotation.Z * (MathF.PI / 180.0f);

        return Matrix4x4.CreateScale(1.0f) *
               Matrix4x4.CreateRotationX(rotX) *
               Matrix4x4.CreateRotationY(rotY) *
               Matrix4x4.CreateRotationZ(rotZ) *
               Matrix4x4.CreateTranslation(Position);
    }

    public void Render(MyShader shader, Matrix4x4 view, Matrix4x4 projection)
    {
        FighterModel.Render(shader, GetBodyMatrix(), view, projection);
    }

    public void LookAt(Vector3 targetPosition)
    {
        Vector3 direction = Vector3.Normalize(targetPosition - Position);
        float yaw = MathF.Atan2(direction.X, direction.Z) * (180.0f / MathF.PI);
        float pitch = MathF.Asin(direction.Y) * (180.0f / MathF.PI);

        Rotation = new Vector3(pitch, yaw, 0.0f);
    }

    public void MoveForward(float distance)
    {
        float currentDistance = IsRunning ? distance * RunSpeedMultiplier : distance;
        float yawInRadians = Rotation.Y * (MathF.PI / 180.0f);
        Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1), Matrix4x4.CreateRotationY(yawInRadians));
        Position += forward * currentDistance;
    }

    public void MoveBackward(float distance)
    {
        float currentDistance = IsRunning ? distance * RunSpeedMultiplier : distance;
        float yawInRadians = Rotation.Y * (MathF.PI / 180.0f);
        Vector3 backward = Vector3.Transform(new Vector3(0, 0, -1), Matrix4x4.CreateRotationY(yawInRadians));
        Position += backward * currentDistance;
    }

    public void UpdateSidestep(float deltaTime)
    {
        if (IsSidestepping)
        {
            Position += _sidestepVelocity * deltaTime;
            _sidestepVelocity = Vector3.Lerp(_sidestepVelocity, Vector3.Zero, 15.0f * deltaTime);
        }
    }

    public void SidestepLeft(float force = 8.0f)
    {
        float yawInRadians = Rotation.Y * (MathF.PI / 180.0f);
        Vector3 leftDirection = Vector3.Transform(new Vector3(1, 0, 0), Matrix4x4.CreateRotationY(yawInRadians));
        _sidestepVelocity = leftDirection * force;
    }

    public void SidestepRight(float force = 8.0f)
    {
        float yawInRadians = Rotation.Y * (MathF.PI / 180.0f);
        Vector3 rightDirection = Vector3.Transform(new Vector3(-1, 0, 0), Matrix4x4.CreateRotationY(yawInRadians));
        _sidestepVelocity = rightDirection * force;
    }

    public void Jump(float jumpForce = 8.0f)
    {
        if (IsGrounded)
        {
            VelocityY = jumpForce;
            IsGrounded = false;
        }
    }

    public void Dispose()
    {
        FighterModel.Dispose();
    }
}