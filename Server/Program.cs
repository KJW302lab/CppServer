using System.Net;
using ServerCore;

namespace Server;

class Program
{
    private static Listener _listener = new();
    
    static void Main(string[] args)
    {
        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[1];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        _listener.Initialize(endPoint, ()=> new ClientSession());
        
        Console.WriteLine("Listening...");

        while (true)
        {
            
        }
    }
}