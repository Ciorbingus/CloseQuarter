using System.Numerics;

using CloseQuarter.Client.Managers;

namespace CloseQuarter.Client.Models;

public class MilitaryMoveset : IMoveset
{
    public string CharacterName => "Toma";

    public bool ProcessAttackInput(Player player, InputHistory history, bool punchTriggered, bool rightPunchTriggered, float currentTime)
    {
        if (rightPunchTriggered)
        {
            InputFlags[] kravCombo = new[] { InputFlags.RightPunch, InputFlags.Punch, InputFlags.RightPunch };

            if (history.CheckSequence(kravCombo, maxWindow: 0.8f, currentTime: currentTime))
            {
                player.AttackRP2();
                return true;
            }

            if (!player.IsAttacking)
            {
                player.AttackRP1();
                return true;
            }
        }

        if (punchTriggered)
        {
            if (!player.IsAttacking)
            {
                player.AttackLP1();
                return true;
            }
        }

        return false;
    }

    public void UpdateAttackAnimation(Player player, int currentFrame, int startup, int active, int recovery)
    {
        if (player.FighterModel is MilitaryModel militaryModel)
        {
            switch (player.CurrentAttackType)
            {
                case AttackType.LP1:
                    militaryModel.AnimateElbowStrike(currentFrame, startup, active, recovery);
                    break;
                case AttackType.RP1:
                    militaryModel.AnimateElbowStrike(currentFrame, startup, active, recovery);
                    break;
                case AttackType.LP2:
                    militaryModel.AnimateKneeStrike(currentFrame, startup, active, recovery);
                    break;
                case AttackType.RP2:
                    militaryModel.AnimateKneeStrike(currentFrame, startup, active, recovery);
                    break;
            }
        }
        else if (player.FighterModel is BoxerModel boxerModel)
        {
            switch (player.CurrentAttackType)
            {
                case AttackType.LP1: boxerModel.AnimateLeftJab(currentFrame, startup, active, recovery); break;
                case AttackType.RP1: boxerModel.AnimateRightJab(currentFrame, startup, active, recovery); break;
                case AttackType.LP2: boxerModel.AnimateLeftHook(currentFrame, startup, active, recovery); break;
                case AttackType.RP2: boxerModel.AnimateRightStraight(currentFrame, startup, active, recovery); break;
            }
        }
    }

    public (Vector3 position, float radius) GetPunchHitbox(Player player)
    {
        float yawInRadians = player.Rotation.Y * (MathF.PI / 180.0f);
        Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1), Matrix4x4.CreateRotationY(yawInRadians));

        float yPos = 1.30f;
        float radius = 0.22f;

        if (player.CurrentAttackType == AttackType.RP2)
        {
            yPos = 1.10f;
            radius = 0.28f;
        }

        Vector3 punchPosition = player.Position + (forward * 0.65f) + new Vector3(0, yPos, 0);
        return (punchPosition, radius);
    }

    public float GetCurrentAttackDamage(AttackType attackType)
    {
        return attackType switch
        {
            AttackType.LP1 => 6.0f,
            AttackType.RP1 => 5.0f,
            AttackType.LP2 => 8.0f,
            AttackType.RP2 => 10.0f,
            _ => 5.0f
        };
    }
}