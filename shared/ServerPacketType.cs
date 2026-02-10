namespace Csharp_game_server.Shared;

public enum ServerPacketType
{
    ClientJoin = 1,
    ClientHeartbeat,
    ClientDisconnect,
}