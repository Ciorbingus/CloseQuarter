using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Drawing;
using ImGuiNET;
using System.Numerics;

using CloseQuarter.Client.Managers;


namespace CloseQuarter.Client.Models;

public class TestScreen : Screen
{
    private float _p1Health = 100f;
    private float _p2Health = 85f;
    private float _timer = 90f;

    private String _bgmFilePath = "CloseQuarter.Client/Audio/tekken.wav";
    private bool _bgmStarted = false;

    private Vector4 timerColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);



    public override void OnLoad(GL gl, IWindow window)
    {
        base.OnLoad(gl, window);
        Console.WriteLine("[TestScreen] Test screen has been loaded successfully.");
    }

    public override void OnUpdate(double deltaTime)
    {
       
        if (!_bgmStarted)
        {
            AudioManager.PlayBGM(_bgmFilePath, 0.3f);
            _bgmStarted = true;
        }
       
        _timer -= (float)deltaTime;
        if (_timer < 0f) 
        {
            _timer = 0f;
            AudioManager.StopBGM();
        }
    }


    private void updateUI()
    {
        // Debug Window
        ImGui.Begin("Debug Engine");
        ImGui.Text($"FPS: {1.0 / ImGui.GetIO().DeltaTime:F0}");
        ImGui.Separator();

        ImGui.Text("Health Bar:");
        ImGui.SliderFloat("P1 Health", ref _p1Health, 0f, 100f);
        ImGui.SliderFloat("P2 Health", ref _p2Health, 0f, 100f);


        if (ImGui.Button("Reset Health"))
        {
            _p1Health = 100f;
            _p2Health = 100f;
        }

        if (ImGui.Button("Reset Timer"))
        {
            _timer = 90f;

            AudioManager.PlayBGM(_bgmFilePath, 0.3f);
        }

        ImGui.End();


        float windowWidth = Window.Size.X;

        // Player 1 HUD
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(20, 20));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 70));
        ImGui.Begin("P1_HUD", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);

        ImGui.TextColored(new System.Numerics.Vector4(0.2f, 0.8f, 1.0f, 1.0f), "PLAYER 1");

        if (_p1Health < 25f)
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new System.Numerics.Vector4(0.9f, 0.1f, 0.1f, 1.0f));
        else if (_p1Health < 50f)
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new System.Numerics.Vector4(0.9f, 0.8f, 0.1f, 1.0f));
        else
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new System.Numerics.Vector4(0.1f, 0.8f, 0.2f, 1.0f));

        ImGui.ProgressBar(_p1Health / 100f, new System.Numerics.Vector2(350, 22), $"{_p1Health:F0} HP");
        ImGui.PopStyleColor();

        ImGui.End();



        // Timer
        ImGui.SetNextWindowPos(new System.Numerics.Vector2((windowWidth / 2f) - 40, 15));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(80, 60));
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
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(windowWidth - 420, 20));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 70));
        ImGui.Begin("P2_HUD", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);

        string p2Label = "PLAYER 2";
        float posX = ImGui.GetWindowWidth() - ImGui.CalcTextSize(p2Label).X - 50;
        ImGui.SetCursorPosX(posX);
        ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.3f, 0.3f, 1.0f), p2Label);

        if (_p2Health < 25f)
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new System.Numerics.Vector4(0.9f, 0.1f, 0.1f, 1.0f));
        else if (_p2Health < 50f)
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new System.Numerics.Vector4(0.9f, 0.8f, 0.1f, 1.0f));
        else
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new System.Numerics.Vector4(0.1f, 0.8f, 0.2f, 1.0f));

        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 350 - 50);
        ImGui.ProgressBar(_p2Health / 100f, new System.Numerics.Vector2(350, 22), $"{_p2Health:F0} HP");
        ImGui.PopStyleColor();

        ImGui.End();
    }

    public override void OnRender(double deltaTime)
    {
        Gl.ClearColor(Color.FromArgb(255, 25, 30, 45));
        Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        updateUI();
        
    }

    public override void OnUnload()
    {
        Console.WriteLine("[TestScreen] Resources have been released.");
    }
}