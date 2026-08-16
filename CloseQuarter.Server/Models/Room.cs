using System.Net.Sockets;
using CloseQuarter.Shared.DTOs;

namespace CloseQuarter.Server.Rooms;

public class Room
{
    public string RoomCode { get; }
    public RoomType Type { get; }
    public RoomPlayer? Player1 { get; private set; }
    public RoomPlayer? Player2 { get; private set; }
    public string SelectedMapId { get; set; } = "test_map";
    public bool IsInMatch { get; set; } = false;

    public bool IsEmpty => Player1 == null && Player2 == null;
    public bool IsFull => Player1 != null && Player2 != null;

    public Room(string roomCode, RoomType type)
    {
        RoomCode = roomCode;
        Type = type;
    }

    public bool AddPlayer(string username, TcpClient socket)
    {
        if (Player1 == null)
        {
            Player1 = new RoomPlayer { Username = username, ClientSocket = socket };
            return true;
        }
        if (Player2 == null)
        {
            Player2 = new RoomPlayer { Username = username, ClientSocket = socket };
            return true;
        }
        return false;
    }

    public bool RemovePlayer(string username)
    {
        if (Player1 != null && Player1.Username == username)
        {
            Player1 = null;
            ResetReadyStates();
            return true;
        }
        if (Player2 != null && Player2.Username == username)
        {
            Player2 = null;
            ResetReadyStates();
            return true;
        }
        return false;
    }

    public void SetCharacter(string username, string characterId)
    {
        if (Player1?.Username == username) Player1.SelectedCharacterId = characterId;
        else if (Player2?.Username == username) Player2.SelectedCharacterId = characterId;
    }

    public void SetReady(string username, bool isReady)
    {
        if (Player1?.Username == username) Player1.IsReady = isReady;
        else if (Player2?.Username == username) Player2.IsReady = isReady;
    }

    public bool AreBothPlayersReady() => IsFull && Player1!.IsReady && Player2!.IsReady;

    public void ResetReadyStates()
    {
        if (Player1 != null) Player1.IsReady = false;
        if (Player2 != null) Player2.IsReady = false;
        IsInMatch = false;
    }

    public RoomStateDTO ToDTO()
    {
        return new RoomStateDTO
        {
            RoomCode = RoomCode,
            Type = Type,
            SelectedMapId = SelectedMapId,
            IsInMatch = IsInMatch,
            Player1 = Player1 == null ? null : new RoomPlayerStateDTO
            {
                Username = Player1.Username,
                SelectedCharacterId = Player1.SelectedCharacterId,
                IsReady = Player1.IsReady
            },
            Player2 = Player2 == null ? null : new RoomPlayerStateDTO
            {
                Username = Player2.Username,
                SelectedCharacterId = Player2.SelectedCharacterId,
                IsReady = Player2.IsReady
            }
        };
    }
}


public class RoomPlayer
{
    public string Username { get; set; } = string.Empty;
    public TcpClient ClientSocket { get; set; } = null!;
    public string SelectedCharacterId { get; set; } = "bobi";
    public bool IsReady { get; set; } = false;
}