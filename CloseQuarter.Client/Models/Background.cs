using Silk.NET.OpenGL;

using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models; 

public class Background : IDisposable
{
    private readonly GL _gl;
    private uint _vao;
    private uint _vbo;
    private MyShader? _shader;
    private MyTexture? _texture;

    public Background(GL gl)
    {
        _gl = gl;
    }

    public unsafe void Initialize(string texturePath, string vertShaderPath = "Shaders/background.vert", string fragShaderPath = "Shaders/background.frag")
    {
        _shader = MyShader.FromFiles(_gl, vertShaderPath, fragShaderPath);
        _texture = new MyTexture(_gl, texturePath);

        float[] vertices = new float[]
        {
            -1.0f,  1.0f,  0.0f, 1.0f,
            -1.0f, -1.0f,  0.0f, 0.0f,
             1.0f, -1.0f,  1.0f, 0.0f,

            -1.0f,  1.0f,  0.0f, 1.0f,
             1.0f, -1.0f,  1.0f, 0.0f,
             1.0f,  1.0f,  1.0f, 1.0f
        };

        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* ptr = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), ptr, BufferUsageARB.StaticDraw);
        }

        uint stride = 4 * sizeof(float);

        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*)0);
        _gl.EnableVertexAttribArray(0);

        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(2 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        _gl.BindVertexArray(0);
    }

    public void Render()
    {
        if (_shader == null || _texture == null || _vao == 0) return;

        _gl.Disable(EnableCap.DepthTest);

        _shader.Use();
        _texture.Bind(TextureUnit.Texture0);

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        _gl.BindVertexArray(0);

        _gl.Enable(EnableCap.DepthTest);
    }

    public void Dispose()
    {
        _texture?.Dispose();
        _shader?.Dispose();

        if (_vao != 0) _gl.DeleteVertexArray(_vao);
        if (_vbo != 0) _gl.DeleteBuffer(_vbo);
    }
}