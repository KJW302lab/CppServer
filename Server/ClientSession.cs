using System.Net;
using ServerCore;

namespace Server;

class Packet
{
    public ushort size;
    public ushort packetId;
}

class PlayerInfoReq : Packet
{
    public long playerId;
}

class PlayerInfoOk : Packet
{
    public int hp;
    public int attack;
}

public enum PacketId
{
    PLAYER_INFO_REQ = 1,
    PLAYER_INFO_OK = 2,
}

class ClientSession : PacketSession
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");
        
        Thread.Sleep(5000);
        Disconnect();
    }
    
    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        ushort count = 0;
        
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        switch ((PacketId)id)
        {
            case PacketId.PLAYER_INFO_REQ:
            {
                long playerId = BitConverter.ToInt64(buffer.Array, buffer.Offset + count);
                count += 8;
                Console.WriteLine($"PlayerInfoReq : {playerId}");
            }
                break;
        }
        
        Console.WriteLine($"RecvPacketId : {(PacketId)id}, Size : {size}");
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"Send bytes : {numOfBytes}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisconnected : {endPoint}");
    }
}