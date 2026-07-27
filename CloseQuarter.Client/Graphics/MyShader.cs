using Silk.NET.OpenGL;
using System.Numerics;

namespace CloseQuarter.Client.Graphics;

public class MyShader : IDisposable
{
    private readonly GL _gl;
    public uint Handle { get; private set; }

    public MyShader(GL gl, string vertexCode, string fragmentCode)
    {
        _gl = gl;

        uint vertex = CompileShader(ShaderType.VertexShader, vertexCode);
        uint fragment = CompileShader(ShaderType.FragmentShader, fragmentCode);

        Handle = _gl.CreateProgram();
        _gl.AttachShader(Handle, vertex);
        _gl.AttachShader(Handle, fragment);
        _gl.LinkProgram(Handle);

        _gl.GetProgram(Handle, ProgramPropertyARB.LinkStatus, out int status);
        if (status == 0)
        {
            throw new Exception($"Error linking shader program: {_gl.GetProgramInfoLog(Handle)}");
        }

        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    public static MyShader FromFiles(GL gl, string vertexPath, string fragmentPath)
    {
        string fullVertexPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, vertexPath);
        string fullFragmentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fragmentPath);

        if (!File.Exists(fullVertexPath))
            throw new FileNotFoundException($"Vertex shader file not found at: {fullVertexPath}");

        if (!File.Exists(fullFragmentPath))
            throw new FileNotFoundException($"Fragment shader file not found at: {fullFragmentPath}");

        string vertexCode = File.ReadAllText(fullVertexPath);
        string fragmentCode = File.ReadAllText(fullFragmentPath);

        return new MyShader(gl, vertexCode, fragmentCode);
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status == 0)
        {
            throw new Exception($"Error compiling {type}: {_gl.GetShaderInfoLog(shader)}");
        }

        return shader;
    }

    public void Use() => _gl.UseProgram(Handle);

    public void SetUniform(string name, Matrix4x4 matrix)
    {
        int location = _gl.GetUniformLocation(Handle, name);
        if (location != -1)
        {
            unsafe
            {
                _gl.UniformMatrix4(location, 1, false, (float*)&matrix);
            }
        }
    }

    public void Dispose()
    {
        _gl.DeleteProgram(Handle);
    }
}