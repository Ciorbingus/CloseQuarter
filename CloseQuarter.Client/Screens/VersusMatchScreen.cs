using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using System.Drawing;
using ImGuiNET;
using System.Numerics;

using CloseQuarter.Client.Managers;
using CloseQuarter.Client.Graphics;
using CloseQuarter.Client.Models;
using CloseQuarter.Client.Maps;

namespace CloseQuarter.Client.Screens;

public class VersusMatchScreen : Screen
{
    private readonly string _p1CharId;
    private readonly string _p2CharId;
    private GameMap _currentMap;

    private readonly CombatManager _combat = new();

    private bool _bgmSoundOn = true;
    private string _bgmFilePath = "CloseQuarter.Client/Audio/tekken.wav";
    private bool _bgmStarted = false;

    private Vector4 _timerColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

    private Camera? _camera;

    private Player? _player1;
    private Player? _player2;
    private MyShader? _playerShader;

    private readonly Vector3 _p1StartPos = new Vector3(-2.5f, 0.0f, 0f);
    private readonly Vector3 _p2StartPos = new Vector3(2.5f, 0.0f, 0f);
    private readonly Vector3 _p1StartRot = new Vector3(0.0f, 90.0f, 0.0f);
    private readonly Vector3 _p2StartRot = new Vector3(0.0f, -90.0f, 0.0f);

    private float _accumulator = 0.0f;
    private const float FixedDeltaTime = 1.0f / 60.0f;

    private readonly InputManager _p1Input = new();
    private readonly InputManager _p2Input = new();
    private IKeyboard? _keyboard;
    private bool _wasEscapePressedLastFrame = false;
    private float _totalGameTime = 0f;

    public VersusMatchScreen(string p1CharId, string p2CharId, GameMap map)
    {
        _p1CharId = p1CharId;
        _p2CharId = p2CharId;
        _currentMap = map;
    }

    public override void OnLoad(GL gl, IWindow window)
    {
        base.OnLoad(gl, window);

        IInputContext inputContext = Program.InputContext;

        if (inputContext != null && inputContext.Keyboards.Count > 0)
        {
            _keyboard = inputContext.Keyboards[0];
            _p1Input.Initialize(inputContext);
            _p2Input.Initialize(inputContext);
        }

        _currentMap.LoadResources();

        float aspectRatio = (float)Window.Size.X / Window.Size.Y;
        _camera = new Camera(aspectRatio);

        ConfigureFighters();
    }

    private void ConfigureFighters()
    {
        try
        {
            _playerShader = MyShader.FromFiles(Gl, "Shaders/player.vert", "Shaders/player.frag");

            _player1 = Player.CreateFromRoster(Gl, _p1CharId);
            if (_player1 != null)
            {
                _player1.ConfigureControls(Key.D, Key.A, Key.W, Key.S, Key.F, Key.G);
                _player1.ResetToDefaultState(_p1StartPos, _p1StartRot);
            }

            _player2 = Player.CreateFromRoster(Gl, _p2CharId);
            if (_player2 != null)
            {
                _player2.ConfigureControls(Key.Right, Key.Left, Key.Up, Key.Down, Key.K, Key.L);
                _player2.ResetToDefaultState(_p2StartPos, _p2StartRot);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OpenGL Error] Failed to configure fighters: {ex.Message}");
        }
    }

    public override void OnUpdate(double deltaTime)
    {
        float dt = (float)deltaTime;

        bool isEscapePressed = _keyboard != null && _keyboard.IsKeyPressed(Key.Escape);
        if (isEscapePressed && !_wasEscapePressedLastFrame)
        {
            _combat.TogglePause();
        }
        _wasEscapePressedLastFrame = isEscapePressed;

        if (!_bgmStarted && _bgmSoundOn)
        {
            AudioManager.PlayBGM(_bgmFilePath, 0.3f);
            _bgmStarted = true;
        }

        if (_player1 == null || _player2 == null) return;

        _combat.UpdateMatch(dt, _player1, _player2);

        if (_combat.IsGameOver)
        {
            AudioManager.StopBGM();
        }

        if (_combat.CurrentState == MatchState.P1Wins || _combat.CurrentState == MatchState.P2Wins)
        {
            UpdateVictoryCamera(dt);

            if (_combat.IsMatchTransitionReady)
            {
                ScreenManager.ChangeScreen(new VersusSelectionScreen());
                return;
            }
        }
        else if (_combat.CurrentState == MatchState.Running || _combat.CurrentState == MatchState.Starting || _combat.CurrentState == MatchState.RoundOver)
        {
            _totalGameTime += dt;

            _accumulator += dt;
            if (_accumulator > 0.1f) _accumulator = 0.1f;

            while (_accumulator >= FixedDeltaTime)
            {
                if (_combat.CurrentState == MatchState.Running)
                {
                    GetInput();
                }

                if (_player1 != null && _player2 != null)
                {
                    _combat.ProcessFixedUpdate(_player1, _player2, FixedDeltaTime);
                }

                _accumulator -= FixedDeltaTime;
            }

            if (_camera != null && _player1 != null && _player2 != null)
            {
                if (_combat.CurrentState == MatchState.RoundOver)
                {
                    UpdateVictoryCamera(dt);
                }
                else
                {
                    _camera.UpdateDynamic(_player1.Position, _player2.Position, dt);
                }
            }
        }
    }

    private void UpdateVictoryCamera(float dt)
    {
        if (_camera == null) return;

        Player? winner = _player1?.CurrentState == PlayerState.Victory ? _player1 :
                        (_player2?.CurrentState == PlayerState.Victory ? _player2 : null);

        if (winner != null)
        {
            _camera.UpdateVictoryCinematic(winner.Position, dt);
        }
    }

    private void GetInput()
    {
        Vector3 camPos = _camera?.Position ?? Vector3.Zero;

        if (_player1 != null) _p1Input.ProcessInput(_player1, camPos, _totalGameTime);
        if (_player2 != null) _p2Input.ProcessInput(_player2, camPos, _totalGameTime);
    }

    public override void OnRender(double deltaTime)
    {
        Gl.ClearColor(Color.FromArgb(255, 25, 30, 45));
        Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        DrawScene();
        DrawHUD();

        switch (_combat.CurrentState)
        {
            case MatchState.Starting:
                DrawCountdownOverlay();
                break;
            case MatchState.Paused:
                DrawPauseMenuUI();
                break;
            case MatchState.RoundOver:
            case MatchState.P1Wins:
            case MatchState.P2Wins:
                DrawVictoryOverlay();
                break;
        }
    }

    private void DrawScene()
    {
        if (_camera == null || _playerShader == null) return;

        Matrix4x4 view = _camera.GetViewMatrix();
        Matrix4x4 projection = _camera.GetProjectionMatrix();

        _currentMap?.Render(view, projection);

        Gl.Enable(EnableCap.DepthTest);
        _player1?.Render(_playerShader, view, projection);
        _player2?.Render(_playerShader, view, projection);
    }

    private void DrawHUD()
    {
        float windowWidth = Window.Size.X;

        ImGui.SetNextWindowPos(new Vector2(20, 20));
        ImGui.SetNextWindowSize(new Vector2(400, 90));
        ImGui.Begin("P1_HUD", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);
        string p1Name = _player1?.DisplayName.ToUpper() ?? "PLAYER 1";
        ImGui.TextColored(new Vector4(0.2f, 0.8f, 1.0f, 1.0f), p1Name);
        PushHealthBarStyle(_combat.P1Health);
        ImGui.ProgressBar(_combat.P1Health / CombatManager.MaxHealth, new Vector2(350, 22), $"{_combat.P1Health:F0} HP");
        ImGui.PopStyleColor();

        string p1Rounds = "";
        for (int i = 0; i < CombatManager.RoundsToWin; i++)
            p1Rounds += (i < _combat.P1RoundsWon) ? " [X] " : " [ ] ";
        ImGui.TextColored(new Vector4(1f, 0.8f, 0.2f, 1f), $"Rounds: {p1Rounds}");
        ImGui.End();

        ImGui.SetNextWindowPos(new Vector2((windowWidth / 2f) - 40, 15));
        ImGui.SetNextWindowSize(new Vector2(80, 60));
        ImGui.Begin("Timer_HUD", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);
        ImGui.SetWindowFontScale(1.8f);
        UpdateTimerColor(_combat.MatchTimer);
        ImGui.TextColored(_timerColor, $"{_combat.MatchTimer:F0}");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.End();

        ImGui.SetNextWindowPos(new Vector2(windowWidth - 420, 20));
        ImGui.SetNextWindowSize(new Vector2(400, 90));
        ImGui.Begin("P2_HUD", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);
        string p2Name = _player2?.DisplayName.ToUpper() ?? "PLAYER 2";
        float posX = ImGui.GetWindowWidth() - ImGui.CalcTextSize(p2Name).X - 50;
        ImGui.SetCursorPosX(posX);
        ImGui.TextColored(new Vector4(1.0f, 0.3f, 0.3f, 1.0f), p2Name);
        PushHealthBarStyle(_combat.P2Health);
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 350 - 50);
        ImGui.ProgressBar(_combat.P2Health / CombatManager.MaxHealth, new Vector2(350, 22), $"{_combat.P2Health:F0} HP");
        ImGui.PopStyleColor();

        string p2Rounds = "";
        for (int i = 0; i < CombatManager.RoundsToWin; i++)
            p2Rounds += (i < _combat.P2RoundsWon) ? " [X] " : " [ ] ";
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize($"Rounds: {p2Rounds}").X - 50);
        ImGui.TextColored(new Vector4(1f, 0.8f, 0.2f, 1f), $"Rounds: {p2Rounds}");
        ImGui.End();
    }

    private void PushHealthBarStyle(float health)
    {
        if (health < 25f) ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.9f, 0.1f, 0.1f, 1.0f));
        else if (health < 50f) ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.9f, 0.6f, 0.2f, 1.0f));
        else if (health < 75f) ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.9f, 0.8f, 0.1f, 1.0f));
        else ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.1f, 0.8f, 0.2f, 1.0f));
    }

    private void UpdateTimerColor(float timer)
    {
        if (timer < 15f) _timerColor = new Vector4(1.0f, 0.2f, 0.2f, 1.0f);
        else if (timer < 45f) _timerColor = new Vector4(1.0f, 0.6f, 0.2f, 1.0f);
        else if (timer < 75f) _timerColor = new Vector4(1.0f, 0.9f, 0.2f, 1.0f);
        else _timerColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
    }

    private void DrawCountdownOverlay()
    {
        float windowWidth = Window.Size.X;
        float windowHeight = Window.Size.Y;

        ImGui.SetNextWindowPos(new Vector2(0, windowHeight * 0.35f));
        ImGui.SetNextWindowSize(new Vector2(windowWidth, 200));
        ImGui.Begin("Countdown", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);

        ImGui.SetWindowFontScale(4.0f);

        string text;
        Vector4 color;

        float timer = _combat.CountdownTimer;
        if (timer > 3.0f) { text = $"ROUND {_combat.CurrentRound}"; color = new Vector4(1, 0.8f, 0.2f, 1); }
        else if (timer > 2.0f) { text = "READY"; color = new Vector4(1, 0.5f, 0.2f, 1); }
        else if (timer > 1.0f) { text = "SET"; color = new Vector4(0.2f, 1f, 0.2f, 1); }
        else { text = "FIGHT!"; color = new Vector4(0.2f, 0.8f, 1f, 1); }

        float textX = (windowWidth - ImGui.CalcTextSize(text).X) / 2f;
        ImGui.SetCursorPosX(textX);
        ImGui.TextColored(color, text);

        ImGui.End();
    }

    private void DrawVictoryOverlay()
    {
        float windowWidth = Window.Size.X;
        float windowHeight = Window.Size.Y;

        ImGui.SetNextWindowPos(new Vector2(0, windowHeight * 0.40f));
        ImGui.SetNextWindowSize(new Vector2(windowWidth, 180));
        ImGui.Begin("VictoryOverlay", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);

        ImGui.SetWindowFontScale(3.5f);

        string winnerName = _player1?.CurrentState == PlayerState.Victory ? _player1.DisplayName.ToUpper() :
                           (_player2?.CurrentState == PlayerState.Victory ? _player2.DisplayName.ToUpper() : "PLAYER");

        bool isFinalMatch = _combat.CurrentState == MatchState.P1Wins || _combat.CurrentState == MatchState.P2Wins;
        string text = isFinalMatch ? $"{winnerName} WINS THE MATCH!" : $"{winnerName} WINS THE ROUND!";

        Vector4 color = _player1?.CurrentState == PlayerState.Victory ? new Vector4(0.2f, 0.8f, 1.0f, 1.0f) : new Vector4(1.0f, 0.3f, 0.3f, 1.0f);

        float textX = (windowWidth - ImGui.CalcTextSize(text).X) / 2f;
        ImGui.SetCursorPosX(textX);
        ImGui.TextColored(color, text);

        ImGui.End();
    }

    private void DrawPauseMenuUI()
    {
        float windowWidth = Window.Size.X;
        float windowHeight = Window.Size.Y;

        Vector2 size = new Vector2(300, 220);
        Vector2 pos = new Vector2((windowWidth - size.X) / 2f, (windowHeight - size.Y) / 2f);

        ImGui.SetNextWindowPos(pos);
        ImGui.SetNextWindowSize(size);
        ImGui.Begin("PAUSED", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

        ImGui.SetWindowFontScale(1.8f);
        string title = "PAUSED";
        ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize(title).X) / 2f);
        ImGui.TextColored(new Vector4(1f, 0.9f, 0.2f, 1f), title);
        ImGui.SetWindowFontScale(1.0f);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button("Resume", new Vector2(280, 40)))
        {
            _combat.TogglePause();
        }

        if (ImGui.Button("Restart Match", new Vector2(280, 40)))
        {
            ResetGame();
        }

        if (ImGui.Button("Quit to Menu", new Vector2(280, 40)))
        {
            ScreenManager.ChangeScreen(new MainMenuScreen());
        }

        ImGui.End();
    }

    private void ResetGame()
    {
        _totalGameTime = 0f;
        _combat.ResetFullMatch();

        if (_bgmSoundOn)
        {
            AudioManager.PlayBGM(_bgmFilePath, 0.3f);
        }

        _player1?.ResetToDefaultState(_p1StartPos, _p1StartRot);
        _player2?.ResetToDefaultState(_p2StartPos, _p2StartRot);

        _p1Input.Reset();
        _p2Input.Reset();

        _camera?.Position = new Vector3(0.0f, 5.0f, 15.0f);
    }

    public override void OnResize(Silk.NET.Maths.Vector2D<int> newSize)
    {
        base.OnResize(newSize);
        if (newSize.Y > 0 && _camera != null)
        {
            _camera.UpdateAspectRatio((float)newSize.X / newSize.Y);
        }
    }

    public override void OnUnload()
    {
        _player1?.Dispose();
        _player2?.Dispose();
        _playerShader?.Dispose();
        _currentMap?.Dispose();
    }
}