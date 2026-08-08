using Silk.NET.OpenGL;
using System.Numerics;

using Silk.NET.Input;

using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models;

public enum PlayerState
{
    Idle,
    Walking,
    Running,
    SideWalkingLeft,
    SideWalkingRight,
    FrontDashing,
    BackDashing,
    Jumping,
    JumpingForward,
    JumpingBackward,
    Falling,
    Rising,
    Crouching,
    SidestepLeft,
    SidestepRight,
    Attacking,
    Defending,
    Downed,
    Hit,
    KnockedOut
}

public enum AttackType
{
    None,
    S1,
    D1,
    S2,
    D2
}

public class Player : IDisposable
{
    public Fighter FighterModel { get; private set; }

    public const float NormalHeight = 1.8f;
    public const float CrouchHeight = 1.0f;

    public float Radius { get; set; } = 0.3f;
    public float Height { get; set; } = NormalHeight;

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public float VelocityY { get; set; } = 0.0f;
    public bool IsGrounded { get; set; } = true;
    public bool IsRunning { get; set; } = false;
    public float RunSpeedMultiplier { get; set; } = 1.8f;

    public PlayerState CurrentState { get; set; } = PlayerState.Idle;
    public PlayerState PreviousState { get; set; } = PlayerState.Idle;

    private Vector3 _dashVelocity = Vector3.Zero;
    public bool IsDashing => _dashVelocity.LengthSquared() > 0.01f;

    public Vector3 OpponentPosition { get; set; } = Vector3.Zero;

    public Key ForwardKey { get; private set; } = Key.Right;
    public Key BackwardKey { get; private set; } = Key.Left;
    public Key LeftKey { get; private set; } = Key.Up;
    public Key RightKey { get; private set; } = Key.Down;

    public Key PunchKey { get; set; } = Key.Z;
    public Key RightPunchKey { get; set; } = Key.X;

    public bool IsAttacking => CurrentState == PlayerState.Attacking;
    public bool HasHitCurrentAttack { get; set; } = false;

    private int _attackFrameCounter = 0;
    private const int StartupFrames = 4;
    private const int ActiveFrames = 3;
    private const int RecoveryFrames = 5;
    private const int TotalAttackFrames = StartupFrames + ActiveFrames + RecoveryFrames;

    private float _walkTimeTimer = 0.0f;
    private float _idleTimeTimer = 0.0f;

    public AttackType CurrentAttackType { get; private set; } = AttackType.None;

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
        LeftKey = Key.Up;
        RightKey = Key.Down;

        if (directionFactor >= 0f)
        {
            ForwardKey = Key.Right;
            BackwardKey = Key.Left;
        }
        else
        {
            ForwardKey = Key.Left;
            BackwardKey = Key.Right;
        }
    }

    public bool IsAnimationLocked => IsAttacking || CurrentState == PlayerState.Hit || CurrentState == PlayerState.KnockedOut;

    public bool CanCancelCurrentState(PlayerState newState)
    {
        if (!IsGrounded) return false;

        if (CurrentState == PlayerState.Walking || CurrentState == PlayerState.SideWalkingLeft || CurrentState == PlayerState.SideWalkingRight)
            return true;

        if (IsDashing && _dashVelocity.LengthSquared() < 4.0f && newState == PlayerState.Attacking)
            return true;

        if (IsAttacking && _attackFrameCounter > (StartupFrames + ActiveFrames))
            return true;

        return false;
    }



    public void SetCrouchState(bool isCrouching)
    {
        if (isCrouching)
        {
            if (CurrentState != PlayerState.Jumping && CurrentState != PlayerState.Falling && !IsAttacking)
            {
                CurrentState = PlayerState.Crouching;
                Height = CrouchHeight;
            }
        }
        else
        {
            Height = NormalHeight;
            if (CurrentState == PlayerState.Crouching)
            {
                CurrentState = PlayerState.Idle;
            }
        }
    }

    public void SetOpponentPosition(Vector3 opponentPosition)
    {
        OpponentPosition = opponentPosition;
    }

    public Matrix4x4 GetBodyMatrix()
    {
        float rotX = Rotation.X * (MathF.PI / 180.0f);
        float rotY = Rotation.Y * (MathF.PI / 180.0f);
        float rotZ = Rotation.Z * (MathF.PI / 180.0f);

        float yOffset = CurrentState == PlayerState.Crouching ? -0.4f : 0.0f;
        if (CurrentState == PlayerState.Downed || CurrentState == PlayerState.KnockedOut)
        {
            yOffset = -0.85f;
        }

        Vector3 visualPosition = Position + new Vector3(0.0f, yOffset, 0.0f);

        return Matrix4x4.CreateScale(1.0f) *
               Matrix4x4.CreateRotationX(rotX) *
               Matrix4x4.CreateRotationY(rotY) *
               Matrix4x4.CreateRotationZ(rotZ) *
               Matrix4x4.CreateTranslation(visualPosition);
    }

    public void Render(MyShader shader, Matrix4x4 view, Matrix4x4 projection)
    {
        FighterModel.Render(shader, GetBodyMatrix(), view, projection);
    }

    public void LookAt(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - Position;

        Vector3 directionXZ = new Vector3(direction.X, 0.0f, direction.Z);
        if (directionXZ.LengthSquared() > 0.001f)
        {
            directionXZ = Vector3.Normalize(directionXZ);
            float yaw = MathF.Atan2(directionXZ.X, directionXZ.Z) * (180.0f / MathF.PI);

            Rotation = new Vector3(0.0f, yaw, 0.0f);
        }

        float distanceXZ = directionXZ.Length();
        if (distanceXZ > 0.001f)
        {
            float eyeLevelY = Position.Y + 1.55f;
            float deltaY = targetPosition.Y + 1.20f - eyeLevelY;

            float pitchInDegrees = MathF.Atan2(deltaY, distanceXZ) * (180.0f / MathF.PI);

            pitchInDegrees = Math.Clamp(pitchInDegrees, -45.0f, 45.0f);

            FighterModel.Head.Rotation = new Vector3(-pitchInDegrees, FighterModel.Head.Rotation.Y, FighterModel.Head.Rotation.Z);
        }
    }

    public void MoveForward(float distance)
    {
        if (!IsGrounded) return;

        SetCrouchState(false);
        CurrentState = IsRunning ? PlayerState.Running : PlayerState.Walking;

        float currentDistance = IsRunning ? distance * RunSpeedMultiplier : distance;
        float yawInRadians = Rotation.Y * (MathF.PI / 180.0f);
        Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1), Matrix4x4.CreateRotationY(yawInRadians));
        Position += forward * currentDistance;
    }

    public void MoveBackward(float distance)
    {
        if (!IsGrounded) return;

        SetCrouchState(false);
        CurrentState = IsRunning ? PlayerState.Running : PlayerState.Walking;

        float currentDistance = IsRunning ? distance * RunSpeedMultiplier : distance;
        float yawInRadians = Rotation.Y * (MathF.PI / 180.0f);
        Vector3 backward = Vector3.Transform(new Vector3(0, 0, -1), Matrix4x4.CreateRotationY(yawInRadians));
        Position += backward * currentDistance;
    }

    public void MoveSidewalk(float speed, bool isLeft)
    {
        SetCrouchState(false);
        LookAt(OpponentPosition);

        CurrentState = isLeft ? PlayerState.SideWalkingLeft : PlayerState.SideWalkingRight;

        float yawInRadians = Rotation.Y * (MathF.PI / 180.0f);
        float sideFactor = isLeft ? 1.0f : -1.0f;
        Vector3 sideVector = Vector3.Transform(new Vector3(sideFactor, 0, 0), Matrix4x4.CreateRotationY(yawInRadians));

        Position += sideVector * speed;
    }

    private float _sidestepAngleVelocity = 0.0f;

    public void ApplyDash(Vector3 localDirection, float force, PlayerState dashState)
    {
        if (!IsGrounded) return;

        SetCrouchState(false);
        LookAt(OpponentPosition);

        CurrentState = dashState;

        if (dashState == PlayerState.SidestepLeft || dashState == PlayerState.SidestepRight)
        {
            Vector3 toPlayer = Position - OpponentPosition;
            toPlayer.Y = 0;
            float currentRadius = toPlayer.Length();

            if (currentRadius < 0.2f) currentRadius = 0.2f;

            float desiredLinearSpeed = 7.5f;

            float angularSpeed = desiredLinearSpeed / currentRadius;

            _sidestepAngleVelocity = (dashState == PlayerState.SidestepLeft) ? angularSpeed : -angularSpeed;
            _dashVelocity = Vector3.Zero;
        }
        else
        {
            _sidestepAngleVelocity = 0.0f;
            float yawInRadians = Rotation.Y * (MathF.PI / 180.0f);
            Vector3 worldDirection = Vector3.Transform(localDirection, Matrix4x4.CreateRotationY(yawInRadians));
            _dashVelocity = worldDirection * force;
        }
    }

    public void UpdateDash(float deltaTime)
    {
        if (CurrentState == PlayerState.SidestepLeft || CurrentState == PlayerState.SidestepRight)
        {
            Vector3 toPlayer = Position - OpponentPosition;
            toPlayer.Y = 0;
            float currentRadius = toPlayer.Length();

            if (currentRadius > 0.05f)
            {
                float currentAngle = MathF.Atan2(toPlayer.X, toPlayer.Z);

                currentAngle += _sidestepAngleVelocity * deltaTime;

                Position = new Vector3(
                    OpponentPosition.X + currentRadius * MathF.Sin(currentAngle),
                    Position.Y,
                    OpponentPosition.Z + currentRadius * MathF.Cos(currentAngle)
                );

                LookAt(OpponentPosition);
            }

            float deceleration = 14.0f;
            _sidestepAngleVelocity = MathF.Sign(_sidestepAngleVelocity) *
                                      MathF.Max(0f, MathF.Abs(_sidestepAngleVelocity) - deceleration * deltaTime);

            if (MathF.Abs(_sidestepAngleVelocity) <= 0.1f)
            {
                _sidestepAngleVelocity = 0.0f;
                if (IsGrounded && !IsAttacking)
                {
                    CurrentState = PlayerState.Idle;
                }
            }
            return;
        }

        if (IsDashing)
        {
            Position += _dashVelocity * deltaTime;
            _dashVelocity = Vector3.Lerp(_dashVelocity, Vector3.Zero, 8.0f * deltaTime);

            if (_dashVelocity.LengthSquared() <= 0.05f)
            {
                _dashVelocity = Vector3.Zero;
                if (IsGrounded && !IsAttacking)
                {
                    CurrentState = PlayerState.Idle;
                }
            }
        }
    }

    public void ResetDash()
    {
        _dashVelocity = Vector3.Zero;
        _sidestepAngleVelocity = 0.0f;
    }

    public void FrontDash(float force = 12.0f) => ApplyDash(new Vector3(0, 0, 1), force, PlayerState.FrontDashing);
    public void BackDash(float force = 12.0f) => ApplyDash(new Vector3(0, 0, -1), force, PlayerState.BackDashing);
    public void SideDashLeft(float force = 11.0f) => ApplyDash(new Vector3(1, 0, 0), force, PlayerState.SidestepLeft);
    public void SideDashRight(float force = 11.0f) => ApplyDash(new Vector3(-1, 0, 0), force, PlayerState.SidestepRight);

    public void Jump(Vector3 direction, float jumpForce = 8.0f)
    {
        if (IsGrounded && !IsAttacking)
        {
            SetCrouchState(false);
            VelocityY = jumpForce;
            IsGrounded = false;

            if (direction.Z > 0f)
            {
                CurrentState = PlayerState.JumpingForward;
            }
            else if (direction.Z < 0f)
            {
                CurrentState = PlayerState.JumpingBackward;
            }
            else
            {
                CurrentState = PlayerState.Jumping;
            }

            if (direction != Vector3.Zero)
            {
                float yawInRadians = Rotation.Y * (MathF.PI / 180.0f);
                Vector3 worldDirection = Vector3.Transform(direction, Matrix4x4.CreateRotationY(yawInRadians));
                Position += worldDirection * 0.25f;
            }
        }
    }

    public void AttackS1() => TriggerAttack(AttackType.S1);
    public void AttackD1() => TriggerAttack(AttackType.D1);
    public void AttackS2() => TriggerAttack(AttackType.S2);
    public void AttackD2() => TriggerAttack(AttackType.D2);

    public void PunchLeft() => AttackS1();
    public void PunchRightHigh() => AttackD2();

    private void TriggerAttack(AttackType attack)
    {
        ResetDash();
        CurrentState = PlayerState.Attacking;
        CurrentAttackType = attack;
        _attackFrameCounter = 0;
        HasHitCurrentAttack = false;
    }

    public void UpdateAttack()
    {
        if (!IsAttacking) return;

        _attackFrameCounter++;

        if (_attackFrameCounter <= TotalAttackFrames)
        {
            switch (CurrentAttackType)
            {
                case AttackType.S1:
                    FighterModel.AnimateLeftJab(_attackFrameCounter, StartupFrames, ActiveFrames, RecoveryFrames);
                    break;
                case AttackType.D1:
                    FighterModel.AnimateRightJab(_attackFrameCounter, StartupFrames, ActiveFrames, RecoveryFrames);
                    break;
                case AttackType.S2:
                    FighterModel.AnimateLeftHook(_attackFrameCounter, StartupFrames, ActiveFrames, RecoveryFrames);
                    break;
                case AttackType.D2:
                    FighterModel.AnimateRightStraight(_attackFrameCounter, StartupFrames, ActiveFrames, RecoveryFrames);
                    break;
            }
        }
        else
        {
            FighterModel.ResetPose();
            CurrentState = PlayerState.Idle;
            CurrentAttackType = AttackType.None;
            _attackFrameCounter = 0;
            HasHitCurrentAttack = false;
        }
    }

    public bool IsAttackInActiveFrames()
    {
        return IsAttacking &&
               _attackFrameCounter > StartupFrames &&
               _attackFrameCounter <= (StartupFrames + ActiveFrames);
    }

    public (Vector3 position, float radius) GetPunchHitbox()
    {
        float yawInRadians = Rotation.Y * (MathF.PI / 180.0f);
        Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1), Matrix4x4.CreateRotationY(yawInRadians));

        float yPos = 1.20f;
        float radius = 0.17f;

        switch (CurrentAttackType)
        {
            case AttackType.S1:
                yPos = 1.20f; radius = 0.17f; break;
            case AttackType.D1:
                yPos = 1.20f; radius = 0.17f; break;
            case AttackType.S2:
                yPos = 1.35f; radius = 0.20f; break;
            case AttackType.D2:
                yPos = 1.55f; radius = 0.23f; break;
        }

        Vector3 punchPosition = Position + (forward * 0.8f) + new Vector3(0, yPos, 0);
        return (punchPosition, radius);
    }

    public void UpdateAnimations(float deltaTime)
    {
        if (IsAttacking)
        {
            UpdateAttack();
            return;
        }

        if (!IsGrounded || CurrentState == PlayerState.Jumping || CurrentState == PlayerState.JumpingForward || CurrentState == PlayerState.JumpingBackward || CurrentState == PlayerState.Falling || CurrentState == PlayerState.Rising)
        {
            FighterModel.AnimateJump();
            return;
        }

        if (CurrentState == PlayerState.Crouching)
        {
            FighterModel.AnimateCrouch();
            return;
        }

        if (CurrentState == PlayerState.FrontDashing)
        {
            FighterModel.AnimateFrontDash();
            return;
        }

        if (CurrentState == PlayerState.BackDashing)
        {
            FighterModel.AnimateBackDash();
            return;
        }

        if (CurrentState == PlayerState.SidestepLeft || CurrentState == PlayerState.SideWalkingLeft)
        {
            FighterModel.AnimateSidestep(isLeft: true);
            return;
        }

        if (CurrentState == PlayerState.SidestepRight || CurrentState == PlayerState.SideWalkingRight)
        {
            FighterModel.AnimateSidestep(isLeft: false);
            return;
        }

        if (CurrentState == PlayerState.Defending)
        {
            FighterModel.AnimateDefending();
            return;
        }

        if (CurrentState == PlayerState.Hit)
        {
            FighterModel.AnimateHit();
            return;
        }

        if (CurrentState == PlayerState.Downed || CurrentState == PlayerState.KnockedOut)
        {
            FighterModel.AnimateKnockedOut();
            return;
        }

        if (CurrentState == PlayerState.Walking || CurrentState == PlayerState.Running)
        {
            _walkTimeTimer += deltaTime;
            float animSpeed = CurrentState == PlayerState.Running ? 16.0f : 10.0f;

            FighterModel.AnimateWalk(_walkTimeTimer, animSpeed);
        }
        else
        {
            _walkTimeTimer = 0.0f;
            _idleTimeTimer += deltaTime;
            FighterModel.AnimateIdle(_idleTimeTimer);
        }
    }

    public void Dispose()
    {
        FighterModel.Dispose();
    }
}