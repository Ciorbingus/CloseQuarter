using Silk.NET.OpenGL;
using StbImageSharp;

namespace CloseQuarter.Client.Graphics;

public class MyTexture : IDisposable
{
    private readonly GL _gl;
    public uint Handle { get; private set; }

    public MyTexture(GL gl, string filePath)
    {
        _gl = gl;

        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"[Texture Error] Image file not found: {fullPath}");
        }

        Handle = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, Handle);

        _gl.TextureParameter(Handle, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TextureParameter(Handle, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.TextureParameter(Handle, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TextureParameter(Handle, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

        StbImage.stbi_set_flip_vertically_on_load(1); 
        using var stream = File.OpenRead(fullPath);
        ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        unsafe
        {
            fixed (byte* ptr = image.Data)
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, 
                               (uint)image.Width, (uint)image.Height, 0, 
                               PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }
        }

        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Bind(TextureUnit unit = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(unit);
        _gl.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public void Dispose()
    {
        _gl.DeleteTexture(Handle);
    }
}