namespace CloseQuarter.Shared.DTOs;

public enum RoomType
{
    Matchmaking,
    Private
}

public class CreateRoomRequestDTO
{
    public RoomType Type { get; set; }
}

public class JoinRoomRequestDTO
{
    public string RoomCode { get; set; } = string.Empty;
}

public class RoomPlayerStateDTO
{
    public string Username { get; set; } = string.Empty;
    public string SelectedCharacterId { get; set; } = "bobi";
    public bool IsReady { get; set; }
}

public class RoomStateDTO
{
    public string RoomCode { get; set; } = string.Empty;
    public RoomType Type { get; set; }
    public RoomPlayerStateDTO? Player1 { get; set; }
    public RoomPlayerStateDTO? Player2 { get; set; }
    public string SelectedMapId { get; set; } = "test_map";
}

public class SelectCharacterRequestDTO
{
    public string CharacterId { get; set; } = string.Empty;
}

public class SetReadyRequestDTO
{
    public bool IsReady { get; set; }
}