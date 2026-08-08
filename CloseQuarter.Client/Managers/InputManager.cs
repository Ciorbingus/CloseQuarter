using System.Numerics;
using Silk.NET.Input;

using CloseQuarter.Client.Models;
using ImGuiNET;

namespace CloseQuarter.Client.Managers;

[Flags]
public enum InputFlags : ushort
{
    None        = 0,
    Forward     = 1 << 0,
    Backward    = 1 << 1,
    Up          = 1 << 2,
    Down        = 1 << 3,
    Punch       = 1 << 4, 
    RightPunch  = 1 << 5, 

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

    public IEnumerable<FrameInput> GetRecentFrames(int count)
    {
        int takeCount = Math.Min(count, _history.Count);
        for (int i = _history.Count - 1; i >= _history.Count - takeCount; i--)
        {
            yield return _history[i];
        }
    }

    public bool CheckSequence(InputFlags[] sequence, float maxWindow = 0.6f, float currentTime = 0f)
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
    private bool _isDownKeyPressed = false;

    private bool _wasPunchPressedLastFrame = false;
    private bool _wasRightPunchPressedLastFrame = false;

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
        bool isPunchDown = _keyboard != null && _keyboard.IsKeyPressed(player.PunchKey);
        bool isRightPunchDown = _keyboard != null && _keyboard.IsKeyPressed(player.RightPunchKey);

        bool punchTriggered = isPunchDown && !_wasPunchPressedLastFrame;
        bool rightPunchTriggered = isRightPunchDown && !_wasRightPunchPressedLastFrame;

        _wasPunchPressedLastFrame = isPunchDown;
        _wasRightPunchPressedLastFrame = isRightPunchDown;

        InputFlags currentFlags = ReadInputFlags(player, punchTriggered, rightPunchTriggered);

        History.AddFrame(currentFlags, currentTime);

        if (rightPunchTriggered)
        {
            InputFlags[] combo112 = new[]
            {
                InputFlags.Punch,
                InputFlags.Punch,
                InputFlags.RightPunch
            };

            if (History.CheckSequence(combo112, maxWindow: 0.6f, currentTime: currentTime))
            {
                player.PunchRightHigh();
                return;
            }

            if (!player.IsAttacking)
            {
                player.PunchRightHigh();
                return;
            }
        }

        if (punchTriggered && !player.IsAttacking)
        {
            player.PunchLeft();
            return;
        }

        if (player.IsAttacking)
        {
            return;
        }

        bool jumpPressed = _keyboard != null && _keyboard.IsKeyPressed(player.LeftKey);
        if (jumpPressed)
        {
            if (currentFlags.HasFlag(InputFlags.Forward)) player.Jump(new Vector3(0, 0, 1.0f));
            else if (currentFlags.HasFlag(InputFlags.Backward)) player.Jump(new Vector3(0, 0, -1.0f));
            else player.Jump(Vector3.Zero);
            return;
        }

        if (currentFlags.HasFlag(InputFlags.Down))
        {
            if (!_isDownKeyPressed)
            {
                _crouchPressTime = currentTime;
                _isDownKeyPressed = true;
            }

            if (currentTime - _crouchPressTime >= HoldThreshold)
            {
                player.SetCrouchState(true);
            }
            return;
        }
        else
        {
            _isDownKeyPressed = false;

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

    private InputFlags ReadInputFlags(Player player, bool punchTriggered, bool rightPunchTriggered)
    {
        if (_keyboard == null) return InputFlags.None;

        InputFlags flags = InputFlags.None;

        if (_keyboard.IsKeyPressed(player.ForwardKey))  flags |= InputFlags.Forward;
        if (_keyboard.IsKeyPressed(player.BackwardKey)) flags |= InputFlags.Backward;
        if (_keyboard.IsKeyPressed(player.LeftKey))     flags |= InputFlags.Up;
        if (_keyboard.IsKeyPressed(player.RightKey))    flags |= InputFlags.Down;

        if (punchTriggered)      flags |= InputFlags.Punch;
        if (rightPunchTriggered) flags |= InputFlags.RightPunch;

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

    public void DrawInputHistoryUI(string title, Vector2 position)
    {
        ImGui.SetNextWindowPos(position, ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(230, 260), ImGuiCond.Always);

        ImGui.Begin(title, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
        ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.2f, 1.0f), "--- INPUT HISTORY ---");
        ImGui.Separator();

        var recentFrames = History.GetRecentFrames(10);

        foreach (var frame in recentFrames)
        {
            string dirText = GetDirectionText(frame.Flags);
            bool hasPunch = frame.HasFlag(InputFlags.Punch);
            bool hasRightPunch = frame.HasFlag(InputFlags.RightPunch);

            if (hasPunch)
            {
                ImGui.TextColored(new Vector4(0.2f, 1.0f, 0.3f, 1.0f), $"{dirText} + LP (1)");
            }
            else if (hasRightPunch)
            {
                ImGui.TextColored(new Vector4(1.0f, 0.3f, 0.3f, 1.0f), $"{dirText} + RP (2)");
            }
            else if (frame.Flags != InputFlags.None)
            {
                ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1.0f), dirText);
            }
            else
            {
                ImGui.TextDisabled("Neutral (5)");
            }
        }

        ImGui.End();
    }

    private string GetDirectionText(InputFlags flags)
    {
        if (flags.HasFlag(InputFlags.UpForward)) return "Up-Forward (9)";
        if (flags.HasFlag(InputFlags.UpBack)) return "Up-Back (7)";
        if (flags.HasFlag(InputFlags.DownForward)) return "Down-Forward (3)";
        if (flags.HasFlag(InputFlags.DownBack)) return "Down-Back (1)";
        if (flags.HasFlag(InputFlags.Up)) return "Up (8)";
        if (flags.HasFlag(InputFlags.Down)) return "Down (2)";
        if (flags.HasFlag(InputFlags.Forward)) return "Forward (6)";
        if (flags.HasFlag(InputFlags.Backward)) return "Back (4)";

        return "Neutral (5)";
    }
}