using System.Net;
using System.Text;
using ServerCore;

public class ServerSession : PacketSession
{
    static unsafe void ToBytes(byte[] array, int offset, ulong value)
    {
        fixed (byte* ptr = &array[offset])
            *(ulong*)ptr = value;
    }

    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnSend(int numOfBytes)
    {
        // Console.WriteLine($"Send bytes : {numOfBytes}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisconnected : {endPoint}");
    }
}