using System.Net;
using System.Net.Sockets;

namespace ServerCore;

public abstract class PacketSession : Session
{
    public static readonly int HeaderSize = 2;

    public sealed override int OnReceive(ArraySegment<byte> buffer)
    {
        int processLength = 0;
        int packetCount = 0;

        while (true)
        {
            // 최소한 헤더는 파싱할 수 있는지 확인
            if (buffer.Count < HeaderSize)
                break;
            
            // 패킷이 완전체로 도착했는지 확인
            ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            if (buffer.Count < dataSize)
                break;
            
            // 여기까지 왔으면 패킷 조립 가능
            OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

            packetCount++;
            processLength += dataSize;
            buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
        }

        if (packetCount >= 1)
            Console.WriteLine($"패킷 모아보내기 : {packetCount}");
        
        return processLength;
    }

    public abstract void OnRecvPacket(ArraySegment<byte> buffer);
}

public abstract class Session
{
    private Socket _socket;
    private int _disconnected = 0;
    SocketAsyncEventArgs _sendArgs = new();
    SocketAsyncEventArgs _recvArgs = new();

    private object _lock = new();
    private Queue<ArraySegment<byte>> _sendQueue = new();
    
    List<ArraySegment<byte>> _pendingList = new();

    private RecvBuffer _recvBuffer = new(65535);

    public abstract void OnConnected(EndPoint endPoint);
    public abstract int OnReceive(ArraySegment<byte> buffer);
    public abstract void OnSend(int numOfBytes);
    public abstract void OnDisconnected(EndPoint endPoint);

    void Clear()
    {
        lock (_lock)
        {
            _sendQueue.Clear();
            _pendingList.Clear();
        }
    }

    public void Start(Socket socket)
    {
        _socket = socket;
        
        _recvArgs.Completed += OnReceiveCompleted;
        _sendArgs.Completed += OnSendCompleted;
        
        RegisterReceive();
    }

    public void Send(ArraySegment<byte> sendBuff)
    {
        lock (_lock)
        {
            _sendQueue.Enqueue(sendBuff);
            if (_pendingList.Count == 0)
                RegisterSend();
        }
    }
    
    public void Send(List<ArraySegment<byte>> sendBuffList)
    {
        if (sendBuffList.Count <= 0)
            return;
        
        lock (_lock)
        {
            foreach (var segment in sendBuffList)
                _sendQueue.Enqueue(segment);
            
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
        Clear();
    }

    #region 네트워크 통신

    void RegisterSend()
    {
        if (_disconnected == 1)
            return;
        
        while (_sendQueue.Count > 0)
        {
            ArraySegment<byte> buff = _sendQueue.Dequeue();
            _pendingList.Add(buff);
        }

        _sendArgs.BufferList = _pendingList;

        try
        {
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }
        catch (Exception e)
        {
            Console.WriteLine( $"RegisterSend Failed {e}");
        }
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
                    Console.WriteLine($"OnSendCompleted Failed : {e}");
                }
            }
            else
                Disconnect();   
        }
    }
    
    void RegisterReceive()
    {
        if (_disconnected == 1)
            return;
        
        _recvBuffer.Clear();
        ArraySegment<byte> segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

        try
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnReceiveCompleted(null, _recvArgs);
        }
        catch (Exception e)
        {
            Console.WriteLine($"RegisterRecv Failed : {e}");
            throw;
        }
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