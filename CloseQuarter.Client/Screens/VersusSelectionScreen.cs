using Silk.NET.OpenGL;
using System.Numerics;
using ImGuiNET;

using CloseQuarter.Client.Managers;
using CloseQuarter.Client.Models;
using CloseQuarter.Client.Maps;

namespace CloseQuarter.Client.Screens;

public class VersusSelectionScreen : Screen
{
    private string _p1SelectedChar = "bobi";
    private string _p2SelectedChar = "toma";
    private int _selectedMapIndex = 0;

    private readonly string[] _availableMaps = new[] { "Test Arena" };

    public void OnLoad()
    {
        
    }

    public override void OnUpdate(double deltaTime)
    {
        
    }

    public override void OnRender(double deltaTime)
    {
        Gl.ClearColor(0.08f, 0.08f, 0.12f, 1.0f);
        Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        DrawSelectionUI();
    }

    private void DrawSelectionUI()
    {
        float windowWidth = Window.Size.X;
        float windowHeight = Window.Size.Y;

        Vector2 size = new Vector2(800, 500);
        Vector2 pos = new Vector2((windowWidth - size.X) / 2f, (windowHeight - size.Y) / 2f);

        ImGui.SetNextWindowPos(pos);
        ImGui.SetNextWindowSize(size);
        ImGui.Begin("Versus Selection", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove);

        ImGui.SetWindowFontScale(1.5f);
        ImGui.TextColored(new Vector4(0.2f, 0.8f, 1.0f, 1.0f), "VERSUS MODE - CHARACTER & MAP SELECTION");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Columns(2, "SelectionColumns", true);

        ImGui.TextColored(new Vector4(0.2f, 0.8f, 1.0f, 1.0f), "PLAYER 1 CHARACTER");
        foreach (var charId in CharacterRoster.Characters.Keys)
        {
            var charDef = CharacterRoster.Characters[charId];
            if (ImGui.RadioButton($"{charDef.DisplayName}##P1", _p1SelectedChar == charId))
            {
                _p1SelectedChar = charId;
            }
        }

        ImGui.NextColumn();

        ImGui.TextColored(new Vector4(1.0f, 0.3f, 0.3f, 1.0f), "PLAYER 2 CHARACTER");
        foreach (var charId in CharacterRoster.Characters.Keys)
        {
            var charDef = CharacterRoster.Characters[charId];
            if (ImGui.RadioButton($"{charDef.DisplayName}##P2", _p2SelectedChar == charId))
            {
                _p2SelectedChar = charId;
            }
        }

        ImGui.Columns(1);
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Text("SELECT MAP:");
        ImGui.Combo("##MapSelect", ref _selectedMapIndex, _availableMaps, _availableMaps.Length);

        ImGui.Spacing();
        ImGui.Spacing();

        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.1f, 0.7f, 0.2f, 0.8f));
        if (ImGui.Button("FIGHT!", new Vector2(ImGui.GetContentRegionAvail().X, 50)))
        {
            GameMap selectedMap = _selectedMapIndex switch
            {
                0 => new TestMap(Gl),
                _ => new TestMap(Gl)
            };

            ScreenManager.ChangeScreen(new VersusMatchScreen(_p1SelectedChar, _p2SelectedChar, selectedMap));
        }
        ImGui.PopStyleColor();

        ImGui.End();
    }

    public override void OnUnload()
    {
        
    }
}