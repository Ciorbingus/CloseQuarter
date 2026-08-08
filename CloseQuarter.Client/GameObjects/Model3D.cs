using Silk.NET.OpenGL;
using System.Numerics;
using CloseQuarter.Client.Graphics;

namespace CloseQuarter.Client.Models;

public class Model3D : IDisposable
{
    public Torso Torso { private set; get; }
    public Head Head { private set; get; }
    public Arm LeftArm { private set; get; }
    public Arm RightArm { private set; get; }
    public Leg LeftLeg { private set; get; }
    public Leg RightLeg { private set; get; }

    public Model3D(GL gl)
    {
        Torso = new Torso(gl);
        Head = new Head(gl);
        LeftArm = new Arm(gl);
        RightArm = new Arm(gl);
        LeftLeg = new Leg(gl);
        RightLeg = new Leg(gl);

        LeftLeg.Position = new Vector3(-0.16f, 0.6f, 0.0f);
        RightLeg.Position = new Vector3(0.16f, 0.6f, 0.0f);

        Torso.Position = new Vector3(0.0f, 1.05f, 0.0f);

        LeftArm.Position = new Vector3(-0.42f, 0.15f, 0.0f);
        RightArm.Position = new Vector3(0.42f, 0.15f, 0.0f);

        Head.Position = new Vector3(0.0f, 0.45f, 0.0f);
    }

    public virtual void LoadTextures(GL gl, string bodyTexturePath, string? headTexturePath = null)
    {
        Torso.LoadTextures(gl, bodyTexturePath);
        LeftArm.LoadTextures(gl, bodyTexturePath);
        RightArm.LoadTextures(gl, bodyTexturePath);
        LeftLeg.LoadTextures(gl, bodyTexturePath);
        RightLeg.LoadTextures(gl, bodyTexturePath);

        if (!string.IsNullOrEmpty(bodyTexturePath))
            Head.LoadTextures(gl, bodyTexturePath);
        else
            Head.LoadTextures(gl, bodyTexturePath);
    }

    // --- ANIMAȚII GENERICE DE BAZĂ ---

    public virtual void AnimateIdle(float idleTime)
    {
        float pulse = MathF.Sin(idleTime * 6.0f);
        float breath = pulse * 2.0f;

        Torso.AbdomenRotation = new Vector3(12.0f + breath, 15.0f, 0.0f);
        Torso.ChestRotation = new Vector3(8.0f, 10.0f, -5.0f);

        Head.Rotation = new Vector3(-15.0f - breath, -5.0f, 0.0f);

        LeftArm.ShoulderRotation = new Vector3(-55.0f + breath, 20.0f, 0.0f);
        LeftArm.LowerArmRotation = new Vector3(-75.0f, 0.0f, 0.0f);

        RightArm.ShoulderRotation = new Vector3(-45.0f, -25.0f, 0.0f);
        RightArm.LowerArmRotation = new Vector3(-90.0f + breath, 0.0f, 0.0f);

        float p1UpperX = -25.0f;
        float p1LowerX = 40.0f - breath;
        LeftLeg.UpperLegRotation = new Vector3(p1UpperX, 0.0f, -10.0f);
        LeftLeg.LowerLegRotation = new Vector3(p1LowerX, 0.0f, 0.0f);
        LeftLeg.FootRotation = new Vector3(-(p1UpperX + p1LowerX), 0.0f, 10.0f);

        float p2UpperX = 15.0f;
        float p2LowerX = 25.0f + breath;
        RightLeg.UpperLegRotation = new Vector3(p2UpperX, 0.0f, 10.0f);
        RightLeg.LowerLegRotation = new Vector3(p2LowerX, 0.0f, 0.0f);
        RightLeg.FootRotation = new Vector3(-(p2UpperX + p2LowerX), 0.0f, -10.0f);
    }

    public virtual void AnimateWalk(float walkTime, float speed = 10.0f)
    {
        float angle = MathF.Sin(walkTime * speed);
        float legSwing = angle * 25.0f;
        float armSwing = angle * 15.0f;

        Torso.AbdomenRotation = new Vector3(10.0f, MathF.Cos(walkTime * speed) * 4.0f, 0.0f);
        Torso.ChestRotation = new Vector3(8.0f, MathF.Sin(walkTime * speed) * 6.0f, 0.0f);
        Head.Rotation = new Vector3(-12.0f, 0.0f, 0.0f);

        float p1Upper = -15.0f - legSwing;
        float p1Lower = 35.0f + (legSwing < 0 ? -legSwing * 0.6f : 0f);
        LeftLeg.UpperLegRotation = new Vector3(p1Upper, 0.0f, -5.0f);
        LeftLeg.LowerLegRotation = new Vector3(p1Lower, 0.0f, 0.0f);
        LeftLeg.FootRotation = new Vector3(-(p1Upper + p1Lower), 0.0f, 5.0f);

        float p2Upper = -15.0f + legSwing;
        float p2Lower = 35.0f + (legSwing > 0 ? legSwing * 0.6f : 0f);
        RightLeg.UpperLegRotation = new Vector3(p2Upper, 0.0f, 5.0f);
        RightLeg.LowerLegRotation = new Vector3(p2Lower, 0.0f, 0.0f);
        RightLeg.FootRotation = new Vector3(-(p2Upper + p2Lower), 0.0f, -5.0f);

        LeftArm.ShoulderRotation = new Vector3(-50.0f + armSwing, 15.0f, 0.0f);
        LeftArm.LowerArmRotation = new Vector3(-75.0f, 0.0f, 0.0f);

        RightArm.ShoulderRotation = new Vector3(-45.0f - armSwing, -20.0f, 0.0f);
        RightArm.LowerArmRotation = new Vector3(-85.0f, 0.0f, 0.0f);
    }

    public virtual void AnimateJump()
    {
        Torso.AbdomenRotation = new Vector3(20.0f, 0.0f, 0.0f);
        Torso.ChestRotation = new Vector3(15.0f, 0.0f, 0.0f);
        Head.Rotation = new Vector3(-20.0f, 0.0f, 0.0f);

        LeftLeg.UpperLegRotation = new Vector3(-65.0f, 0.0f, -10.0f);
        LeftLeg.LowerLegRotation = new Vector3(85.0f, 0.0f, 0.0f);
        LeftLeg.FootRotation = new Vector3(10.0f, 0.0f, 0.0f);

        RightLeg.UpperLegRotation = new Vector3(-50.0f, 0.0f, 10.0f);
        RightLeg.LowerLegRotation = new Vector3(75.0f, 0.0f, 0.0f);
        RightLeg.FootRotation = new Vector3(10.0f, 0.0f, 0.0f);

        LeftArm.ShoulderRotation = new Vector3(-55.0f, 20.0f, 0.0f);
        LeftArm.LowerArmRotation = new Vector3(-85.0f, 0.0f, 0.0f);

        RightArm.ShoulderRotation = new Vector3(-55.0f, -20.0f, 0.0f);
        RightArm.LowerArmRotation = new Vector3(-85.0f, 0.0f, 0.0f);
    }

    public virtual void AnimateCrouch()
    {
        Torso.AbdomenRotation = new Vector3(25.0f, 10.0f, 0.0f);
        Torso.ChestRotation = new Vector3(15.0f, 5.0f, 0.0f);
        Head.Rotation = new Vector3(-20.0f, 0.0f, 0.0f);

        float p1Upper = -50.0f;
        float p1Lower = 85.0f;
        LeftLeg.UpperLegRotation = new Vector3(p1Upper, 0.0f, -10.0f);
        LeftLeg.LowerLegRotation = new Vector3(p1Lower, 0.0f, 0.0f);
        LeftLeg.FootRotation = new Vector3(-(p1Upper + p1Lower), 0.0f, 10.0f);

        float p2Upper = -35.0f;
        float p2Lower = 75.0f;
        RightLeg.UpperLegRotation = new Vector3(p2Upper, 0.0f, 10.0f);
        RightLeg.LowerLegRotation = new Vector3(p2Lower, 0.0f, 0.0f);
        RightLeg.FootRotation = new Vector3(-(p2Upper + p2Lower), 0.0f, -10.0f);

        LeftArm.ShoulderRotation = new Vector3(-65.0f, 20.0f, 0.0f);
        LeftArm.LowerArmRotation = new Vector3(-85.0f, 0.0f, 0.0f);

        RightArm.ShoulderRotation = new Vector3(-60.0f, -20.0f, 0.0f);
        RightArm.LowerArmRotation = new Vector3(-90.0f, 0.0f, 0.0f);
    }

    public virtual void AnimateSidestep(bool isLeft)
    {
        float sideFactor = isLeft ? 1.0f : -1.0f;

        Torso.AbdomenRotation = new Vector3(15.0f, -10.0f * sideFactor, -15.0f * sideFactor);
        Torso.ChestRotation = new Vector3(10.0f, -5.0f * sideFactor, -10.0f * sideFactor);
        Head.Rotation = new Vector3(-15.0f, 10.0f * sideFactor, 5.0f * sideFactor);

        if (isLeft)
        {
            LeftLeg.UpperLegRotation = new Vector3(-25.0f, 0.0f, -25.0f);
            LeftLeg.LowerLegRotation = new Vector3(45.0f, 0.0f, 0.0f);
            LeftLeg.FootRotation = new Vector3(-20.0f, 0.0f, 15.0f);

            RightLeg.UpperLegRotation = new Vector3(10.0f, 0.0f, 15.0f);
            RightLeg.LowerLegRotation = new Vector3(30.0f, 0.0f, 0.0f);
            RightLeg.FootRotation = new Vector3(-40.0f, 0.0f, -10.0f);
        }
        else
        {
            LeftLeg.UpperLegRotation = new Vector3(10.0f, 0.0f, -15.0f);
            LeftLeg.LowerLegRotation = new Vector3(30.0f, 0.0f, 0.0f);
            LeftLeg.FootRotation = new Vector3(-40.0f, 0.0f, 10.0f);

            RightLeg.UpperLegRotation = new Vector3(-25.0f, 0.0f, 25.0f);
            RightLeg.LowerLegRotation = new Vector3(45.0f, 0.0f, 0.0f);
            RightLeg.FootRotation = new Vector3(-20.0f, 0.0f, -15.0f);
        }

        LeftArm.ShoulderRotation = new Vector3(-60.0f, 20.0f, 0.0f);
        LeftArm.LowerArmRotation = new Vector3(-80.0f, 0.0f, 0.0f);

        RightArm.ShoulderRotation = new Vector3(-50.0f, -20.0f, 0.0f);
        RightArm.LowerArmRotation = new Vector3(-85.0f, 0.0f, 0.0f);
    }

    public virtual void AnimateFrontDash()
    {
        Torso.AbdomenRotation = new Vector3(28.0f, 10.0f, 0.0f);
        Torso.ChestRotation = new Vector3(15.0f, 5.0f, 0.0f);
        Head.Rotation = new Vector3(-22.0f, 0.0f, 0.0f);

        LeftLeg.UpperLegRotation = new Vector3(-45.0f, 0.0f, -5.0f);
        LeftLeg.LowerLegRotation = new Vector3(65.0f, 0.0f, 0.0f);
        LeftLeg.FootRotation = new Vector3(-20.0f, 0.0f, 5.0f);

        RightLeg.UpperLegRotation = new Vector3(20.0f, 0.0f, 5.0f);
        RightLeg.LowerLegRotation = new Vector3(25.0f, 0.0f, 0.0f);
        RightLeg.FootRotation = new Vector3(-45.0f, 0.0f, -5.0f);

        LeftArm.ShoulderRotation = new Vector3(-70.0f, 25.0f, 0.0f);
        LeftArm.LowerArmRotation = new Vector3(-90.0f, 0.0f, 0.0f);

        RightArm.ShoulderRotation = new Vector3(-60.0f, -25.0f, 0.0f);
        RightArm.LowerArmRotation = new Vector3(-95.0f, 0.0f, 0.0f);
    }

    public virtual void AnimateBackDash()
    {
        Torso.AbdomenRotation = new Vector3(-15.0f, -5.0f, 0.0f);
        Torso.ChestRotation = new Vector3(-10.0f, -5.0f, 0.0f);
        Head.Rotation = new Vector3(10.0f, 0.0f, 0.0f);

        LeftLeg.UpperLegRotation = new Vector3(25.0f, 0.0f, -5.0f);
        LeftLeg.LowerLegRotation = new Vector3(30.0f, 0.0f, 0.0f);
        LeftLeg.FootRotation = new Vector3(-55.0f, 0.0f, 5.0f);

        RightLeg.UpperLegRotation = new Vector3(-35.0f, 0.0f, 5.0f);
        RightLeg.LowerLegRotation = new Vector3(55.0f, 0.0f, 0.0f);
        RightLeg.FootRotation = new Vector3(-20.0f, 0.0f, -5.0f);

        LeftArm.ShoulderRotation = new Vector3(-40.0f, 15.0f, 0.0f);
        LeftArm.LowerArmRotation = new Vector3(-65.0f, 0.0f, 0.0f);

        RightArm.ShoulderRotation = new Vector3(-35.0f, -15.0f, 0.0f);
        RightArm.LowerArmRotation = new Vector3(-70.0f, 0.0f, 0.0f);
    }

    public virtual void AnimateDefending()
    {
        Torso.AbdomenRotation = new Vector3(8.0f, 5.0f, 0.0f);
        Torso.ChestRotation = new Vector3(5.0f, 5.0f, 0.0f);
        Head.Rotation = new Vector3(-10.0f, 0.0f, 0.0f);

        LeftArm.ShoulderRotation = new Vector3(-80.0f, 40.0f, -15.0f);
        LeftArm.LowerArmRotation = new Vector3(-110.0f, 0.0f, 0.0f);

        RightArm.ShoulderRotation = new Vector3(-80.0f, -40.0f, 15.0f);
        RightArm.LowerArmRotation = new Vector3(-110.0f, 0.0f, 0.0f);

        LeftLeg.UpperLegRotation = new Vector3(-20.0f, 0.0f, -5.0f);
        LeftLeg.LowerLegRotation = new Vector3(35.0f, 0.0f, 0.0f);
        LeftLeg.FootRotation = new Vector3(-15.0f, 0.0f, 5.0f);

        RightLeg.UpperLegRotation = new Vector3(10.0f, 0.0f, 5.0f);
        RightLeg.LowerLegRotation = new Vector3(20.0f, 0.0f, 0.0f);
        RightLeg.FootRotation = new Vector3(-30.0f, 0.0f, -5.0f);
    }

    public virtual void AnimateHit()
    {
        Torso.AbdomenRotation = new Vector3(-25.0f, -15.0f, 5.0f);
        Torso.ChestRotation = new Vector3(-20.0f, -10.0f, 5.0f);
        Head.Rotation = new Vector3(25.0f, -20.0f, -10.0f);

        LeftArm.ShoulderRotation = new Vector3(-20.0f, 45.0f, 30.0f);
        LeftArm.LowerArmRotation = new Vector3(-40.0f, 0.0f, 0.0f);

        RightArm.ShoulderRotation = new Vector3(-15.0f, -40.0f, -30.0f);
        RightArm.LowerArmRotation = new Vector3(-45.0f, 0.0f, 0.0f);

        LeftLeg.UpperLegRotation = new Vector3(15.0f, 0.0f, -10.0f);
        LeftLeg.LowerLegRotation = new Vector3(25.0f, 0.0f, 0.0f);
        LeftLeg.FootRotation = new Vector3(-40.0f, 0.0f, 10.0f);

        RightLeg.UpperLegRotation = new Vector3(-30.0f, 0.0f, 10.0f);
        RightLeg.LowerLegRotation = new Vector3(45.0f, 0.0f, 0.0f);
        RightLeg.FootRotation = new Vector3(-15.0f, 0.0f, -10.0f);
    }

    public virtual void AnimateKnockedOut()
    {
        Torso.AbdomenRotation = new Vector3(-85.0f, 0.0f, 0.0f);
        Torso.ChestRotation = new Vector3(-5.0f, 0.0f, 0.0f);
        Head.Rotation = new Vector3(20.0f, 15.0f, 0.0f);

        LeftArm.ShoulderRotation = new Vector3(15.0f, 30.0f, 20.0f);
        LeftArm.LowerArmRotation = new Vector3(-20.0f, 0.0f, 0.0f);

        RightArm.ShoulderRotation = new Vector3(10.0f, -35.0f, -25.0f);
        RightArm.LowerArmRotation = new Vector3(-15.0f, 0.0f, 0.0f);

        LeftLeg.UpperLegRotation = new Vector3(5.0f, 0.0f, -15.0f);
        LeftLeg.LowerLegRotation = new Vector3(10.0f, 0.0f, 0.0f);
        LeftLeg.FootRotation = new Vector3(-15.0f, 0.0f, 15.0f);

        RightLeg.UpperLegRotation = new Vector3(10.0f, 0.0f, 15.0f);
        RightLeg.LowerLegRotation = new Vector3(5.0f, 0.0f, 0.0f);
        RightLeg.FootRotation = new Vector3(-15.0f, 0.0f, -15.0f);
    }

    public void ResetPose()
    {
        LeftArm.ShoulderRotation = Vector3.Zero;
        LeftArm.LowerArmRotation = Vector3.Zero;
        RightArm.ShoulderRotation = Vector3.Zero;
        RightArm.LowerArmRotation = Vector3.Zero;

        Torso.AbdomenRotation = Vector3.Zero;
        Torso.ChestRotation = Vector3.Zero;

        LeftLeg.UpperLegRotation = Vector3.Zero;
        LeftLeg.LowerLegRotation = Vector3.Zero;
        RightLeg.UpperLegRotation = Vector3.Zero;
        RightLeg.LowerLegRotation = Vector3.Zero;
    }

    protected float Lerp(float start, float end, float amount)
    {
        return start + (end - start) * Math.Clamp(amount, 0.0f, 1.0f);
    }

    public void Render(MyShader shader, Matrix4x4 playerBaseMatrix, Matrix4x4 view, Matrix4x4 projection)
    {
        Matrix4x4 chestMatrix = Torso.Render(shader, playerBaseMatrix, view, projection);

        Head.Render(shader, chestMatrix, view, projection);

        LeftArm.Render(shader, chestMatrix, view, projection);
        RightArm.Render(shader, chestMatrix, view, projection);

        LeftLeg.Render(shader, playerBaseMatrix, view, projection);
        RightLeg.Render(shader, playerBaseMatrix, view, projection);
    }

    public void Dispose()
    {
        Torso.Dispose();
        Head.Dispose();
        LeftArm.Dispose();
        RightArm.Dispose();
        LeftLeg.Dispose();
        RightLeg.Dispose();
    }
}