using Silk.NET.OpenGL;
using System.Numerics;
using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models;

public class Ground : IDisposable
{
    private readonly GL _gl;
    private uint _vao;
    private uint _vbo;
    private uint _ebo;

    public Ground(GL gl)
    {
        _gl = gl;
    }

    public void Initialize(float size = 50.0f)
    {
        float half = size / 2.0f;

        float[] vertices = new float[]
        {
            -half, 0.0f, -half,      0.0f, 0.0f, 
             half, 0.0f, -half,      1.0f, 0.0f, 
             half, 0.0f,  half,      1.0f, 1.0f, 
            -half, 0.0f,  half,      0.0f, 1.0f  
        };

        uint[] indices = new uint[]
        {
            0, 1, 2,
            2, 3, 0
        };

        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _ebo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        unsafe
        {
            fixed (float* v = vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, GLEnum.StaticDraw);
            }
        }

        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        unsafe
        {
            fixed (uint* i = indices)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, GLEnum.StaticDraw);
            }
        }

        _gl.EnableVertexAttribArray(0);
        unsafe
        {
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
        }

        _gl.EnableVertexAttribArray(1);
        unsafe
        {
            _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
        }

        _gl.BindVertexArray(0);
    }

    public void Render(MyShader shader, Matrix4x4 view, Matrix4x4 projection)
    {
        shader.Use();

        Matrix4x4 model = Matrix4x4.Identity;

        shader.SetUniform("uModel", model);
        shader.SetUniform("uView", view);
        shader.SetUniform("uProjection", projection);

        _gl.BindVertexArray(_vao);
        unsafe
        {
            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
        }
        _gl.BindVertexArray(0);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
    }
}