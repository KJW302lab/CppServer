using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore;

class Program
{
    private static Listener _listener = new();

    static void OnAcceptHandler(Socket clientSocket)
    {
        try
        {
            Session session = new();
            session.Start(clientSocket);
            
            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome To MMORPG Server !");
            session.Send(sendBuff);
            
            Thread.Sleep(1000);
            session.Disconnect();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    static void Main(string[] args)
    {
        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[3];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        _listener.Initialize(endPoint, OnAcceptHandler);
        
        Console.WriteLine("Listening...");

        while (true)
        {
            
        }
    }
}