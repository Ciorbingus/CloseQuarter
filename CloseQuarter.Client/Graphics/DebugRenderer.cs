using Silk.NET.OpenGL;
using System.Numerics;

using CloseQuarter.Client.Managers; 

namespace CloseQuarter.Client.Graphics;

public class DebugRenderer : IDisposable
{
    private readonly GL _gl;
    private uint _vaoCircle;
    private uint _vboCircle;
    private int _circleVertexCount;

    private uint _vaoLine;
    private uint _vboLine;

    private uint _vaoVerticals;
    private uint _vboVerticals;

    public DebugRenderer(GL gl, int segments = 32)
    {
        _gl = gl;
        _circleVertexCount = segments;

        List<float> circleVertices = new List<float>();
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * MathF.PI * 2.0f;
            float x = MathF.Cos(angle);
            float z = MathF.Sin(angle);

            circleVertices.Add(x);
            circleVertices.Add(0.0f); 
            circleVertices.Add(z);
        }

        _vaoCircle = _gl.GenVertexArray();
        _vboCircle = _gl.GenBuffer();

        _gl.BindVertexArray(_vaoCircle);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vboCircle);

        unsafe
        {
            fixed (float* v = circleVertices.ToArray())
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(circleVertices.Count * sizeof(float)), v, GLEnum.StaticDraw);
            }

            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
        }

        float[] lineVertices = new float[]
        {
            0.0f, 0.05f, 0.0f, 
            0.0f, 0.05f, 1.5f 
        };

        _vaoLine = _gl.GenVertexArray();
        _vboLine = _gl.GenBuffer();

        _gl.BindVertexArray(_vaoLine);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vboLine);

        unsafe
        {
            fixed (float* v = lineVertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(lineVertices.Length * sizeof(float)), v, GLEnum.StaticDraw);
            }

            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
        }

        float h = PhysicsManager.PlayerHeight; 
        float[] verticalLines = new float[]
        {
            0.0f, 0.0f, 1.0f,   0.0f, h, 1.0f,
            0.0f, 0.0f, -1.0f,  0.0f, h, -1.0f,
            1.0f, 0.0f, 0.0f,   1.0f, h, 0.0f,
            -1.0f, 0.0f, 0.0f,  -1.0f, h, 0.0f
        };

        _vaoVerticals = _gl.GenVertexArray();
        _vboVerticals = _gl.GenBuffer();

        _gl.BindVertexArray(_vaoVerticals);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vboVerticals);

        unsafe
        {
            fixed (float* v = verticalLines)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(verticalLines.Length * sizeof(float)), v, GLEnum.StaticDraw);
            }

            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
        }

        _gl.BindVertexArray(0);
    }

    public void DrawPlayerHitbox(MyShader shader, Matrix4x4 playerBodyMatrix, float radius, Matrix4x4 view, Matrix4x4 projection)
    {
        shader.Use();
        shader.SetUniform("uView", view);
        shader.SetUniform("uProjection", projection);

        float height = PhysicsManager.PlayerHeight;

        _gl.BindVertexArray(_vaoCircle);

        Matrix4x4 bottomCircle = Matrix4x4.CreateScale(radius, 1.0f, radius) * playerBodyMatrix;
        shader.SetUniform("uModel", bottomCircle);
        _gl.DrawArrays(PrimitiveType.LineLoop, 0, (uint)_circleVertexCount);

        Matrix4x4 midCircle = Matrix4x4.CreateScale(radius, 1.0f, radius) * 
                              Matrix4x4.CreateTranslation(0.0f, height * 0.5f, 0.0f) * 
                              playerBodyMatrix;
        shader.SetUniform("uModel", midCircle);
        _gl.DrawArrays(PrimitiveType.LineLoop, 0, (uint)_circleVertexCount);

        Matrix4x4 topCircle = Matrix4x4.CreateScale(radius, 1.0f, radius) * 
                            Matrix4x4.CreateTranslation(0.0f, height, 0.0f) * 
                            playerBodyMatrix;
        shader.SetUniform("uModel", topCircle);
        _gl.DrawArrays(PrimitiveType.LineLoop, 0, (uint)_circleVertexCount);


        Matrix4x4 cylinderModel = Matrix4x4.CreateScale(radius, 1.0f, radius) * playerBodyMatrix;
        shader.SetUniform("uModel", cylinderModel);

        _gl.BindVertexArray(_vaoVerticals);
        _gl.DrawArrays(PrimitiveType.Lines, 0, 8);


        shader.SetUniform("uModel", playerBodyMatrix);

        _gl.BindVertexArray(_vaoLine);
        _gl.DrawArrays(PrimitiveType.Lines, 0, 2);

        _gl.BindVertexArray(0);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vaoCircle);
        _gl.DeleteBuffer(_vboCircle);
        _gl.DeleteVertexArray(_vaoLine);
        _gl.DeleteBuffer(_vboLine);
        _gl.DeleteVertexArray(_vaoVerticals);
        _gl.DeleteBuffer(_vboVerticals);
    }
}