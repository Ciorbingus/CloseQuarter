using Silk.NET.OpenGL;
using System.Numerics;

namespace CloseQuarter.Client.Models;

public class MilitaryModel : Model3D
{
    public MilitaryModel(GL gl) : base(gl) { }

    public override void AnimateIdle(float idleTime)
    {
        float pulse = MathF.Sin(idleTime * 8.0f);
        float breath = pulse * 1.5f;

        Torso.AbdomenRotation = new Vector3(8.0f + breath, 5.0f, 0.0f);
        Torso.ChestRotation = new Vector3(10.0f, 5.0f, 0.0f);

        Head.Rotation = new Vector3(-10.0f - breath, 0.0f, 0.0f);

        LeftArm.ShoulderRotation = new Vector3(-75.0f + breath, 30.0f, -10.0f);
        LeftArm.LowerArmRotation = new Vector3(-105.0f, 0.0f, 0.0f);

        RightArm.ShoulderRotation = new Vector3(-70.0f, -30.0f, 10.0f);
        RightArm.LowerArmRotation = new Vector3(-110.0f + breath, 0.0f, 0.0f);

        LeftLeg.UpperLegRotation = new Vector3(-20.0f, 0.0f, -5.0f);
        LeftLeg.LowerLegRotation = new Vector3(35.0f - breath, 0.0f, 0.0f);

        RightLeg.UpperLegRotation = new Vector3(10.0f, 0.0f, 5.0f);
        RightLeg.LowerLegRotation = new Vector3(20.0f + breath, 0.0f, 0.0f);
    }

    public void AnimateElbowStrike(int currentFrame, int startupFrames, int activeFrames, int recoveryFrames)
    {
        int totalFrames = startupFrames + activeFrames + recoveryFrames;
        float progress = (float)currentFrame / totalFrames;

        Torso.ChestRotation = new Vector3(10f, Lerp(0f, 60f, progress), 0f);
        RightArm.ShoulderRotation = new Vector3(-80f, -50f, 30f);
        RightArm.LowerArmRotation = new Vector3(-130f, 0f, 0f); 
    }

    public void AnimateKneeStrike(int currentFrame, int startupFrames, int activeFrames, int recoveryFrames)
    {
        int totalFrames = startupFrames + activeFrames + recoveryFrames;
        float progress = (float)currentFrame / totalFrames;

        Torso.AbdomenRotation = new Vector3(-15f, 0f, 0f);
        RightLeg.UpperLegRotation = new Vector3(Lerp(0f, -110f, progress), 0f, 0f);
        RightLeg.LowerLegRotation = new Vector3(Lerp(0f, 130f, progress), 0f, 0f);
    }
}