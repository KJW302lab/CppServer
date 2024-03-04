using System.Net;
using System.Net.Sockets;

namespace ServerCore;

public class Connector
{
    private Socket _socket;
    private Func<Session> _sessionFactory;

    public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            _socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            SocketAsyncEventArgs args = new();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            RegisterConnect(args);   
        }
    }

    void RegisterConnect(SocketAsyncEventArgs args)
    {
        bool pending = _socket.ConnectAsync(args);
        if (pending == false)
            OnConnectCompleted(null, args);
    }

    void OnConnectCompleted(object? sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            Session session = _sessionFactory.Invoke();
            session.Start(args.ConnectSocket);
            session.OnConnected(args.RemoteEndPoint);
        }
        else
            Console.WriteLine($"OnConnectCompleted Fail : {args.SocketError}");
    }
}