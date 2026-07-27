using System.Numerics;

namespace CloseQuarter.Client.Models;

public class Camera
{
    public Vector3 Position { get; set; }
    public Vector3 Target { get; set; }
    public Vector3 Up { get; set; }
    public float FieldOfView { get; set; }
    public float AspectRatio { get; set; }
    public float NearPlane { get; set; }
    public float FarPlane { get; set; }

    public float Yaw { get; set; } 
    public float Pitch { get; set; } = 0.0f;   

    public Camera(Vector3 position, Vector3 target, Vector3 up, float fieldOfView, float aspectRatio, float nearPlane = 0.1f, float farPlane = 100.0f)
    {
        Position = position;
        Target = target;
        Up = up;
        FieldOfView = fieldOfView;
        AspectRatio = aspectRatio;
        NearPlane = nearPlane;
        FarPlane = farPlane;

        Vector3 offset = Position - Target;
        Yaw = MathF.Atan2(offset.Z, offset.X) * (180.0f / MathF.PI);
    }

    public Camera(float aspectRatio) 
        : this(new Vector3(0.0f, 4.0f, 12.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitY, MathF.PI / 4.0f, aspectRatio)
    {
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, Target, Up);
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
    }

    
    public Vector3 GetForwardVector()
    {
        Vector3 forward = Target - Position;
        forward.Y = 0f; 
        return Vector3.Normalize(forward);
    }

    public Vector3 GetRightVector()
    {
        return Vector3.Normalize(Vector3.Cross(GetForwardVector(), Up));
    }



    public void MoveForward(float distance)
    {
        Vector3 moveVector = GetForwardVector() * distance;
        Position += moveVector;
        Target += moveVector;
    }

    public void MoveBackward(float distance) => MoveForward(-distance);

    public void MoveRight(float distance)
    {
        Vector3 moveVector = GetRightVector() * distance;
        Position += moveVector;
        Target += moveVector;
    }

    public void MoveLeft(float distance) => MoveRight(-distance);


    public void MoveUp(float distance)
    {
        Vector3 moveVector = Up * distance;
        Position += moveVector;
        Target += moveVector;
    }

    public void MoveDown(float distance) => MoveUp(-distance);

    public void RotateAroundTarget(float angleInDegrees)
    {
        Vector3 offset = Position - Target;
        float radius = new Vector2(offset.X, offset.Z).Length();

        Yaw += angleInDegrees;
        float radians = Yaw * (MathF.PI / 180.0f);

        float newX = Target.X + radius * MathF.Cos(radians);
        float newZ = Target.Z + radius * MathF.Sin(radians);

        Position = new Vector3(newX, Position.Y, newZ);
    }

    public void UpdateAspectRatio(float aspectRatio) => AspectRatio = aspectRatio;
    public void UpdatePosition(Vector3 position) => Position = position;
    public void UpdateTarget(Vector3 target) => Target = target;
}