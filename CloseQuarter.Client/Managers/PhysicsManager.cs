using System.Numerics;

using CloseQuarter.Client.Models;

namespace CloseQuarter.Client.Managers;

public static class PhysicsManager
{
    public const float PlayerRadius = 0.15f;
    public const float PlayerHeight = 1.8f;
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
        Vector3 pos = player.Position;
        Vector2 pos2D = new Vector2(pos.X, pos.Z);

        if (pos2D.Length() > RingRadius - PlayerRadius)
        {
            pos2D = Vector2.Normalize(pos2D) * (RingRadius - PlayerRadius);
            player.Position = new Vector3(pos2D.X, pos.Y, pos2D.Y);
        }
    }

    public static void ResolvePlayerCollision(Player p1, Player p2)
    {
        Vector3 p1Pos = p1.Position;
        Vector3 p2Pos = p2.Position;

        float yMin1 = p1Pos.Y;
        float yMax1 = p1Pos.Y + PlayerHeight;

        float yMin2 = p2Pos.Y;
        float yMax2 = p2Pos.Y + PlayerHeight;

        bool overlapY = (yMin1 < yMax2) && (yMax1 > yMin2);

        if (!overlapY) return;

        Vector2 deltaXZ = new Vector2(p1Pos.X - p2Pos.X, p1Pos.Z - p2Pos.Z);
        float distanceXZ = deltaXZ.Length();
        float minDistanceXZ = PlayerRadius * 2.0f;

        if (distanceXZ < minDistanceXZ)
        {
            float overlapXZ = minDistanceXZ - distanceXZ;

            Vector2 pushDir;
            if (distanceXZ < 0.001f)
            {
                pushDir = new Vector2(1.0f, 0.0f);
            }
            else
            {
                pushDir = Vector2.Normalize(deltaXZ);
            }

            Vector2 pushAmount = pushDir * (overlapXZ * 0.5f);

            p1.Position = new Vector3(p1Pos.X + pushAmount.X, p1Pos.Y, p1Pos.Z + pushAmount.Y);
            p2.Position = new Vector3(p2Pos.X - pushAmount.X, p2Pos.Y, p2Pos.Z - pushAmount.Y);

            if (!p1.IsGrounded || !p2.IsGrounded)
            {
                if (p1Pos.Y > p2Pos.Y + (PlayerHeight * 0.5f))
                {
                    p1.Position += new Vector3(pushDir.X, 0, pushDir.Y) * 0.05f;
                }
                else if (p2Pos.Y > p1Pos.Y + (PlayerHeight * 0.5f))
                {
                    p2.Position -= new Vector3(pushDir.X, 0, pushDir.Y) * 0.05f;
                }
            }
        }
    }
}