using Silk.NET.OpenGL;
using System.Numerics;

using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models;

public abstract class GameObject : IDisposable
{
    protected readonly GL Gl;

    protected uint Vao;
    protected uint Vbo;
    protected uint Ebo;
    protected uint IndexCount;

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;

    protected GameObject(GL gl)
    {
        Gl = gl;
    }

    protected abstract (float[] vertices, uint[] indices) GetMeshData();

    public virtual unsafe void Initialize()
    {
        var (vertices, indices) = GetMeshData();
        IndexCount = (uint)indices.Length;

        Vao = Gl.GenVertexArray();
        Vbo = Gl.GenBuffer();
        Ebo = Gl.GenBuffer();

        Gl.BindVertexArray(Vao);

        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo);
        fixed (float* ptr = vertices)
        {
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), ptr, BufferUsageARB.StaticDraw);
        }

        Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, Ebo);
        fixed (uint* ptr = indices)
        {
            Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), ptr, BufferUsageARB.StaticDraw);
        }

        uint stride = 5 * sizeof(float);

        Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
        Gl.EnableVertexAttribArray(0);

        Gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
        Gl.EnableVertexAttribArray(1);

        Gl.BindVertexArray(0);
    }

    public virtual Matrix4x4 GetModelMatrix()
    {
        return Matrix4x4.CreateScale(Scale) *
               Matrix4x4.CreateRotationX(Rotation.X) *
               Matrix4x4.CreateRotationY(Rotation.Y) *
               Matrix4x4.CreateRotationZ(Rotation.Z) *
               Matrix4x4.CreateTranslation(Position);
    }



    public virtual void Render(MyShader shader, Matrix4x4 view, Matrix4x4 projection)
    {
        Render(shader, view, projection, GetModelMatrix());
    }

    public virtual void Render(MyShader shader, Matrix4x4 view, Matrix4x4 projection, Matrix4x4 customModelMatrix)
    {
        if (Vao == 0) return;

        shader.Use();
        shader.SetUniform("uModel", customModelMatrix);
        shader.SetUniform("uView", view);
        shader.SetUniform("uProjection", projection);

        Gl.BindVertexArray(Vao);
        unsafe
        {
            Gl.DrawElements(PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, (void*)0);
        }
        Gl.BindVertexArray(0);
    }


    public virtual void Dispose()
    {
        if (Vao != 0) Gl.DeleteVertexArray(Vao);
        if (Vbo != 0) Gl.DeleteBuffer(Vbo);
        if (Ebo != 0) Gl.DeleteBuffer(Ebo);
    }
}