using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore;

class Program
{
    static void Main(string[] args)
    {
        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
        
        // 문지기
        Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            // 문지기 교육
            listenSocket.Bind(endPoint);
        
            // 영업 시작
            // backlog : 최대 대기수
            listenSocket.Listen(10);
        
            // 10은 해당 소켓이 연결을 수락할 수 있는 최대 대기 큐 크기
            // 이 값은 연결 요청을 받아들이기 전에 대기열에 들어온 연결 요청의 최대 개수를 의미
            // 여기서는 listenSocket이라는 소켓이 동시에 최대 10개의 클라이언트 연결 요청을 대기열에 보관할 수 있도록 설정
            // backlog를 조절하여 동시에 처리 가능한 클라이언트 수 조정 가능
            // 만약 대기열이 가득 찬 상태에서 새로운 연결 요청이 들어오면, 클라이언트는 연결이 수락될 때까지 대기

            while (true)
            {
                Console.WriteLine("Listening...");
            
                // 손님 입장시킨다
                Socket clientSocket = listenSocket.Accept(); // Blocking 메서드
            
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}