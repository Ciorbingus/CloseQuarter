using System.Numerics;
using Silk.NET.Input;

using CloseQuarter.Client.Models;

namespace CloseQuarter.Client.Managers;

public enum InputFlags : ushort
{
    None        = 0,
    Forward     = 1 << 0,
    Backward    = 1 << 1,
    Up          = 1 << 2,
    Down        = 1 << 3,
    Punch       = 1 << 4,

    DownForward = Down | Forward,
    DownBack    = Down | Backward,
    UpForward   = Up | Forward,
    UpBack      = Up | Backward
}

public struct FrameInput
{
    public InputFlags Flags;
    public float Timestamp;

    public FrameInput(InputFlags flags, float timestamp)
    {
        Flags = flags;
        Timestamp = timestamp;
    }

    public bool HasFlag(InputFlags flag) => (Flags & flag) == flag;
}

public class InputHistory
{
    private readonly List<FrameInput> _history = new();
    private const int MaxHistoryLength = 60;

    public void AddFrame(InputFlags flags, float currentTime)
    {
        _history.Add(new FrameInput(flags, currentTime));

        if (_history.Count > MaxHistoryLength)
        {
            _history.RemoveAt(0);
        }
    }

    public bool CheckSequence(InputFlags[] sequence, float maxWindow = 0.4f, float currentTime = 0f)
    {
        if (_history.Count == 0 || sequence.Length == 0) return false;

        int seqIdx = sequence.Length - 1;
        float latestTime = _history[_history.Count - 1].Timestamp;

        if (currentTime - latestTime > maxWindow) return false;

        for (int i = _history.Count - 1; i >= 0; i--)
        {
            var frame = _history[i];

            if (latestTime - frame.Timestamp > maxWindow)
                break;

            if (frame.HasFlag(sequence[seqIdx]))
            {
                seqIdx--;
                if (seqIdx < 0) return true;
            }
        }

        return false;
    }

    public void Clear() => _history.Clear();
}

public class InputManager
{
    private const float HoldThreshold = 0.15f;
    private float _crouchPressTime = 0f;

    private IKeyboard? _keyboard;

    public InputHistory History { get; } = new();

   
    public void Initialize(IInputContext inputContext)
    {
        if (inputContext.Keyboards.Count > 0)
        {
            _keyboard = inputContext.Keyboards[0];
        }
    }


    public void ProcessInput(Player player, float currentTime)
    {
        if (_keyboard == null) return;

        InputFlags currentFlags = ReadInputFlags(player);

        bool punchPressed = _keyboard.IsKeyPressed(player.PunchKey);

        History.AddFrame(currentFlags, currentTime);

        if (punchPressed)
        {
            player.Punch();
            return;
        }

        if (_keyboard.IsKeyPressed(player.LeftKey)) 
        {
            if (currentFlags.HasFlag(InputFlags.Forward))
            {
                player.Jump(new Vector3(0, 0, 1.0f));
            }
            else if (currentFlags.HasFlag(InputFlags.Backward))
            {
                player.Jump(new Vector3(0, 0, -1.0f));
            }
            else
            {
                player.Jump(Vector3.Zero);
            }
            return;
        }

        if (_keyboard.IsKeyPressed(player.RightKey))
        {
            _crouchPressTime = currentTime;
        }

        if (currentFlags.HasFlag(InputFlags.Down))
        {
            if (currentTime - _crouchPressTime >= HoldThreshold)
            {
                player.SetCrouchState(true);
            }
            return;
        }
        else
        {
            if (player.CurrentState == PlayerState.Crouching)
            {
                player.SetCrouchState(false);
            }
        }

        if (currentFlags.HasFlag(InputFlags.Forward))
        {
            player.MoveForward(0.05f);
        }
        else if (currentFlags.HasFlag(InputFlags.Backward))
        {
            player.MoveBackward(0.05f);
        }
        else
        {
            if (player.IsGrounded && 
                !player.IsAttacking && 
                player.CurrentState != PlayerState.Crouching &&
                player.CurrentState != PlayerState.Jumping &&
                player.CurrentState != PlayerState.JumpingForward &&
                player.CurrentState != PlayerState.JumpingBackward)
            {
                player.CurrentState = PlayerState.Idle;
            }
        }
    }


    private InputFlags ReadInputFlags(Player player)
    {
        if (_keyboard == null) return InputFlags.None;

        InputFlags flags = InputFlags.None;

        if (_keyboard.IsKeyPressed(player.ForwardKey))  flags |= InputFlags.Forward;
        if (_keyboard.IsKeyPressed(player.BackwardKey)) flags |= InputFlags.Backward;
        if (_keyboard.IsKeyPressed(player.LeftKey))     flags |= InputFlags.Up;
        if (_keyboard.IsKeyPressed(player.RightKey))    flags |= InputFlags.Down;
        if (_keyboard.IsKeyPressed(player.PunchKey))    flags |= InputFlags.Punch;

        return flags;
    }


    public void ProcessCameraInput(Camera camera, float deltaTime, float rotationSpeed = 0.1f, float movementSpeed = 5.0f)
    {
        if (_keyboard == null) return;

        if (_keyboard.IsKeyPressed(Key.A)) { camera.MoveLeft(movementSpeed * deltaTime); return; }
        if (_keyboard.IsKeyPressed(Key.D)) { camera.MoveRight(movementSpeed * deltaTime); return; }
        if (_keyboard.IsKeyPressed(Key.W)) { camera.MoveForward(movementSpeed * deltaTime); return; }
        if (_keyboard.IsKeyPressed(Key.S)) { camera.MoveBackward(movementSpeed * deltaTime); return; }

        if (_keyboard.IsKeyPressed(Key.Q)) { camera.RotateAroundTarget(rotationSpeed * deltaTime); return; }
        if (_keyboard.IsKeyPressed(Key.E)) { camera.RotateAroundTarget(-rotationSpeed * deltaTime); return; }
        if (_keyboard.IsKeyPressed(Key.R)) { camera.MoveUp(movementSpeed * deltaTime); return; }
        if (_keyboard.IsKeyPressed(Key.F)) { camera.MoveDown(movementSpeed * deltaTime); return; }
    }
}