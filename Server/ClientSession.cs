using System.Net;
using ServerCore;

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
            case PacketId.PlayerInfoReq:
            {
                PlayerInfoReq p = new();
                p.Read(buffer);
                Console.WriteLine($"PlayerInfoReq : {p.playerId} {p.name}");

                foreach (var skill in p.skills)
                    Console.WriteLine($"Skill({skill.id}) ({skill.level}) ({skill.duration})");
            }
                break;
        }

        Console.WriteLine($"RecvPacketId : {id}, Size : {size}");
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