using Silk.NET.OpenGL;
using System.Numerics;

namespace CloseQuarter.Client.Models;

public class Ring : GameObject
{
    public Ring(GL gl) : base(gl)
    {
        Scale = new Vector3(12.0f, 1.5f, 8.0f);
        Position = new Vector3(0.0f, -1.0f, 0.0f);
    }

    protected override (float[] vertices, uint[] indices) GetMeshData()
    {
        float[] vertices = new float[]
        {
            -0.5f,  0.5f, -0.5f,     0.6f, 0.6f, 0.65f,
             0.5f,  0.5f, -0.5f,     0.6f, 0.6f, 0.65f,
             0.5f,  0.5f,  0.5f,     0.6f, 0.6f, 0.65f,
            -0.5f,  0.5f,  0.5f,     0.6f, 0.6f, 0.65f,

            -0.5f, -0.5f, -0.5f,     0.2f, 0.2f, 0.2f,
             0.5f, -0.5f, -0.5f,     0.2f, 0.2f, 0.2f,
             0.5f, -0.5f,  0.5f,     0.2f, 0.2f, 0.2f,
            -0.5f, -0.5f,  0.5f,     0.2f, 0.2f, 0.2f,

            -0.5f, -0.5f,  0.5f,     0.8f, 0.2f, 0.2f,
             0.5f, -0.5f,  0.5f,     0.8f, 0.2f, 0.2f,
             0.5f,  0.5f,  0.5f,     0.8f, 0.2f, 0.2f,
            -0.5f,  0.5f,  0.5f,     0.8f, 0.2f, 0.2f,

            -0.5f, -0.5f, -0.5f,     0.3f, 0.3f, 0.35f,
             0.5f, -0.5f, -0.5f,     0.3f, 0.3f, 0.35f,
             0.5f,  0.5f, -0.5f,     0.3f, 0.3f, 0.35f,
            -0.5f,  0.5f, -0.5f,     0.3f, 0.3f, 0.35f,

            -0.5f, -0.5f, -0.5f,     0.2f, 0.4f, 0.8f,
            -0.5f, -0.5f,  0.5f,     0.2f, 0.4f, 0.8f,
            -0.5f,  0.5f,  0.5f,     0.2f, 0.4f, 0.8f,
            -0.5f,  0.5f, -0.5f,     0.2f, 0.4f, 0.8f,

             0.5f, -0.5f, -0.5f,     0.3f, 0.3f, 0.35f,
             0.5f, -0.5f,  0.5f,     0.3f, 0.3f, 0.35f,
             0.5f,  0.5f,  0.5f,     0.3f, 0.3f, 0.35f,
             0.5f,  0.5f, -0.5f,     0.3f, 0.3f, 0.35f,
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