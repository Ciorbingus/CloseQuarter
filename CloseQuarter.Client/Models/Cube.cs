using Silk.NET.OpenGL;
using System.Numerics;

namespace CloseQuarter.Client.Models;

public class Cube : GameObject
{
    public Cube(GL gl, float width = 1.0f, float height = 1.0f, float depth = 1.0f) : base(gl)
    {
        Scale = new Vector3(width, height, depth);
        Initialize();
    }

    protected override (float[] vertices, uint[] indices) GetMeshData()
    {
        float[] vertices = new float[]
        {
            -0.5f,  0.5f, -0.5f,     0.0f, 1.0f,
             0.5f,  0.5f, -0.5f,     1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,     1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,     0.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,     0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,     1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,     1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,     0.0f, 0.0f,

            -0.5f, -0.5f,  0.5f,     0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,     1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,     1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,     0.0f, 1.0f,

            -0.5f, -0.5f, -0.5f,     1.0f, 0.0f,
             0.5f, -0.5f, -0.5f,     0.0f, 0.0f,
             0.5f,  0.5f, -0.5f,     0.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,     1.0f, 1.0f,

            -0.5f, -0.5f, -0.5f,     0.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,     1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,     1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,     0.0f, 1.0f,

             0.5f, -0.5f, -0.5f,     1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,     0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,     0.0f, 1.0f,
             0.5f,  0.5f, -0.5f,     1.0f, 1.0f,
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