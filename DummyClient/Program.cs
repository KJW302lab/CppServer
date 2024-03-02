using System.Net;
using System.Text;
using ServerCore;

namespace DummyClient;

class Program
{
    static void Main(string[] args)
    {
        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[1];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new();
        connector.Connect(endPoint, () => new ServerSession());

        while (true)
        {
            Thread.Sleep(1000);
        }
    }
}