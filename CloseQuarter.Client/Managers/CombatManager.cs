using CloseQuarter.Client.Models;

namespace CloseQuarter.Client.Managers;

public enum MatchState
{
    Starting,
    Running,
    Paused,
    RoundOver,
    P1Wins,
    P2Wins
}

public class CombatManager
{
    public const float MaxHealth = 100f;
    public const int RoundsToWin = 2;

    public float P1Health { get; set; } = MaxHealth;
    public float P2Health { get; set; } = MaxHealth;
    public float MatchTimer { get; set; } = 90f;

    public int P1RoundsWon { get; private set; } = 0;
    public int P2RoundsWon { get; private set; } = 0;
    public int CurrentRound { get; private set; } = 1;

    public MatchState CurrentState { get; private set; } = MatchState.Starting;
    public float CountdownTimer { get; private set; } = 4.0f;

    private float _roundOverTimer = 0.0f;
    private const float RoundOverDuration = 3.5f;

    public bool IsGameOver => CurrentState == MatchState.P1Wins || CurrentState == MatchState.P2Wins;

    private readonly string _soundEffectPath = "CloseQuarter.Client/Audio/boom.wav";


    public bool IsMatchTransitionReady { get; private set; } = false;


    public void ResetFullMatch()
    {
        P1RoundsWon = 0;
        P2RoundsWon = 0;
        CurrentRound = 1;
        ResetRound();
    }

    public void ResetRound()
    {
        P1Health = MaxHealth;
        P2Health = MaxHealth;
        MatchTimer = 90f;
        CountdownTimer = 4.0f;
        _roundOverTimer = 0.0f;
        CurrentState = MatchState.Starting;
    }

    public void TogglePause()
    {
        if (CurrentState == MatchState.Running) CurrentState = MatchState.Paused;
        else if (CurrentState == MatchState.Paused) CurrentState = MatchState.Running;
    }

    public void UpdateMatch(float deltaTime, Player p1, Player p2)
    {
        if (CurrentState == MatchState.Starting)
        {
            CountdownTimer -= deltaTime;
            if (CountdownTimer <= 0f) CurrentState = MatchState.Running;
            return;
        }

        if (CurrentState == MatchState.P1Wins || CurrentState == MatchState.P2Wins)
        {
            _roundOverTimer += deltaTime;
            if (_roundOverTimer >= RoundOverDuration)
            {
                IsMatchTransitionReady = true;
            }
            return;
        }

        if (CurrentState == MatchState.RoundOver)
        {
            _roundOverTimer += deltaTime;
            if (_roundOverTimer >= RoundOverDuration)
            {
                if (P1RoundsWon >= RoundsToWin)
                {
                    CurrentState = MatchState.P1Wins;
                    _roundOverTimer = 0.0f;
                }
                else if (P2RoundsWon >= RoundsToWin)
                {
                    CurrentState = MatchState.P2Wins;
                    _roundOverTimer = 0.0f;
                }
                else
                {
                    CurrentRound++;
                    ResetRound();
                    p1?.ResetToDefaultState(new System.Numerics.Vector3(-2.5f, 0, 0), new System.Numerics.Vector3(0, 90, 0));
                    p2?.ResetToDefaultState(new System.Numerics.Vector3(2.5f, 0, 0), new System.Numerics.Vector3(0, -90, 0));
                }
            }
            return;
        }

        if (CurrentState != MatchState.Running) return;

        MatchTimer -= deltaTime;
        if (MatchTimer <= 0f)
        {
            MatchTimer = 0f;
            DetermineRoundWinnerByTime(p1, p2);
            return;
        }

        if (P2Health <= 0f)
        {
            P2Health = 0f;
            AwardRoundWin(winner: p1, loser: p2, isP1: true);
        }
        else if (P1Health <= 0f)
        {
            P1Health = 0f;
            AwardRoundWin(winner: p2, loser: p1, isP1: false);
        }
    }

    private void AwardRoundWin(Player winner, Player loser, bool isP1)
    {
        if (isP1) P1RoundsWon++;
        else P2RoundsWon++;

        CurrentState = MatchState.RoundOver;
        _roundOverTimer = 0.0f;

        if (winner != null) winner.CurrentState = PlayerState.Victory;
        if (loser != null) loser.CurrentState = PlayerState.DownedFaceUp;
    }

    private void DetermineRoundWinnerByTime(Player p1, Player p2)
    {
        if (P1Health >= P2Health) AwardRoundWin(p1, p2, isP1: true);
        else AwardRoundWin(p2, p1, isP1: false);
    }

    public void ProcessFixedUpdate(Player p1, Player p2, float fixedDeltaTime)
    {
        if (p1 == null || p2 == null) return;

        if (CurrentState == MatchState.Starting || CurrentState == MatchState.RoundOver)
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

                if (isP1Attacker) P2Health = MathF.Max(0f, P2Health - damage);
                else P1Health = MathF.Max(0f, P1Health - damage);

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