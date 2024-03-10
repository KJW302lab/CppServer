using System.Net;
using ServerCore;

namespace Server;

class Program
{
    private static Listener _listener = new();
    public static GameRoom Room = new();

    static void FlushRoom()
    {
        Room.Push(()=> Room.Flush());
        JobTimer.Instance.Push(FlushRoom, 250);
    }
    
    static void Main(string[] args)
    {
        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        _listener.Initialize(endPoint, ()=> SessionManager.Instance.Generate());
        
        Console.WriteLine("Listening...");
        
        // FlushRoom();
        JobTimer.Instance.Push(FlushRoom);
        
        while (true)
        {
            JobTimer.Instance.Flush();
        }
    }
}