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
            // 받는다
            byte[] recvBuff = new byte[1024];
            int recvBytes = clientSocket.Receive(recvBuff);
            string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
            Console.WriteLine($"[From Client] {recvData}");

            // 보낸다
            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome To MMORPG Server !");
            clientSocket.Send(sendBuff);
            
            // 쫓아낸다
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
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
        IPAddress ipAddr = ipHost.AddressList[1];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        _listener.Initialize(endPoint, OnAcceptHandler);
        
        Console.WriteLine("Listening...");

        while (true)
        {
            
        }
    }
}