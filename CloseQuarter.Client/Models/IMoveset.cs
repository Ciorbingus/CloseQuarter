using System.Numerics;

using CloseQuarter.Client.Managers;

namespace CloseQuarter.Client.Models;

public interface IMoveset
{
    string CharacterName { get; }

    bool ProcessAttackInput(Player player, InputHistory history, bool punchTriggered, bool rightPunchTriggered, float currentTime);
    void UpdateAttackAnimation(Player player, int currentFrame, int startup, int active, int recovery);
    (Vector3 position, float radius) GetPunchHitbox(Player player);
    float GetCurrentAttackDamage(AttackType attackType);
}