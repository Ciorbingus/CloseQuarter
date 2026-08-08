using System.Numerics;

using CloseQuarter.Client.Models;

namespace CloseQuarter.Client.Managers;

public static class PhysicsManager
{
    public const float RingRadius = 7.0f;
    public const float Gravity = -20.0f;
    public const float RingSurfaceY = 0.0f;

    public static void ApplyGravityAndFloor(Player player, float deltaTime)
    {
        player.VelocityY += Gravity * deltaTime;

        Vector3 pos = player.Position;
        pos.Y += player.VelocityY * deltaTime;

        if (pos.Y <= RingSurfaceY && player.VelocityY <= 0.0f)
        {
            pos.Y = RingSurfaceY;
            player.VelocityY = 0.0f;
            player.IsGrounded = true;

            if (player.CurrentState == PlayerState.Jumping ||
                player.CurrentState == PlayerState.JumpingForward ||
                player.CurrentState == PlayerState.JumpingBackward ||
                player.CurrentState == PlayerState.Falling)
            {
                player.CurrentState = PlayerState.Idle;
            }
        }
        else
        {
            player.IsGrounded = false;

            if (!player.IsAttacking && player.CurrentState != PlayerState.Hit && player.CurrentState != PlayerState.KnockedOut)
            {
                if (player.VelocityY < 0.0f)
                {
                    player.CurrentState = PlayerState.Falling;
                }
            }
        }

        player.Position = pos;
    }

    public static void KeepPlayerInRing(Player player)
    {
        Vector3 pos = player.Position;
        Vector2 pos2D = new Vector2(pos.X, pos.Z);

        if (pos2D.Length() > RingRadius - player.Radius)
        {
            pos2D = Vector2.Normalize(pos2D) * (RingRadius - player.Radius);
            player.Position = new Vector3(pos2D.X, pos.Y, pos2D.Y);
        }
    }

    public static void ResolvePlayerCollision(Player p1, Player p2)
    {
        Vector3 p1Pos = p1.Position;
        Vector3 p2Pos = p2.Position;

        float yMin1 = p1Pos.Y;
        float yMax1 = p1Pos.Y + p1.Height;

        float yMin2 = p2Pos.Y;
        float yMax2 = p2Pos.Y + p2.Height;

        bool overlapY = (yMin1 < yMax2) && (yMax1 > yMin2);

        if (!overlapY) return;

        Vector2 deltaXZ = new Vector2(p1Pos.X - p2Pos.X, p1Pos.Z - p2Pos.Z);
        float distanceXZ = deltaXZ.Length();

        float minDistanceXZ = p1.Radius + p2.Radius;

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
                if (p1Pos.Y > p2Pos.Y + (p2.Height * 0.5f))
                {
                    p1.Position += new Vector3(pushDir.X, 0, pushDir.Y) * 0.05f;
                }
                else if (p2Pos.Y > p1Pos.Y + (p1.Height * 0.5f))
                {
                    p2.Position -= new Vector3(pushDir.X, 0, pushDir.Y) * 0.05f;
                }
            }
        }
    }

    public static bool CheckPunchHit(Vector3 punchPos, float punchRadius, Player defender)
    {
        Vector3 defPos = defender.Position;

        if (punchPos.Y + punchRadius < defPos.Y || punchPos.Y - punchRadius > defPos.Y + defender.Height)
        {
            return false;
        }

        Vector2 punchXZ = new Vector2(punchPos.X, punchPos.Z);
        Vector2 defXZ = new Vector2(defPos.X, defPos.Z);

        float distanceXZ = Vector2.Distance(punchXZ, defXZ);
        float maxDistanceXZ = punchRadius + defender.Radius;

        return distanceXZ <= maxDistanceXZ;
    }
}