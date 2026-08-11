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

public class TestScreen : Screen
{
    private readonly CombatManager _combat = new();

    private bool _bgmSoundOn = true;
    private string _bgmFilePath = "CloseQuarter.Client/Audio/tekken.wav";
    private string _soundEffect = "CloseQuarter.Client/Audio/boom.wav";
    private bool _bgmStarted = false;

    private Vector4 _timerColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

    private Camera? _camera;
    private float _cameraSpeed = 5.0f;
    private float _cameraRotationSpeed = 30.0f;
    public bool toggleCamera = false;

    private Player? _player1;
    private Player? _player2;
    private MyShader? _playerShader;

    private readonly Vector3 _player1Position = new Vector3(-2.5f, 0.0f, 0f);
    private readonly Vector3 _player2Position = new Vector3(2.5f, 0.0f, 0f);
    private readonly Vector3 _player1Rotation = new Vector3(0.0f, 90.0f, 0.0f);
    private readonly Vector3 _player2Rotation = new Vector3(0.0f, -90.0f, 0.0f);

    private GameMap? _currentMap;

    private bool _showHitboxes = true;
    private DebugRenderer? _debugRenderer;

    private float _accumulator = 0.0f;
    private const float FixedDeltaTime = 1.0f / 60.0f;

    private readonly InputManager _p1Input = new();
    private readonly InputManager _p2Input = new();
    private readonly InputManager _cameraInput = new();
    private IKeyboard? _keyboard;
    private bool _wasEscapePressedLastFrame = false;
    private float _totalGameTime = 0f;

    public override void OnLoad(GL gl, IWindow window)
    {
        base.OnLoad(gl, window);

        IInputContext inputContext = Program.InputContext;

        if (inputContext != null && inputContext.Keyboards.Count > 0)
        {
            _keyboard = inputContext.Keyboards[0];
            _p1Input.Initialize(inputContext);
            _p2Input.Initialize(inputContext);
            _cameraInput.Initialize(inputContext);
        }

        if (inputContext == null || inputContext.Keyboards.Count == 0)
        {
            Console.WriteLine("[Input Error] No keyboard found. Input will not work.");
            return;
        }

        _currentMap = new TestMap(Gl);
        _currentMap.LoadResources();

        float aspectRatio = (float)Window.Size.X / Window.Size.Y;
        _camera = new Camera(aspectRatio);

        ConfigureFighters();

        try
        {
            _debugRenderer = new DebugRenderer(Gl, segments: 32);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OpenGL Error] Failed to initialize DebugRenderer: {ex.Message}");
        }
    }

    private void ConfigureFighters()
    {
        try
        {
            _playerShader = MyShader.FromFiles(Gl, "Shaders/player.vert", "Shaders/player.frag");

            _player1 = Player.CreateFromRoster(Gl, "bobi");
            if (_player1 != null)
            {
                _player1.ConfigureControls(Key.D, Key.A, Key.W, Key.S, Key.Z, Key.X);
                _player1.ResetToDefaultState(_player1Position, _player1Rotation);
            }

            _player2 = Player.CreateFromRoster(Gl, "toma");
            if (_player2 != null)
            {
                _player2.ConfigureControls(Key.Right, Key.Left, Key.Up, Key.Down, Key.K, Key.L);
                _player2.ResetToDefaultState(_player2Position, _player2Rotation);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OpenGL Error] Failed to load players/shaders: {ex.Message}");
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

        if (_combat.CurrentState == MatchState.Running || _combat.CurrentState == MatchState.Starting || _combat.CurrentState == MatchState.RoundOver)
        {
            _totalGameTime += dt;

            _accumulator += dt;
            if (_accumulator > 0.1f) _accumulator = 0.1f;

            while (_accumulator >= FixedDeltaTime)
            {
                if (_combat.CurrentState == MatchState.Running)
                {
                    GetInput(FixedDeltaTime);
                }

                if (_player1 != null && _player2 != null)
                {
                    _combat.ProcessFixedUpdate(_player1, _player2, FixedDeltaTime);
                }

                _accumulator -= FixedDeltaTime;
            }

            if (toggleCamera && _camera != null && _player1 != null && _player2 != null)
            {
                _camera.UpdateDynamic(_player1.Position, _player2.Position, dt);
            }
        }
    }

    private void GetInput(double deltaTime)
    {
        Vector3 camPos = _camera?.Position ?? Vector3.Zero;

        if (_player1 != null) _p1Input.ProcessInput(_player1, camPos, _totalGameTime);
        if (_player2 != null) _p2Input.ProcessInput(_player2, camPos, _totalGameTime);

        if (_camera != null && !toggleCamera)
        {
            _cameraInput.ProcessCameraInput(_camera, (float)deltaTime, _cameraRotationSpeed, _cameraSpeed);
        }
    }

    public override void OnRender(double deltaTime)
    {
        Gl.ClearColor(Color.FromArgb(255, 25, 30, 45));
        Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        DrawScene();
        UpdateUI();

        switch (_combat.CurrentState)
        {
            case MatchState.Starting:
                DrawCountdownOverlay();
                break;
            case MatchState.Paused:
                DrawPauseMenuUI();
                break;
            case MatchState.P1Wins:
            case MatchState.P2Wins:
                DrawWinnerUI();
                break;
        }
    }

    private void DrawScene()
    {
        if (_camera == null) return;

        Matrix4x4 view = _camera.GetViewMatrix();
        Matrix4x4 projection = _camera.GetProjectionMatrix();

        _currentMap?.Render(view, projection);

        if (_playerShader != null)
        {
            Gl.Enable(EnableCap.DepthTest);
            _player1?.Render(_playerShader, view, projection);
            _player2?.Render(_playerShader, view, projection);
        }

        if (_showHitboxes && _debugRenderer != null && _playerShader != null)
        {
            DrawDebugHitboxes(view, projection);
        }
    }

    private void DrawDebugHitboxes(Matrix4x4 view, Matrix4x4 projection)
    {
        if (_playerShader == null || _debugRenderer == null) return;

        if (_player1 != null)
        {
            _debugRenderer.DrawPlayerHitbox(_playerShader, _player1, view, projection);
            if (_player1.IsAttackInActiveFrames())
            {
                var (punchPos, punchRadius) = _player1.GetPunchHitbox();
                _debugRenderer.DrawPunchHitbox(_playerShader, punchPos, punchRadius, view, projection);
            }
        }

        if (_player2 != null)
        {
            _debugRenderer.DrawPlayerHitbox(_playerShader, _player2, view, projection);
            if (_player2.IsAttackInActiveFrames())
            {
                var (punchPos, punchRadius) = _player2.GetPunchHitbox();
                _debugRenderer.DrawPunchHitbox(_playerShader, punchPos, punchRadius, view, projection);
            }
        }
    }

    private void UpdateUI()
    {
        ImGui.Begin("Debug Engine");
        ImGui.Text($"FPS: {1.0 / ImGui.GetIO().DeltaTime:F0}");
        ImGui.Text($"Match State: {_combat.CurrentState}");
        ImGui.Text($"Current Round: {_combat.CurrentRound}");
        ImGui.Separator();

        ImGui.Text($"Camera Position: X: {_camera?.Position.X:F2}, Y: {_camera?.Position.Y:F2}, Z: {_camera?.Position.Z:F2}");

        float p1Hp = _combat.P1Health;
        float p2Hp = _combat.P2Health;
        if (ImGui.SliderFloat("P1 Health", ref p1Hp, 0f, CombatManager.MaxHealth)) _combat.P1Health = p1Hp;
        if (ImGui.SliderFloat("P2 Health", ref p2Hp, 0f, CombatManager.MaxHealth)) _combat.P2Health = p2Hp;

        if (ImGui.Button("Reset Health")) _combat.ResetRound();
        if (ImGui.Button("Reset Timer")) _combat.MatchTimer = 90f;
        if (ImGui.Button("Play SFX")) AudioManager.PlaySFX(_soundEffect, 0.3f);

        if (ImGui.Button("Toggle Camera Mode")) toggleCamera = !toggleCamera;
        ImGui.Text($"Camera Mode: {(toggleCamera ? "Automatic" : "Manual")}");

        if (ImGui.Button("Reset Full Game")) ResetGame();

        if (ImGui.Button("Toggle BGM"))
        {
            _bgmSoundOn = !_bgmSoundOn;
            if (_bgmSoundOn) AudioManager.PlayBGM(_bgmFilePath, 0.3f);
            else AudioManager.StopBGM();
        }

        ImGui.SliderFloat("Camera Speed", ref _cameraSpeed, 0f, 100f);
        ImGui.SliderFloat("Camera Rotation Speed", ref _cameraRotationSpeed, 0f, 15000f);
        if (ImGui.Button("Reset Camera") && _camera != null)
        {
            _camera = new Camera((float)Window.Size.X / Window.Size.Y);
        }

        if (ImGui.Button("Toggle Hitboxes")) _showHitboxes = !_showHitboxes;

        ImGui.Text($"Player 1 Position: X: {_player1?.Position.X:F2}, Y: {_player1?.Position.Y:F2}, Z: {_player1?.Position.Z:F2}");
        ImGui.Text($"Player 2 Position: X: {_player2?.Position.X:F2}, Y: {_player2?.Position.Y:F2}, Z: {_player2?.Position.Z:F2}");
        ImGui.Text($"Player 1 State: {_player1?.CurrentState}");
        ImGui.Text($"Player 2 State: {_player2?.CurrentState}");

        ImGui.End();

        DrawHUDOverlays();
    }

    private void DrawHUDOverlays()
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
        string p2Label = _player2?.DisplayName.ToUpper() ?? "PLAYER 2";
        float posX = ImGui.GetWindowWidth() - ImGui.CalcTextSize(p2Label).X - 50;
        ImGui.SetCursorPosX(posX);
        ImGui.TextColored(new Vector4(1.0f, 0.3f, 0.3f, 1.0f), p2Label);
        PushHealthBarStyle(_combat.P2Health);
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 350 - 50);
        ImGui.ProgressBar(_combat.P2Health / CombatManager.MaxHealth, new Vector2(350, 22), $"{_combat.P2Health:F0} HP");
        ImGui.PopStyleColor();

        string p2Rounds = "";
        for (int i = 0; i < CombatManager.RoundsToWin; i++)
            p2Rounds += (i < _combat.P2RoundsWon) ? " [X] " : " [ ] ";
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize($"Rounds: {p2Rounds}").X - 50);
        ImGui.TextColored(new Vector4(1f, 0.8f, 0.2f, 1f), $"Rounds: {p2Rounds}");

        _p1Input.DrawInputHistoryUI("P1 Inputs", new Vector2(20, 120));
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

    private void DrawWinnerUI()
    {
        float windowWidth = Window.Size.X;
        float windowHeight = Window.Size.Y;

        Vector2 gameOverSize = new Vector2(400, 220);
        Vector2 gameOverPos = new Vector2((windowWidth - gameOverSize.X) / 2f, (windowHeight - gameOverSize.Y) / 2f);

        ImGui.SetNextWindowPos(gameOverPos);
        ImGui.SetNextWindowSize(gameOverSize);

        ImGui.Begin("WinnerWindow", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

        ImGui.SetWindowFontScale(2.0f);

        string winnerText = _combat.CurrentState == MatchState.P1Wins 
            ? $"{_player1?.DisplayName.ToUpper() ?? "P1"} WINS MATCH!" 
            : $"{_player2?.DisplayName.ToUpper() ?? "P2"} WINS MATCH!";

        Vector4 winnerColor = _combat.CurrentState == MatchState.P1Wins 
            ? new Vector4(0.2f, 0.8f, 1.0f, 1.0f) 
            : new Vector4(1.0f, 0.3f, 0.3f, 1.0f);

        float titleX = (ImGui.GetWindowWidth() - ImGui.CalcTextSize(winnerText).X) / 2f;
        ImGui.SetCursorPosX(titleX);
        ImGui.TextColored(winnerColor, winnerText);
        ImGui.SetWindowFontScale(1.0f);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button("Restart Game", new Vector2(380, 40)))
        {
            ResetGame();
        }

        if (ImGui.Button("Return to Main Menu", new Vector2(380, 40)))
        {
            ScreenManager.ChangeScreen(new MainMenuScreen());
        }

        ImGui.End();
    }

    private void ResetGame()
    {
        _totalGameTime = 0f;
        _combat.ResetFullMatch();

        if (_bgmSoundOn) AudioManager.PlayBGM(_bgmFilePath, 0.3f);

        _player1?.ResetToDefaultState(_player1Position, _player1Rotation);
        _player2?.ResetToDefaultState(_player2Position, _player2Rotation);

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
        _debugRenderer?.Dispose();
        _currentMap?.Dispose();
    }
}