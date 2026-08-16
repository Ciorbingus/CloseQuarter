using System.Collections.Concurrent;
using System.Net.Sockets;

using CloseQuarter.Shared.DTOs;

namespace CloseQuarter.Server.Rooms;

public class RoomManager
{
    private readonly ConcurrentDictionary<string, Room> _rooms = new();
    private readonly Random _random = new();

    public Room CreatePrivateRoom(string hostUsername, TcpClient socket)
    {
        string roomCode = GenerateUniqueRoomCode();
        var room = new Room(roomCode, RoomType.Private);
        room.AddPlayer(hostUsername, socket);

        _rooms[roomCode] = room;
        return room;
    }

    public Room? JoinRoomByCode(string roomCode, string username, TcpClient socket)
    {
        string cleanCode = roomCode.Trim().ToUpper();

        if (_rooms.TryGetValue(cleanCode, out var room))
        {
            if (!room.IsFull && !room.IsInMatch)
            {
                if (room.AddPlayer(username, socket))
                {
                    return room;
                }
            }
        }

        return null;
    }

    public Room MatchmakeOrJoin(string username, TcpClient socket)
    {

        var availableRoom = _rooms.Values.FirstOrDefault(r => r.Type == RoomType.Matchmaking && !r.IsFull && !r.IsInMatch);

        if (availableRoom != null)
        {
            availableRoom.AddPlayer(username, socket);
            return availableRoom;
        }

        string roomCode = GenerateUniqueRoomCode();
        var newRoom = new Room(roomCode, RoomType.Matchmaking);
        newRoom.AddPlayer(username, socket);

        _rooms[roomCode] = newRoom;
        return newRoom;
    }

    public void LeaveAllRooms(string username)
    {
        foreach (var kvp in _rooms)
        {
            var room = kvp.Value;
            if (room.RemovePlayer(username))
            {
                if (room.IsEmpty)
                {
                    _rooms.TryRemove(kvp.Key, out _);
                }
            }
        }
    }

    public Room? GetRoomByCode(string roomCode)
    {
        _rooms.TryGetValue(roomCode.Trim().ToUpper(), out var room);
        return room;
    }

    private string GenerateUniqueRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string code;

        do
        {
            char[] stringChars = new char[6];
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[_random.Next(chars.Length)];
            }
            code = new string(stringChars);
        } 
        while (_rooms.ContainsKey(code));

        return code;
    }
}