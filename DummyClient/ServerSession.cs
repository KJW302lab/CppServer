using System.Net;
using System.Text;
using ServerCore;

namespace DummyClient;

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

class ServerSession : Session
{
    static unsafe void ToBytes(byte[] array, int offset, ulong value)
    {
        fixed (byte* ptr = &array[offset])
            *(ulong*)ptr = value;
    }
    
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");
        
        PlayerInfoReq packet = new() {packetId = (ushort)PacketId.PLAYER_INFO_REQ, playerId = 1001};

        // for (int i = 0; i < 5; i++)
        {
            ArraySegment<byte> s = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;
            
            // success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), packet.size);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), packet.packetId);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), packet.playerId);
            count += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count);

            ArraySegment<byte> sendBuffer = SendBufferHelper.Close(count);

            if (success)
                Send(sendBuffer);
        }
    }

    public override int OnReceive(ArraySegment<byte> buffer)
    {
        string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"[From Server] {recvData}");

        return buffer.Count;
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