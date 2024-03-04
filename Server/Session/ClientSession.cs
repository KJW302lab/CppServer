using System.Net;
using Server;
using ServerCore;

public class ClientSession : PacketSession
{
    public int SessionId { get; set; }
    public GameRoom Room { get; set; }
    
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");
        
        Program.Room.Enter(this);
    }
    
    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"Send bytes : {numOfBytes}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        SessionManager.Instance.Remove(this);

        if (Room != null)
        {
            Room.Leave(this);
            Room = null;
        }

        Console.WriteLine($"OnDisconnected : {endPoint}");
    }
}