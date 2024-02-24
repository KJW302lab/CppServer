using System.Net.Sockets;
using System.Text;

namespace ServerCore;

public class Session
{
    private Socket _socket;
    private int _disconnected = 0;

    public void Start(Socket socket)
    {
        _socket = socket;
        
        SocketAsyncEventArgs args = new();
        args.Completed += OnReceiveCompleted;
        
        args.SetBuffer(new byte[1024], 0, 1024);
        RegisterReceive(args);
    }

    public void Send(byte[] sendBuff)
    {
        _socket.Send(sendBuff);
    }

    public void Disconnect()
    {
        if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            return;
        
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }

    #region 네트워크 통신
    void RegisterReceive(SocketAsyncEventArgs args)
    {
        bool pending = _socket.ReceiveAsync(args);
        if (pending == false)
            OnReceiveCompleted(null, args);
    }

    void OnReceiveCompleted(object? sender, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            try
            {
                string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                Console.WriteLine($"[From Client] {recvData}");
                RegisterReceive(args);
            }
            catch (Exception e)
            {
                Console.WriteLine($"OnReceiveCompleted Failed {e}");
            }
        }
        else
        {
            // todo Disconnect
        }
    }
    #endregion
}