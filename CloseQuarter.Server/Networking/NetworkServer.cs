using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

using CloseQuarter.Server.Data;
using CloseQuarter.Server.Rooms;
using CloseQuarter.Server.Services;
using CloseQuarter.Shared.DTOs;

namespace CloseQuarter.Server.Networking;

public class NetworkServer
{
    private readonly TcpListener _listener;
    private readonly RoomManager _roomManager = new();
    private bool _isRunning = false;

    public NetworkServer(int port = 5000)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public void Start()
    {
        _listener.Start();
        _isRunning = true;
        Console.WriteLine($"[Server] Listening on port 5000...");

        while (_isRunning)
        {
            var clientSocket = _listener.AcceptTcpClient();
            Task.Run(() => HandleClientAsync(clientSocket));
        }
    }

    private async Task HandleClientAsync(TcpClient socket)
    {
        string? authenticatedUsername = null;
        using var stream = socket.GetStream();
        var lengthBuffer = new byte[4];

        try
        {
            while (socket.Connected)
            {
                int totalLengthRead = 0;
                while (totalLengthRead < 4)
                {
                    int read = await stream.ReadAsync(lengthBuffer, totalLengthRead, 4 - totalLengthRead);
                    if (read == 0) return;
                    totalLengthRead += read;
                }

                int packetLength = BitConverter.ToInt32(lengthBuffer, 0);
                byte[] packetBuffer = new byte[packetLength];
                int totalPacketRead = 0;

                while (totalPacketRead < packetLength)
                {
                    int read = await stream.ReadAsync(packetBuffer, totalPacketRead, packetLength - totalPacketRead);
                    if (read == 0) return;
                    totalPacketRead += read;
                }

                string rawJson = Encoding.UTF8.GetString(packetBuffer);
                var packet = JsonSerializer.Deserialize<NetworkPacket>(rawJson);

                if (packet != null)
                {
                    await ProcessPacketAsync(packet, socket, stream, (user) => authenticatedUsername = user);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Client error: {ex.Message}");
        }
        finally
        {
            if (!string.IsNullOrEmpty(authenticatedUsername))
            {
                _roomManager.LeaveAllRooms(authenticatedUsername);
            }
            socket.Close();
            Console.WriteLine("[Server] Client session closed.");
        }
    }

    private async Task ProcessPacketAsync(NetworkPacket packet, TcpClient socket, NetworkStream stream, Action<string> onAuthSuccess)
    {
        using var db = new AppDbContext();
        var authService = new AuthService(db);

        switch (packet.Type)
        {
            case PacketType.LoginRequest:
                {
                    var req = JsonSerializer.Deserialize<LoginRequestDTO>(packet.PayloadJson);
                    if (req != null)
                    {
                        var res = await authService.LoginOrRegisterAsync(req);
                        if (res.Success) onAuthSuccess(res.Username);
                        await SendPacketAsync(stream, PacketType.AuthResponse, res);
                    }
                    break;
                }

            case PacketType.TokenLoginRequest:
                {
                    var req = JsonSerializer.Deserialize<TokenLoginRequestDTO>(packet.PayloadJson);
                    if (req != null)
                    {
                        var res = await authService.LoginWithTokenAsync(req);
                        if (res.Success) onAuthSuccess(res.Username);
                        await SendPacketAsync(stream, PacketType.AuthResponse, res);
                    }
                    break;
                }

            case PacketType.CreateRoomRequest:
                {
                    var req = JsonSerializer.Deserialize<CreateRoomRequestDTO>(packet.PayloadJson);
                    string username = ExtractUsernameFromPayload(packet.PayloadJson);
                    if (req != null)
                    {
                        var room = _roomManager.CreatePrivateRoom(username, socket);
                        BroadcastRoomState(room);
                    }
                    break;
                }

            case PacketType.JoinRoomRequest:
                {
                    var req = JsonSerializer.Deserialize<JoinRoomRequestDTO>(packet.PayloadJson);
                    string username = ExtractUsernameFromPayload(packet.PayloadJson);
                    if (req != null)
                    {
                        var room = _roomManager.JoinRoomByCode(req.RoomCode, username, socket);
                        if (room != null) BroadcastRoomState(room);
                        else await SendErrorAsync(stream, "Room is full or doesn't exist!");
                    }
                    break;
                }

            case PacketType.MatchmakeRequest:
                {
                    string username = ExtractUsernameFromPayload(packet.PayloadJson);
                    var room = _roomManager.MatchmakeOrJoin(username, socket);
                    BroadcastRoomState(room);
                    break;
                }

            case PacketType.SelectCharacterRequest:
                {
                    var req = JsonSerializer.Deserialize<SelectCharacterRequestDTO>(packet.PayloadJson);
                    string username = ExtractUsernameFromPayload(packet.PayloadJson);
                    string roomCode = ExtractRoomCodeFromPayload(packet.PayloadJson);

                    var room = _roomManager.GetRoomByCode(roomCode);
                    if (room != null && req != null)
                    {
                        room.SetCharacter(username, req.CharacterId);
                        BroadcastRoomState(room);
                    }
                    break;
                }

            case PacketType.SetReadyRequest:
                {
                    var req = JsonSerializer.Deserialize<SetReadyRequestDTO>(packet.PayloadJson);
                    string username = ExtractUsernameFromPayload(packet.PayloadJson);
                    string roomCode = ExtractRoomCodeFromPayload(packet.PayloadJson);

                    var room = _roomManager.GetRoomByCode(roomCode);
                    if (room != null && req != null)
                    {
                        room.SetReady(username, req.IsReady);

                        if (room.AreBothPlayersReady())
                        {
                            room.IsInMatch = true;
                        }

                        BroadcastRoomState(room);
                    }
                    break;
                }
        }
    }

    private void BroadcastRoomState(Room room)
    {
        var dto = room.ToDTO();
        var packet = new NetworkPacket
        {
            Type = PacketType.RoomStateUpdate,
            PayloadJson = JsonSerializer.Serialize(dto)
        };

        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(packet));
        byte[] lengthPrefix = BitConverter.GetBytes(body.Length);

        byte[] data = new byte[4 + body.Length];
        Buffer.BlockCopy(lengthPrefix, 0, data, 0, 4);
        Buffer.BlockCopy(body, 0, data, 4, body.Length);

        SendToPlayerSocket(room.Player1?.ClientSocket, data);
        SendToPlayerSocket(room.Player2?.ClientSocket, data);
    }

    private void SendToPlayerSocket(TcpClient? socket, byte[] data)
    {
        if (socket != null && socket.Connected)
        {
            try
            {
                socket.GetStream().Write(data, 0, data.Length);
            }
            catch { }
        }
    }

    private async Task SendPacketAsync<T>(NetworkStream stream, PacketType type, T payload)
    {
        var packet = new NetworkPacket
        {
            Type = type,
            PayloadJson = JsonSerializer.Serialize(payload)
        };

        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(packet));
        byte[] lengthPrefix = BitConverter.GetBytes(body.Length);

        byte[] data = new byte[4 + body.Length];
        Buffer.BlockCopy(lengthPrefix, 0, data, 0, 4);
        Buffer.BlockCopy(body, 0, data, 4, body.Length);

        await stream.WriteAsync(data, 0, data.Length);
    }

    private async Task SendErrorAsync(NetworkStream stream, string message)
    {
        await SendPacketAsync(stream, PacketType.ErrorMessage, message);
    }

    private string ExtractUsernameFromPayload(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("Username", out var prop) ||
                doc.RootElement.TryGetProperty("username", out prop))
            {
                return prop.GetString() ?? "";
            }
        }
        catch { }

        return "";
    }

    private string ExtractRoomCodeFromPayload(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("RoomCode", out var prop) ||
                doc.RootElement.TryGetProperty("roomCode", out prop))
            {
                return prop.GetString() ?? "";
            }
        }
        catch { }

        return "";
    }
}