using System.Numerics;

using CloseQuarter.Client.Managers;

namespace CloseQuarter.Client.Models;

public class BoxerMoveset : IMoveset
{
    public string CharacterName => "Bobi";

    public bool ProcessAttackInput(Player player, InputHistory history, bool punchTriggered, bool rightPunchTriggered, float currentTime)
    {
        if (rightPunchTriggered)
        {
            InputFlags[] combo3 = new[] { InputFlags.Punch, InputFlags.Punch, InputFlags.RightPunch };

            if (history.CheckSequence(combo3, maxWindow: 0.7f, currentTime: currentTime))
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
            InputFlags[] comboLP1LP2 = new[] { InputFlags.Punch, InputFlags.Punch };

            if (history.CheckSequence(comboLP1LP2, maxWindow: 0.5f, currentTime: currentTime))
            {
                player.AttackLP2();
                return true;
            }

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
        if (player.FighterModel is BoxerModel boxerModel)
        {
            switch (player.CurrentAttackType)
            {
                case AttackType.LP1:
                    boxerModel.AnimateLeftJab(currentFrame, startup, active, recovery);
                    break;
                case AttackType.RP1:
                    boxerModel.AnimateRightJab(currentFrame, startup, active, recovery);
                    break;
                case AttackType.LP2:
                    boxerModel.AnimateLeftHook(currentFrame, startup, active, recovery);
                    break;
                case AttackType.RP2:
                    boxerModel.AnimateRightStraight(currentFrame, startup, active, recovery);
                    break;
            }
        }
    }

    public (Vector3 position, float radius) GetPunchHitbox(Player player)
    {
        float yawInRadians = player.Rotation.Y * (MathF.PI / 180.0f);
        Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1), Matrix4x4.CreateRotationY(yawInRadians));

        float yPos = 1.20f;
        float radius = 0.17f;

        switch (player.CurrentAttackType)
        {
            case AttackType.LP1: yPos = 1.20f; radius = 0.17f; break;
            case AttackType.RP1: yPos = 1.20f; radius = 0.17f; break;
            case AttackType.LP2: yPos = 1.35f; radius = 0.20f; break;
            case AttackType.RP2: yPos = 1.55f; radius = 0.23f; break;
        }

        Vector3 punchPosition = player.Position + (forward * 0.8f) + new Vector3(0, yPos, 0);
        return (punchPosition, radius);
    }

    public float GetCurrentAttackDamage(AttackType attackType)
    {
        return attackType switch
        {
            AttackType.LP1 => 4.0f,
            AttackType.RP1 => 5.0f,
            AttackType.LP2 => 6.0f,
            AttackType.RP2 => 8.0f,
            _ => 5.0f
        };
    }
}