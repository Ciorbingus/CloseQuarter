using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;
using ImGuiNET;

using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models;

public class EditorCube
{
    public string Name { get; set; } = "Cube";
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;
    public bool OnlyFrontTexture { get; set; } = false;
    public Cube CubeMesh { get; set; }

    public EditorCube(GL gl, string name, Vector3 scale)
    {
        Name = name;
        Scale = scale;
        CubeMesh = new Cube(gl, scale.X, scale.Y, scale.Z);
    }
}

public class ModelEditorScreen : Screen
{
    private Camera? _camera;
    private MyShader? _shader;
    private MyTexture? _bodyTexture;
    private MyTexture? _headTexture;

    private List<EditorCube> _cubes = new();
    private int _selectedCubeIndex = 0;

    private Vector3 _modelBasePosition = new Vector3(0, 0, 0);
    private float _modelYaw = 0f;

    private string _newCubeName = "New_Part";


    private bool toggleMovement = true;

    public override void OnLoad(GL gl, IWindow window)
    {
        base.OnLoad(gl, window);

        float aspectRatio = (float)Window.Size.X / Window.Size.Y;
        _camera = new Camera(aspectRatio);

        try
        {
            _shader = MyShader.FromFiles(Gl, "Shaders/player.vert", "Shaders/player.frag");
            _bodyTexture = new MyTexture(Gl, "Textures/player.png");
            _headTexture = new MyTexture(Gl, "Textures/player_face.png");

            AddCube("Torso", new Vector3(0.6f, 1.1f, 0.25f), new Vector3(0f, 1.0f, 0f));
            AddCube("Head", new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0f, 1.8f, 0f), true);
            AddCube("Left_Arm", new Vector3(0.2f, 0.9f, 0.2f), new Vector3(-0.45f, 1.0f, 0f));
            AddCube("Right_Arm", new Vector3(0.2f, 0.9f, 0.2f), new Vector3(0.45f, 1.0f, 0f));
            AddCube("Left_Leg", new Vector3(0.22f, 1.0f, 0.22f), new Vector3(-0.18f, 0.0f, 0f));
            AddCube("Right_Leg", new Vector3(0.22f, 1.0f, 0.22f), new Vector3(0.18f, 0.0f, 0f));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Editor Error] Failed to load editor resources: {ex.Message}");
        }
    }

    private void AddCube(string name, Vector3 scale, Vector3 position, bool frontTex = false)
    {
        var cube = new EditorCube(Gl, name, scale)
        {
            Position = position,
            OnlyFrontTexture = frontTex
        };
        _cubes.Add(cube);
    }

    public override void OnUpdate(double deltaTime)
    {
        float speed = 5.0f * (float)deltaTime;

        if (ImGui.IsKeyPressed(ImGuiKey.Space)) toggleMovement = !toggleMovement;
        
        if (!toggleMovement) return;

        if (ImGui.IsKeyDown(ImGuiKey.LeftArrow)) _camera?.MoveRight(-speed);
        if (ImGui.IsKeyDown(ImGuiKey.RightArrow)) _camera?.MoveRight(speed);
        if (ImGui.IsKeyDown(ImGuiKey.UpArrow)) _camera?.MoveForward(speed);
        if (ImGui.IsKeyDown(ImGuiKey.DownArrow)) _camera?.MoveBackward(speed);
        if (ImGui.IsKeyDown(ImGuiKey.R)) _camera?.MoveUp(speed);
        if (ImGui.IsKeyDown(ImGuiKey.F)) _camera?.MoveDown(speed);
        if (ImGui.IsKeyDown(ImGuiKey.Q)) _camera?.RotateAroundTarget(-30.0f * (float)deltaTime);
        if (ImGui.IsKeyDown(ImGuiKey.E)) _camera?.RotateAroundTarget(30.0f * (float)deltaTime);
    }

    public override void OnRender(double deltaTime)
    {
        Gl.ClearColor(Color.FromArgb(255, 30, 32, 40));
        Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        DrawScene();
        DrawEditorUI();
    }

    private void DrawScene()
    {
        if (_camera == null || _shader == null) return;

        Gl.Enable(EnableCap.DepthTest);

        Matrix4x4 view = _camera.GetViewMatrix();
        Matrix4x4 projection = _camera.GetProjectionMatrix();

        Matrix4x4 modelRootMatrix = Matrix4x4.CreateRotationY(_modelYaw) * Matrix4x4.CreateTranslation(_modelBasePosition);

        foreach (var cube in _cubes)
        {
            if (cube.OnlyFrontTexture && _headTexture != null)
                _headTexture.Bind(TextureUnit.Texture0);
            else
                _bodyTexture?.Bind(TextureUnit.Texture0);

            Matrix4x4 localMatrix = Matrix4x4.CreateScale(cube.Scale) *
                                    Matrix4x4.CreateRotationX(cube.Rotation.X) *
                                    Matrix4x4.CreateRotationY(cube.Rotation.Y) *
                                    Matrix4x4.CreateRotationZ(cube.Rotation.Z) *
                                    Matrix4x4.CreateTranslation(cube.Position);

            Matrix4x4 finalMatrix = localMatrix * modelRootMatrix;

            _shader.Use();
            _shader.SetUniform("uModel", finalMatrix);
            _shader.SetUniform("uView", view);
            _shader.SetUniform("uProjection", projection);

            cube.CubeMesh.Render(_shader, view, projection, finalMatrix);
        }
    }

    private void DrawEditorUI()
    {
        ImGui.SetNextWindowPos(new Vector2(10, 10), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new Vector2(420, 680), ImGuiCond.FirstUseEver);

        ImGui.Begin("3D Model Editor");

        ImGui.Text($"Movement Toggle: {(toggleMovement ? "ON" : "OFF")}");
        ImGui.Separator();

        ImGui.Text("MODEL CONTROLS:");
        ImGui.SliderFloat("Rotate Entire Model", ref _modelYaw, -MathF.PI, MathF.PI);

        ImGui.Separator();
        ImGui.Text( "ADD NEW PART:");
        ImGui.InputText("Part Name", ref _newCubeName, 32);
        if (ImGui.Button("Add Cube"))
        {
            AddCube(_newCubeName, new Vector3(0.5f, 0.5f, 0.5f), Vector3.Zero);
            _selectedCubeIndex = _cubes.Count - 1;
        }

        ImGui.Separator();
        ImGui.Text("CUBE LIST:");

        string[] cubeNames = _cubes.Select(c => c.Name).ToArray();
        ImGui.Combo("Select Part", ref _selectedCubeIndex, cubeNames, cubeNames.Length);

        if (_cubes.Count > 0 && _selectedCubeIndex >= 0 && _selectedCubeIndex < _cubes.Count)
        {
            var selected = _cubes[_selectedCubeIndex];

            ImGui.Spacing();
            ImGui.Text($"Editing: {selected.Name}");

            Vector3 pos = selected.Position;
            if (ImGui.SliderFloat3("Position (X, Y, Z)", ref pos, -3.0f, 3.0f))
            {
                selected.Position = pos;
            }

            Vector3 scale = selected.Scale;
            if (ImGui.SliderFloat3("Scale", ref scale, 0.05f, 3.0f))
            {
                selected.Scale = scale;
            }

            Vector3 rot = selected.Rotation;
            if (ImGui.SliderFloat3("Rotation", ref rot, -MathF.PI, MathF.PI))
            {
                selected.Rotation = rot;
            }

            bool onlyFront = selected.OnlyFrontTexture;
            if (ImGui.Checkbox("Head", ref onlyFront))
            {
                selected.OnlyFrontTexture = onlyFront;
            }

            if (ImGui.Button("Delete Part") && _cubes.Count > 1)
            {
                _cubes.RemoveAt(_selectedCubeIndex);
                _selectedCubeIndex = Math.Max(0, _selectedCubeIndex - 1);
            }
        }

        ImGui.Separator();
        if (ImGui.Button("EXPORT C# CODE TO CONSOLE"))
        {
            Console.WriteLine("\n================= GENERATED PLAYER CODE =================");
            foreach (var cube in _cubes)
            {
                Console.WriteLine($"_{cube.Name.ToLower()} = new Cube(gl, {cube.Scale.X:F2}f, {cube.Scale.Y:F2}f, {cube.Scale.Z:F2}f, onlyFrontTexture: {cube.OnlyFrontTexture.ToString().ToLower()});");
                Console.WriteLine($"// Position: new Vector3({cube.Position.X:F2}f, {cube.Position.Y:F2}f, {cube.Position.Z:F2}f);");
            }
            Console.WriteLine("=========================================================\n");
        }

        ImGui.End();
    }

    public override void OnUnload()
    {
        _shader?.Dispose();
        _bodyTexture?.Dispose();
        _headTexture?.Dispose();
        foreach (var cube in _cubes) cube.CubeMesh.Dispose();
    }
}