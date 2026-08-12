namespace CloseQuarter.Shared.DTOs;

public class PlayerInputPacketDTO
{
    public uint TickNumber { get; set; }
    public ushort InputFlags { get; set; } 
}

public class PlayerNetStateDTO
{
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float Health { get; set; }
    public byte CurrentState { get; set; } 
}

public class GameStateSnapshotDTO
{
    public uint TickNumber { get; set; }
    public byte MatchState { get; set; } 
    public float MatchTimer { get; set; }
    public byte CountdownTimer { get; set; }
    public byte P1RoundsWon { get; set; }
    public byte P2RoundsWon { get; set; }

    public PlayerNetStateDTO Player1 { get; set; } = new();
    public PlayerNetStateDTO Player2 { get; set; } = new();
}