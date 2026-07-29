using Silk.NET.OpenGL;
using Silk.NET.Windowing;
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
    private const float MaxHealth = 100f;

    private float _p1Health = MaxHealth;
    private float _p2Health = MaxHealth;
    private float _timer = 90f;

    private bool _bgmSoundOn = true;
    private bool _isGameOver = false;


    private string _backgroundTexturePath = "Textures/night.jpeg";

    private string _bgmFilePath = "CloseQuarter.Client/Audio/tekken.wav";
    private string _soundEffect = "CloseQuarter.Client/Audio/boom.wav";
    private bool _bgmStarted = false;

    private Vector4 timerColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);



    private MyShader? _ringShader;
    private Ring? _ring;

    private MyTexture? _ringTexture;
    private string _ringTexturePath = "Textures/sky.jpg";


    private Camera? _camera;



    private float _cameraSpeed = 5.0f;
    private float _cameraRotationSpeed = 30.0f;


    private Player? _player1;
    private Player? _player2;
    private MyShader? _playerShader;

    private String _p1TexturePath = "Textures/player.png";
    private String _p2TexturePath = "Textures/player.png";

    private String _faceTexturePath1 = "Textures/player_face.png";
    private String _faceTexturePath2 = "Textures/player2_face.png";


    private Vector3 _player1Position = new Vector3(-4.5f, 0.0f, 2.5f);
    private Vector3 _player2Position = new Vector3(4.5f, 0.0f, 2.5f);

    private Vector3 _player1Rotation = new Vector3(0.0f, MathF.PI / 2.0f, 0.0f);
    private Vector3 _player2Rotation = new Vector3(0.0f, -MathF.PI / 2.0f, 0.0f);


    private GameMap? _currentMap;

    private Background? _background;


    private bool _showHitboxes = true;
    private DebugRenderer? _debugRenderer;


    public override void OnLoad(GL gl, IWindow window)
    {
        base.OnLoad(gl, window);
        Console.WriteLine("[Debug] Test screen has been loaded successfully.");


        _currentMap = new TestMap(Gl);
        _currentMap.LoadResources();

        float aspectRatio = (float)Window.Size.X / Window.Size.Y;
        _camera = new Camera(aspectRatio);

        try
        {
            _player1 = new Player(Gl);
            _player1.LoadTextures(Gl, _p1TexturePath, _faceTexturePath1);
            _playerShader = MyShader.FromFiles(Gl, "Shaders/player.vert", "Shaders/player.frag");

            if (_player1 != null && _playerShader != null && _camera != null)
            {
                Matrix4x4 view = _camera.GetViewMatrix();
                Matrix4x4 projection = _camera.GetProjectionMatrix();

                _player1.Position = _player1Position;
                _player1.Rotation = _player1Rotation;

                Console.WriteLine("[Debug] Player resources loaded successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OpenGL Error] Failed to load player1 resources: {ex.Message}");
        }

        try
        {
            _player2 = new Player(Gl);
            _player2.LoadTextures(Gl, _p2TexturePath, _faceTexturePath2);

            if (_player2 != null && _playerShader != null && _camera != null)
            {
                Matrix4x4 view = _camera.GetViewMatrix();
                Matrix4x4 projection = _camera.GetProjectionMatrix();

                _player2.Position = _player2Position;
                _player2.Rotation = _player2Rotation;

                Console.WriteLine("[Debug] Player 2 resources loaded successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OpenGL Error] Failed to load player2 resources: {ex.Message}");
        }

        try
        {
            _debugRenderer = new DebugRenderer(Gl, segments: 32);
            Console.WriteLine("[Debug] DebugRenderer initialized successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OpenGL Error] Failed to initialize DebugRenderer: {ex.Message}");
        }

    }

    private void GameOver()
    {
        if (_isGameOver || _timer <= 0f)
        {
            if (_timer <= 0f) _timer = 0f;
            DrawGameOverUI();
            AudioManager.StopBGM();
        }
    }

    private void DrawGameOverUI()
    {
        float windowWidth = Window.Size.X;
        float windowHeight = Window.Size.Y;

        Vector2 gameOverSize = new Vector2(400, 200);
        Vector2 gameOverPos = new Vector2((windowWidth - gameOverSize.X) / 2f, (windowHeight - gameOverSize.Y) / 2f);

        ImGui.SetNextWindowPos(gameOverPos);
        ImGui.SetNextWindowSize(gameOverSize);

        ImGui.Begin("Game Over", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

        ImGui.SetWindowFontScale(1.6f);
        string title = "GAME OVER";
        float titleX = (ImGui.GetWindowWidth() - ImGui.CalcTextSize(title).X) / 2f;
        ImGui.SetCursorPosX(titleX);
        ImGui.TextColored(new Vector4(1.0f, 0.1f, 0.1f, 1.0f), title);
        ImGui.SetWindowFontScale(1.0f);

        ImGui.Spacing();

        if (ImGui.Button("Return to Main Menu"))
        {
            ScreenManager.ChangeScreen(new MainMenuScreen());
        }

        if (ImGui.Button("Restart Game"))
        {
            ResetGame();
        }

        ImGui.End();
    }


    private void GetInput(double deltaTime)
    {

        if (ImGui.IsKeyPressed(ImGuiKey.B))
        {
            if (_p2Health > 0f)
            {
                _p2Health -= 10f;
                AudioManager.PlaySFX(_soundEffect, 0.3f);
            }
        }


        float cameraSpeed = _cameraSpeed * (float)deltaTime;

        if (ImGui.IsKeyDown(ImGuiKey.A))
        {
            _camera?.MoveRight(-cameraSpeed);
        }

        if (ImGui.IsKeyDown(ImGuiKey.D))
        {
            _camera?.MoveRight(cameraSpeed);
        }

        if (ImGui.IsKeyDown(ImGuiKey.W))
        {
            _camera?.MoveForward(cameraSpeed);
        }

        if (ImGui.IsKeyDown(ImGuiKey.S))
        {
            _camera?.MoveBackward(cameraSpeed);
        }


        if (ImGui.IsKeyDown(ImGuiKey.R))
        {
            _camera?.MoveUp(cameraSpeed);
        }

        if (ImGui.IsKeyDown(ImGuiKey.F))
        {
            _camera?.MoveDown(cameraSpeed);
        }

        if (ImGui.IsKeyDown(ImGuiKey.Q))
        {
            _camera?.RotateAroundTarget(-_cameraRotationSpeed * (float)deltaTime);
        }

        if (ImGui.IsKeyDown(ImGuiKey.E))
        {
            _camera?.RotateAroundTarget(_cameraRotationSpeed * (float)deltaTime);
        }


        float playerSpeed = 1.0f * (float)deltaTime;
        float rotationSpeed = 1.5f * (float)deltaTime;


        if (ImGui.IsKeyDown(ImGuiKey.LeftShift))
        {
            playerSpeed *= 3.0f;
        }

        if (ImGui.IsKeyDown(ImGuiKey.LeftArrow))
        {
            _player1?.MoveBackward(playerSpeed);
        }

        if (ImGui.IsKeyDown(ImGuiKey.RightArrow))
        {
            _player1?.MoveForward(playerSpeed);
        }

        if (ImGui.IsKeyDown(ImGuiKey.U))
        {
            _player1?.Rotate(rotationSpeed, 0f);
        }
        if (ImGui.IsKeyDown(ImGuiKey.O))
        {
            _player1?.Rotate(-rotationSpeed, 0f);
        }

        if (ImGui.IsKeyDown(ImGuiKey.UpArrow))
        {
            _player1?.Jump();
        }



    }

    public override void OnUpdate(double deltaTime)
    {
        _isGameOver = (_p1Health <= 0f || _p2Health <= 0f || _timer <= 0f);

        if (!_bgmStarted)
        {
            if (_bgmSoundOn)
            {
                AudioManager.PlayBGM(_bgmFilePath, 0.3f);
            }
            _bgmStarted = true;
        }

        if (!_isGameOver)
        {
            _timer -= (float)deltaTime;
        }

        if (_p1Health < 0f) _p1Health = 0f;
        if (_p2Health < 0f) _p2Health = 0f;
        if (_timer < 0f) _timer = 0f;

        GameOver();


        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            ScreenManager.ChangeScreen(new MainMenuScreen());
        }

        if (!_isGameOver)
        {
            GetInput(deltaTime);

            float dt = (float)deltaTime;

            if (_player1 != null)
            {
                PhysicsManager.ApplyGravityAndFloor(_player1, dt);
                PhysicsManager.KeepPlayerInRing(_player1);
            }

            if (_player2 != null)
            {
                PhysicsManager.ApplyGravityAndFloor(_player2, dt);
                PhysicsManager.KeepPlayerInRing(_player2);
            }

            if (_player1 != null && _player2 != null)
            {
                PhysicsManager.ResolvePlayerCollision(_player1, _player2);
            }

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

            if (_player1 != null)
            {
                _player1.Render(_playerShader, view, projection);
            }

            if (_player2 != null)
            {
                _player2.Render(_playerShader, view, projection);
            }
        }

        if (_showHitboxes && _debugRenderer != null && _playerShader != null)
        {
            float playerRadius = 0.45f;

            if (_player1 != null)
            {
                _debugRenderer.DrawPlayerHitbox(_playerShader, _player1.GetBodyMatrix(), playerRadius, view, projection);
            }

            if (_player2 != null)
            {
                _debugRenderer.DrawPlayerHitbox(_playerShader, _player2.GetBodyMatrix(), playerRadius, view, projection);
            }
        }
    }


    private void ResetGame()
    {
        _p1Health = MaxHealth;
        _p2Health = MaxHealth;
        _timer = 90f;
        _isGameOver = false;

        if (_bgmSoundOn)
        {
            AudioManager.PlayBGM(_bgmFilePath, 0.3f);
        }

        _player1?.Position = _player1Position;
        _player2?.Position = _player2Position;
        _player1?.Rotation = _player1Rotation;
        _player2?.Rotation = _player2Rotation;


        _camera?.Position = new Vector3(0.0f, 5.0f, 15.0f);
    }

    private void UpdateUI()
    {
        // Debug Window
        ImGui.Begin("Debug Engine");
        ImGui.Text($"FPS: {1.0 / ImGui.GetIO().DeltaTime:F0}");
        ImGui.Separator();

        ImGui.Text($"Camera Position: X: {_camera?.Position.X:F2}, Y: {_camera?.Position.Y:F2}, Z: {_camera?.Position.Z:F2}");

        ImGui.SliderFloat("P1 Health", ref _p1Health, 0f, MaxHealth);
        ImGui.SliderFloat("P2 Health", ref _p2Health, 0f, MaxHealth);

        if (ImGui.Button("Reset Health"))
        {
            _p1Health = MaxHealth;
            _p2Health = MaxHealth;
        }

        if (ImGui.Button("Reset Timer"))
        {
            _timer = 90f;
        }

        if (ImGui.Button("Play SFX"))
        {
            AudioManager.PlaySFX(_soundEffect, 0.3f);
        }

        if (ImGui.Button("Reset Game"))
        {
            ResetGame();
        }

        if (ImGui.Button("Toggle BGM"))
        {
            _bgmSoundOn = !_bgmSoundOn;

            if (_bgmSoundOn)
                AudioManager.PlayBGM(_bgmFilePath, 0.3f);
            else
                AudioManager.StopBGM();
        }

        ImGui.SliderFloat("Camera Speed", ref _cameraSpeed, 0f, 100f);
        ImGui.SliderFloat("Camera Rotation Speed", ref _cameraRotationSpeed, 0f, 15000f);
        if (ImGui.Button("Reset Camera"))
        {
            if (_camera != null)
            {
                float aspectRatio = (float)Window.Size.X / Window.Size.Y;
                _camera = new Camera(aspectRatio);
            }
        }

        if (ImGui.Button("Toggle Hitboxes"))
        {
            _showHitboxes = !_showHitboxes;
        }

        ImGui.Text($"Player 1 Position: X: {_player1?.Position.X:F2}, Y: {_player1?.Position.Y:F2}, Z: {_player1?.Position.Z:F2}");
        ImGui.Text($"Player 2 Position: X: {_player2?.Position.X:F2}, Y: {_player2?.Position.Y:F2}, Z: {_player2?.Position.Z:F2}");

        ImGui.End();

        float windowWidth = Window.Size.X;

        // Player 1 HUD
        ImGui.SetNextWindowPos(new Vector2(20, 20));
        ImGui.SetNextWindowSize(new Vector2(400, 70));
        ImGui.Begin("P1_HUD", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);
        ImGui.TextColored(new Vector4(0.2f, 0.8f, 1.0f, 1.0f), "PLAYER 1");

        if (_p1Health < 25f)
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.9f, 0.1f, 0.1f, 1.0f));
        else if (_p1Health < 50f)
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.9f, 0.6f, 0.2f, 1.0f));
        else if (_p1Health < 75f)
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.9f, 0.8f, 0.1f, 1.0f));
        else
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.1f, 0.8f, 0.2f, 1.0f));

        ImGui.ProgressBar(_p1Health / 100f, new Vector2(350, 22), $"{_p1Health:F0} HP");
        ImGui.PopStyleColor();
        ImGui.End();

        // Timer HUD
        ImGui.SetNextWindowPos(new Vector2((windowWidth / 2f) - 40, 15));
        ImGui.SetNextWindowSize(new Vector2(80, 60));
        ImGui.Begin("Timer_HUD", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);
        ImGui.SetWindowFontScale(1.8f);

        if (_timer < 15f)
            timerColor = new Vector4(1.0f, 0.2f, 0.2f, 1.0f);
        else if (_timer < 45f)
            timerColor = new Vector4(1.0f, 0.6f, 0.2f, 1.0f);
        else if (_timer < 75f)
            timerColor = new Vector4(1.0f, 0.9f, 0.2f, 1.0f);
        else
            timerColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        ImGui.TextColored(timerColor, $"{_timer:F0}");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.End();

        // Player 2 HUD
        ImGui.SetNextWindowPos(new Vector2(windowWidth - 420, 20));
        ImGui.SetNextWindowSize(new Vector2(400, 70));
        ImGui.Begin("P2_HUD", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);

        string p2Label = "PLAYER 2";
        float posX = ImGui.GetWindowWidth() - ImGui.CalcTextSize(p2Label).X - 50;
        ImGui.SetCursorPosX(posX);
        ImGui.TextColored(new Vector4(1.0f, 0.3f, 0.3f, 1.0f), p2Label);

        if (_p2Health < 25f)
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.9f, 0.1f, 0.1f, 1.0f));
        else if (_p2Health < 50f)
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.9f, 0.6f, 0.2f, 1.0f));
        else if (_p2Health < 75f)
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.9f, 0.8f, 0.1f, 1.0f));
        else
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.1f, 0.8f, 0.2f, 1.0f));

        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 350 - 50);
        ImGui.ProgressBar(_p2Health / 100f, new Vector2(350, 22), $"{_p2Health:F0} HP");
        ImGui.PopStyleColor();
        ImGui.End();
    }

    public override void OnRender(double deltaTime)
    {
        Gl.ClearColor(Color.FromArgb(255, 25, 30, 45));
        Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        DrawScene();
        UpdateUI();
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
        _background?.Dispose();
        _ring?.Dispose();
        _ringShader?.Dispose();
        _ringTexture?.Dispose();
        _player1?.Dispose();
        _player2?.Dispose();
        _playerShader?.Dispose();
        _debugRenderer?.Dispose();
        _currentMap?.Dispose();


        Console.WriteLine("[Debug] 3D Resources and TestScreen released.");
    }
}