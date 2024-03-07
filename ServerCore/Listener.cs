using System.Net;
using System.Net.Sockets;

namespace ServerCore;

public class Listener
{
    private Socket _listenSocket;
    private Func<Session> _sessionFactory;

    private int _connected;

    public void Initialize(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
    {
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _sessionFactory += sessionFactory;
        
        _listenSocket.Bind(endPoint);
        
        _listenSocket.Listen(backlog);

        for (int i = 0; i < register; i++)
        {
            SocketAsyncEventArgs args = new();
            args.Completed += OnAcceptCompleted;
            RegisterAccept(args);   
        }
    }
    
    void RegisterAccept(SocketAsyncEventArgs args)
    {
        args.AcceptSocket = null;

        bool pending = _listenSocket.AcceptAsync(args);
        if (pending == false)
            OnAcceptCompleted(null, args);
    }

    void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success && args.AcceptSocket != null)
        {
            Session session = _sessionFactory.Invoke();
            session.Start(args.AcceptSocket);
            session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            Console.WriteLine($"connected : {++_connected}");
        }
        else
            Console.WriteLine(args.SocketError);

        RegisterAccept(args);
    }
}