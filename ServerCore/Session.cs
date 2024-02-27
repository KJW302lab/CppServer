using System.Net;
using System.Net.Sockets;

namespace ServerCore;

public abstract class Session
{
    private Socket _socket;
    private int _disconnected = 0;
    SocketAsyncEventArgs _sendArgs = new();
    SocketAsyncEventArgs _recvArgs = new();

    private object _lock = new();
    private Queue<byte[]> _sendQueue = new();
    
    List<ArraySegment<byte>> _pendingList = new();

    private RecvBuffer _recvBuffer = new(1024);

    public abstract void OnConnected(EndPoint endPoint);
    public abstract int OnReceive(ArraySegment<byte> buffer);
    public abstract void OnSend(int numOfBytes);
    public abstract void OnDisconnected(EndPoint endPoint);

    public void Start(Socket socket)
    {
        _socket = socket;
        
        _recvArgs.Completed += OnReceiveCompleted;
        _sendArgs.Completed += OnSendCompleted;
        
        RegisterReceive();
    }

    public void Send(byte[] sendBuff)
    {
        lock (_lock)
        {
            _sendQueue.Enqueue(sendBuff);
            if (_pendingList.Count == 0)
                RegisterSend();
        }
    }

    public void Disconnect()
    {
        if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            return;
        
        OnDisconnected(_socket.RemoteEndPoint);
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }

    #region 네트워크 통신

    void RegisterSend()
    {
        while (_sendQueue.Count > 0)
        {
            byte[] buff = _sendQueue.Dequeue();
            _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
        }

        _sendArgs.BufferList = _pendingList;
        
        bool pending = _socket.SendAsync(_sendArgs);
        if (pending == false)
            OnSendCompleted(null, _sendArgs);
    }

    void OnSendCompleted(object? sender, SocketAsyncEventArgs args)
    {
        lock (_lock)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    _sendArgs.BufferList = null;
                    _pendingList.Clear();

                    OnSend(_sendArgs.BytesTransferred);
                    if (_sendQueue.Count > 0)
                        RegisterSend();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnSendCompleted Failed {e}");
                }
            }
            else
                Disconnect();   
        }
    }
    
    void RegisterReceive()
    {
        _recvBuffer.Clear();
        ArraySegment<byte> segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
        
        bool pending = _socket.ReceiveAsync(_recvArgs);
        if (pending == false)
            OnReceiveCompleted(null, _recvArgs);
    }

    void OnReceiveCompleted(object? sender, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            try
            {
                // write 커서 이동
                if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                {
                    Disconnect();
                    return;
                }
                
                // 컨텐츠쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다
                int processLength = OnReceive(_recvBuffer.ReadSegment);
                if (processLength < 0 || _recvBuffer.DataSize < processLength)
                {
                    Disconnect();
                    return;
                }
                
                // read 커서 이동
                if (_recvBuffer.OnRead(processLength) == false)
                {
                    Disconnect();
                    return;
                }
                
                RegisterReceive();
            }
            catch (Exception e)
            {
                Console.WriteLine($"OnReceiveCompleted Failed {e}");
            }
        }
        else
            Disconnect();
    }
    #endregion
}