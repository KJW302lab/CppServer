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
        string input = Console.ReadLine();

        if (input == null)
            return;

        IPEndPoint endPoint;
            
        string[] arr = input.Split(':');
        string ipAddrStr = arr[0];
        string portStr = arr[1];

        if (!IPAddress.TryParse(ipAddrStr, out IPAddress ip)) return;
        if (!int.TryParse(portStr, out int port)) return;

        endPoint = new(ip, port);

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