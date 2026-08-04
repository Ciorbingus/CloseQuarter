using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Drawing;
using ImGuiNET;
using System.Numerics;

using CloseQuarter.Client.Managers;

namespace CloseQuarter.Client.Screens;

public class MainMenuScreen : Screen
{
    private string _bgmFilePath = "CloseQuarter.Client/Audio/tekken.wav"; 
    private bool _bgmStarted = false;

    public override void OnLoad(GL gl, IWindow window)
    {
        base.OnLoad(gl, window);
        Console.WriteLine("[Main Menu] Main menu loaded.");
    }

    public override void OnUpdate(double deltaTime)
    {
        if (!_bgmStarted)
        {
            AudioManager.PlayBGM(_bgmFilePath, 0.2f);
            _bgmStarted = true;
        }
    }

    public override void OnRender(double deltaTime)
    {
        Gl.ClearColor(Color.FromArgb(255, 15, 18, 26));
        Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        DrawMenuUI();
    }

    private void DrawMenuUI()
    {
        float windowWidth = Window.Size.X;
        float windowHeight = Window.Size.Y;

        Vector2 menuSize = new Vector2(650, 600);
        Vector2 menuPos = new Vector2((windowWidth - menuSize.X) / 2f, (windowHeight - menuSize.Y) / 2f);

        ImGui.SetNextWindowPos(menuPos);
        ImGui.SetNextWindowSize(menuSize);

        ImGui.Begin("Main Menu", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

        ImGui.SetWindowFontScale(1.6f);
        string title = "CLOSE QUARTER";
        float titleX = (ImGui.GetWindowWidth() - ImGui.CalcTextSize(title).X) / 2f;
        ImGui.SetCursorPosX(titleX);
        ImGui.TextColored(new Vector4(0.2f, 0.8f, 1.0f, 1.0f), title);
        ImGui.SetWindowFontScale(1.0f);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.Spacing();

        Vector2 buttonSize = new Vector2(ImGui.GetWindowWidth() - 40, 40);
        ImGui.SetCursorPosX(20);

        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.5f, 0.8f, 0.7f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.6f, 0.9f, 1.0f));
        if (ImGui.Button("Debug Mode", buttonSize))
        {
            ScreenManager.ChangeScreen(new TestScreen());
        }
        ImGui.PopStyleColor(2);

        ImGui.Spacing();
        ImGui.SetCursorPosX(20);

        ImGui.BeginDisabled();
        ImGui.Button("Play", buttonSize);
        ImGui.EndDisabled();


        ImGui.Spacing();
        ImGui.SetCursorPosX(20);

        if (ImGui.Button("Model Editor", buttonSize))
        {
            ScreenManager.ChangeScreen(new ModelEditorScreen());
        }

        ImGui.Spacing();
        ImGui.SetCursorPosX(20);

        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.7f, 0.2f, 0.2f, 0.7f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.3f, 0.3f, 1.0f));
        if (ImGui.Button("Exit Game", buttonSize))
        {
            Window.Close();
        }
        ImGui.PopStyleColor(2);

        ImGui.End();
    }

    public override void OnUnload()
    {
        Console.WriteLine("[MainMenu] Unloaded.");
    }
}