using System.Net;
using System.Text;
using ServerCore;

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
        
        PlayerInfoReq packet = new() {playerId = 1001, name = "ABCD"};
        var skill = new PlayerInfoReq.Skill() { id = 101, duration = 3.0f, level = 1 };
        skill.attributes.Add(new(){att = 77});
        packet.skills.Add(skill);
        packet.skills.Add(new PlayerInfoReq.Skill(){id = 101, duration = 3.0f, level = 1});
        packet.skills.Add(new PlayerInfoReq.Skill(){id = 102, duration = 2.5f, level = 2});
        packet.skills.Add(new PlayerInfoReq.Skill(){id = 103, duration = 6.0f, level = 3});
        packet.skills.Add(new PlayerInfoReq.Skill(){id = 104, duration = 5.6f, level = 4});

        // for (int i = 0; i < 5; i++)
        {
            ArraySegment<byte> segment = packet.Write();

            if (segment != null)
                Send(segment);
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