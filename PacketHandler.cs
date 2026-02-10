namespace Csharp_game_server;

public static class PacketHandler
{
    public static void ReceivePacket(byte[] buffer)
    {
        // First bit signals if it is a multibyte packet type
        // 1 = multibyte
        // 0 = single-byte
        var multiByteType = (buffer[0] & 0b10000000) != 0;
        
        var packetTypeIndex =  (ushort)(buffer[0] & 0b01111111);
        
        if (multiByteType) packetTypeIndex = ReadMultiByteTypePacket(buffer);
        if (packetTypeIndex == 0) return;
        
        var packetType = (PacketType)packetTypeIndex;
    }

    private static ushort ReadMultiByteTypePacket(byte[] buffer)
    {
        var firstByte = buffer[0];
        var secondByte = buffer[1];
        
        firstByte = (byte)(firstByte & 0b01111111);
        var packetType = (ushort)((firstByte << 8) | secondByte);
        
        return packetType;
    }
}