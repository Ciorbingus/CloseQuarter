using System.Numerics;
using CloseQuarter.Client.Models;

namespace CloseQuarter.Client.Managers;

public static class PhysicsManager
{
    public const float PlayerRadius = 0.45f;
    public const float RingRadius = 7.0f; 

    public const float Gravity = -20.0f; 

    public const float RingSurfaceY = 0.0f; 


    public static void ApplyGravityAndFloor(Player player, float deltaTime)
    {
        player.VelocityY += Gravity * deltaTime;

        Vector3 pos = player.Position;
        pos.Y += player.VelocityY * deltaTime;

        if (pos.Y <= RingSurfaceY)
        {
            pos.Y = RingSurfaceY; 
            player.VelocityY = 0.0f;
            player.IsGrounded = true;
        }

        player.Position = pos;
    }

    public static void KeepPlayerInRing(Player player)
    {
        Vector2 pos2D = new Vector2(player.Position.X, player.Position.Z);
        float distance = pos2D.Length();
        float maxAllowedDistance = RingRadius - PlayerRadius;

        if (distance > maxAllowedDistance)
        {
            Vector2 clampedPos = Vector2.Normalize(pos2D) * maxAllowedDistance;
            player.Position = new Vector3(clampedPos.X, player.Position.Y, clampedPos.Y);
        }
    }

    public static void ResolvePlayerCollision(Player p1, Player p2)
    {
        Vector2 pos1 = new Vector2(p1.Position.X, p1.Position.Z);
        Vector2 pos2 = new Vector2(p2.Position.X, p2.Position.Z);

        Vector2 delta = pos1 - pos2;
        float distance = delta.Length();
        float minDistance = PlayerRadius * 2.0f;

        if (distance < minDistance && distance > 0.0001f)
        {
            float overlap = minDistance - distance;
            Vector2 pushDirection = Vector2.Normalize(delta);

            Vector2 newPos1 = pos1 + pushDirection * (overlap / 2.0f);
            Vector2 newPos2 = pos2 - pushDirection * (overlap / 2.0f);

            p1.Position = new Vector3(newPos1.X, p1.Position.Y, newPos1.Y);
            p2.Position = new Vector3(newPos2.X, p2.Position.Y, newPos2.Y);
        }
    }
}