using Silk.NET.OpenGL;
using System.Numerics;

namespace CloseQuarter.Client.Models;

public class Cube : GameObject
{
    private readonly bool _onlyFrontTexture;

    public Cube(GL gl, float width = 1.0f, float height = 1.0f, float depth = 1.0f, bool onlyFrontTexture = false) : base(gl)
    {
        _onlyFrontTexture = onlyFrontTexture;
        Scale = new Vector3(width, height, depth);
        Initialize();
    }

    protected override (float[] vertices, uint[] indices) GetMeshData()
    {
        float uMin = 0.0f, uMax = 1.0f;
        float vMin = 0.0f, vMax = 1.0f;

        float defaultU = 0.0f;
        float defaultV = 0.0f;

        float[] vertices = new float[]
        {
            -0.5f,  0.5f, -0.5f,     _onlyFrontTexture ? defaultU : uMin, _onlyFrontTexture ? defaultV : vMax,
             0.5f,  0.5f, -0.5f,     _onlyFrontTexture ? defaultU : uMax, _onlyFrontTexture ? defaultV : vMax,
             0.5f,  0.5f,  0.5f,     _onlyFrontTexture ? defaultU : uMax, _onlyFrontTexture ? defaultV : vMin,
            -0.5f,  0.5f,  0.5f,     _onlyFrontTexture ? defaultU : uMin, _onlyFrontTexture ? defaultV : vMin,

            -0.5f, -0.5f, -0.5f,     _onlyFrontTexture ? defaultU : uMin, _onlyFrontTexture ? defaultV : vMax,
             0.5f, -0.5f, -0.5f,     _onlyFrontTexture ? defaultU : uMax, _onlyFrontTexture ? defaultV : vMax,
             0.5f, -0.5f,  0.5f,     _onlyFrontTexture ? defaultU : uMax, _onlyFrontTexture ? defaultV : vMin,
            -0.5f, -0.5f,  0.5f,     _onlyFrontTexture ? defaultU : uMin, _onlyFrontTexture ? defaultV : vMin,

            -0.5f, -0.5f,  0.5f,     uMin, vMin,
             0.5f, -0.5f,  0.5f,     uMax, vMin,
             0.5f,  0.5f,  0.5f,     uMax, vMax,
            -0.5f,  0.5f,  0.5f,     uMin, vMax,

            -0.5f, -0.5f, -0.5f,     _onlyFrontTexture ? defaultU : uMax, _onlyFrontTexture ? defaultV : vMin,
             0.5f, -0.5f, -0.5f,     _onlyFrontTexture ? defaultU : uMin, _onlyFrontTexture ? defaultV : vMin,
             0.5f,  0.5f, -0.5f,     _onlyFrontTexture ? defaultU : uMin, _onlyFrontTexture ? defaultV : vMax,
            -0.5f,  0.5f, -0.5f,     _onlyFrontTexture ? defaultU : uMax, _onlyFrontTexture ? defaultV : vMax,

            -0.5f, -0.5f, -0.5f,     _onlyFrontTexture ? defaultU : uMin, _onlyFrontTexture ? defaultV : vMin,
            -0.5f, -0.5f,  0.5f,     _onlyFrontTexture ? defaultU : uMax, _onlyFrontTexture ? defaultV : vMin,
            -0.5f,  0.5f,  0.5f,     _onlyFrontTexture ? defaultU : uMax, _onlyFrontTexture ? defaultV : vMax,
            -0.5f,  0.5f, -0.5f,     _onlyFrontTexture ? defaultU : uMin, _onlyFrontTexture ? defaultV : vMax,

             0.5f, -0.5f, -0.5f,     _onlyFrontTexture ? defaultU : uMax, _onlyFrontTexture ? defaultV : vMin,
             0.5f, -0.5f,  0.5f,     _onlyFrontTexture ? defaultU : uMin, _onlyFrontTexture ? defaultV : vMin,
             0.5f,  0.5f,  0.5f,     _onlyFrontTexture ? defaultU : uMin, _onlyFrontTexture ? defaultV : vMax,
             0.5f,  0.5f, -0.5f,     _onlyFrontTexture ? defaultU : uMax, _onlyFrontTexture ? defaultV : vMax,
        };

        uint[] indices = new uint[]
        {
            0, 1, 2,  2, 3, 0,    
            4, 5, 6,  6, 7, 4,    
            8, 9, 10, 10, 11, 8, 
            12, 13, 14, 14, 15, 12,
            16, 17, 18, 18, 19, 16,
            20, 21, 22, 22, 23, 20 
        };

        return (vertices, indices);
    }
}