using CloseQuarter.Client.Models;

namespace CloseQuarter.Client.Managers;

public enum MatchState
{
    Starting,
    Running,
    Paused,
    P1Wins,
    P2Wins,
    DoubleKO
}

public class CombatManager
{
    public const float MaxHealth = 100f;

    public float P1Health { get; set; } = MaxHealth;
    public float P2Health { get; set; } = MaxHealth;
    public float MatchTimer { get; set; } = 90f;

    public MatchState CurrentState { get; private set; } = MatchState.Starting;
    public float CountdownTimer { get; private set; } = 4.0f;

    public bool IsGameOver => CurrentState == MatchState.P1Wins || CurrentState == MatchState.P2Wins;

    private readonly string _soundEffectPath = "CloseQuarter.Client/Audio/boom.wav";

    public void ResetMatch()
    {
        P1Health = MaxHealth;
        P2Health = MaxHealth;
        MatchTimer = 90f;
        CountdownTimer = 4.0f;
        CurrentState = MatchState.Starting;
    }

    public void TogglePause()
    {
        if (CurrentState == MatchState.Running)
        {
            CurrentState = MatchState.Paused;
        }
        else if (CurrentState == MatchState.Paused)
        {
            CurrentState = MatchState.Running;
        }
    }

    public void UpdateMatch(float deltaTime)
    {
        if (CurrentState == MatchState.Starting)
        {
            CountdownTimer -= deltaTime;
            if (CountdownTimer <= 0f)
            {
                CurrentState = MatchState.Running;
            }
            return;
        }

        if (CurrentState != MatchState.Running) return;

        MatchTimer -= deltaTime;
        if (MatchTimer <= 0f)
        {
            MatchTimer = 0f;
            DetermineWinnerByTime();
            return;
        }

        if (P2Health <= 0f)
        {
            P2Health = 0f;
            CurrentState = MatchState.P1Wins;
        }
        else if (P1Health <= 0f)
        {
            P1Health = 0f;
            CurrentState = MatchState.P2Wins; 
        }
    }

    private void DetermineWinnerByTime()
    {
        if (P1Health > P2Health) CurrentState = MatchState.P1Wins;
        else if (P2Health > P1Health) CurrentState = MatchState.P2Wins;
        else CurrentState = MatchState.P1Wins;
    }

    public void ProcessFixedUpdate(Player p1, Player p2, float fixedDeltaTime)
    {
        if (p1 == null || p2 == null) return;

        if (CurrentState == MatchState.Starting)
        {
            UpdatePlayerPhysicsAndAnimations(p1, fixedDeltaTime);
            UpdatePlayerPhysicsAndAnimations(p2, fixedDeltaTime);

            p1.SetOpponentPosition(p2.Position);
            p2.SetOpponentPosition(p1.Position);
            return;
        }

        if (CurrentState == MatchState.Running)
        {
            UpdatePlayerPhysicsAndAnimations(p1, fixedDeltaTime);
            UpdatePlayerPhysicsAndAnimations(p2, fixedDeltaTime);

            ProcessPlayerAttack(p1, p2, isP1Attacker: true);
            ProcessPlayerAttack(p2, p1, isP1Attacker: false);

            PhysicsManager.ResolvePlayerCollision(p1, p2);

            p1.SetOpponentPosition(p2.Position);
            p2.SetOpponentPosition(p1.Position);
        }
    }

    private void UpdatePlayerPhysicsAndAnimations(Player player, float deltaTime)
    {
        player.UpdateAnimations(deltaTime);
        player.UpdateDash(deltaTime);
        player.UpdatePhysicsAndKnockback(deltaTime);
        PhysicsManager.ApplyGravityAndFloor(player, deltaTime);
        PhysicsManager.KeepPlayerInRing(player);
    }

    private void ProcessPlayerAttack(Player attacker, Player defender, bool isP1Attacker)
    {
        if (attacker.IsAttackInActiveFrames() && !attacker.HasHitCurrentAttack)
        {
            var (punchPos, punchRadius) = attacker.GetPunchHitbox();

            if (PhysicsManager.CheckPunchHit(punchPos, punchRadius, defender))
            {
                float damage = attacker.GetAttackDamage();

                if (isP1Attacker)
                {
                    P2Health = MathF.Max(0f, P2Health - damage);
                }
                else
                {
                    P1Health = MathF.Max(0f, P1Health - damage);
                }

                attacker.HasHitCurrentAttack = true;

                if (attacker.CurrentAttackType == AttackType.RP2)
                {
                    PhysicsManager.ApplyDirectionalKnockdown(attacker, defender, knockbackForce: 7.0f);
                }
                else
                {
                    defender.CurrentState = PlayerState.Hit;
                }

                AudioManager.PlaySFX(_soundEffectPath, 0.4f);
            }
        }
    }
}