using Silk.NET.OpenGL;
using System.Numerics;

namespace CloseQuarter.Client.Models;

public class BoxerModel : Model3D
{
    public BoxerModel(GL gl) : base(gl) { }

    public void AnimateLeftJab(int currentFrame, int startupFrames, int activeFrames, int recoveryFrames)
    {
        int totalFrames = startupFrames + activeFrames + recoveryFrames;
        float progress = 0.0f;

        if (currentFrame <= startupFrames)
        {
            progress = (float)currentFrame / startupFrames;

            LeftArm.ShoulderRotation = new Vector3(Lerp(-55f, -90f, progress), Lerp(20f, 10f, progress), 0f);
            LeftArm.LowerArmRotation = new Vector3(Lerp(-75f, -5f, progress), 0f, 0f);

            RightArm.ShoulderRotation = new Vector3(-50f, -25f, 0f);
            RightArm.LowerArmRotation = new Vector3(-90f, 0f, 0f);

            Torso.AbdomenRotation = new Vector3(12f, Lerp(15f, 30f, progress), 0f);
            Torso.ChestRotation = new Vector3(10f, Lerp(10f, 20f, progress), 0f);
            Head.Rotation = new Vector3(-15f, Lerp(-5f, -15f, progress), 0f);
        }
        else if (currentFrame <= startupFrames + activeFrames)
        {
            LeftArm.ShoulderRotation = new Vector3(-90f, 10f, 0f);
            LeftArm.LowerArmRotation = new Vector3(-5f, 0f, 0f);

            RightArm.ShoulderRotation = new Vector3(-50f, -25f, 0f);
            RightArm.LowerArmRotation = new Vector3(-90f, 0f, 0f);

            Torso.AbdomenRotation = new Vector3(12f, 30f, 0f);
            Torso.ChestRotation = new Vector3(10f, 20f, 0f);
            Head.Rotation = new Vector3(-15f, -15f, 0f);
        }
        else if (currentFrame <= totalFrames)
        {
            progress = 1.0f - ((float)(currentFrame - startupFrames - activeFrames) / recoveryFrames);

            LeftArm.ShoulderRotation = new Vector3(Lerp(-55f, -90f, progress), Lerp(20f, 10f, progress), 0f);
            LeftArm.LowerArmRotation = new Vector3(Lerp(-75f, -5f, progress), 0f, 0f);

            RightArm.ShoulderRotation = new Vector3(-50f, -25f, 0f);
            RightArm.LowerArmRotation = new Vector3(-90f, 0f, 0f);

            Torso.AbdomenRotation = new Vector3(12f, Lerp(15f, 30f, progress), 0f);
            Torso.ChestRotation = new Vector3(10f, Lerp(10f, 20f, progress), 0f);
            Head.Rotation = new Vector3(-15f, Lerp(-5f, -15f, progress), 0f);
        }
    }

    public void AnimateRightJab(int currentFrame, int startupFrames, int activeFrames, int recoveryFrames)
    {
        int totalFrames = startupFrames + activeFrames + recoveryFrames;
        float progress = 0.0f;

        if (currentFrame <= startupFrames)
        {
            progress = (float)currentFrame / startupFrames;

            RightArm.ShoulderRotation = new Vector3(Lerp(-45f, -85f, progress), Lerp(-25f, -15f, progress), 0f);
            RightArm.LowerArmRotation = new Vector3(Lerp(-90f, -10f, progress), 0f, 0f);

            LeftArm.ShoulderRotation = new Vector3(-55f, 20f, 0f);
            LeftArm.LowerArmRotation = new Vector3(-75f, 0f, 0f);

            Torso.AbdomenRotation = new Vector3(12f, Lerp(15f, -15f, progress), 0f);
            Torso.ChestRotation = new Vector3(10f, Lerp(10f, -10f, progress), 0f);
        }
        else if (currentFrame <= startupFrames + activeFrames)
        {
            RightArm.ShoulderRotation = new Vector3(-85f, -15f, 0f);
            RightArm.LowerArmRotation = new Vector3(-10f, 0f, 0f);

            Torso.AbdomenRotation = new Vector3(12f, -15f, 0f);
            Torso.ChestRotation = new Vector3(10f, -10f, 0f);
        }
        else if (currentFrame <= totalFrames)
        {
            progress = 1.0f - ((float)(currentFrame - startupFrames - activeFrames) / recoveryFrames);

            RightArm.ShoulderRotation = new Vector3(Lerp(-45f, -85f, progress), Lerp(-25f, -15f, progress), 0f);
            RightArm.LowerArmRotation = new Vector3(Lerp(-90f, -10f, progress), 0f, 0f);

            Torso.AbdomenRotation = new Vector3(12f, Lerp(15f, -15f, progress), 0f);
            Torso.ChestRotation = new Vector3(10f, Lerp(10f, -10f, progress), 0f);
        }
    }

    public void AnimateLeftHook(int currentFrame, int startupFrames, int activeFrames, int recoveryFrames)
    {
        int totalFrames = startupFrames + activeFrames + recoveryFrames;
        float progress = 0.0f;

        if (currentFrame <= startupFrames)
        {
            progress = (float)currentFrame / startupFrames;

            LeftArm.ShoulderRotation = new Vector3(Lerp(-55f, -80f, progress), Lerp(20f, 45f, progress), Lerp(0f, -20f, progress));
            LeftArm.LowerArmRotation = new Vector3(Lerp(-75f, -40f, progress), 0f, 0f);

            Torso.AbdomenRotation = new Vector3(15f, Lerp(15f, 40f, progress), 0f);
            Torso.ChestRotation = new Vector3(12f, Lerp(10f, 30f, progress), 0f);
        }
        else if (currentFrame <= startupFrames + activeFrames)
        {
            LeftArm.ShoulderRotation = new Vector3(-80f, 45f, -20f);
            LeftArm.LowerArmRotation = new Vector3(-40f, 0f, 0f);

            Torso.AbdomenRotation = new Vector3(15f, 40f, 0f);
            Torso.ChestRotation = new Vector3(12f, 30f, 0f);
        }
        else if (currentFrame <= totalFrames)
        {
            progress = 1.0f - ((float)(currentFrame - startupFrames - activeFrames) / recoveryFrames);

            LeftArm.ShoulderRotation = new Vector3(Lerp(-55f, -80f, progress), Lerp(20f, 45f, progress), Lerp(0f, -20f, progress));
            LeftArm.LowerArmRotation = new Vector3(Lerp(-75f, -40f, progress), 0f, 0f);

            Torso.AbdomenRotation = new Vector3(15f, Lerp(15f, 40f, progress), 0f);
            Torso.ChestRotation = new Vector3(12f, Lerp(10f, 30f, progress), 0f);
        }
    }

    public void AnimateRightStraight(int currentFrame, int startupFrames, int activeFrames, int recoveryFrames)
    {
        int totalFrames = startupFrames + activeFrames + recoveryFrames;
        float progress = 0.0f;

        if (currentFrame <= startupFrames)
        {
            progress = (float)currentFrame / startupFrames;

            RightArm.ShoulderRotation = new Vector3(Lerp(-50f, -95f, progress), Lerp(-25f, -10f, progress), 0f);
            RightArm.LowerArmRotation = new Vector3(Lerp(-90f, -5f, progress), 0f, 0f);

            LeftArm.ShoulderRotation = new Vector3(-55f, 20f, 0f);
            LeftArm.LowerArmRotation = new Vector3(-85f, 0f, 0f);

            Torso.AbdomenRotation = new Vector3(12f, Lerp(15f, -25f, progress), 0f);
            Torso.ChestRotation = new Vector3(10f, Lerp(10f, -20f, progress), 0f);
            Head.Rotation = new Vector3(-15f, Lerp(-5f, 10f, progress), 0f);
        }
        else if (currentFrame <= startupFrames + activeFrames)
        {
            RightArm.ShoulderRotation = new Vector3(-95f, -10f, 0f);
            RightArm.LowerArmRotation = new Vector3(-5f, 0f, 0f);

            Torso.AbdomenRotation = new Vector3(12f, -25f, 0f);
            Torso.ChestRotation = new Vector3(10f, -20f, 0f);
        }
        else if (currentFrame <= totalFrames)
        {
            progress = 1.0f - ((float)(currentFrame - startupFrames - activeFrames) / recoveryFrames);

            RightArm.ShoulderRotation = new Vector3(Lerp(-50f, -95f, progress), Lerp(-25f, -10f, progress), 0f);
            RightArm.LowerArmRotation = new Vector3(Lerp(-90f, -5f, progress), 0f, 0f);

            LeftArm.ShoulderRotation = new Vector3(-55f, 20f, 0f);
            LeftArm.LowerArmRotation = new Vector3(-85f, 0f, 0f);

            Torso.AbdomenRotation = new Vector3(12f, Lerp(15f, -25f, progress), 0f);
            Torso.ChestRotation = new Vector3(10f, Lerp(10f, -20f, progress), 0f);
        }
    }
}