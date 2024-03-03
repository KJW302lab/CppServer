using ServerCore;

public class PacketManager
{
    #region Instance
    private static PacketManager _instance;
    public static PacketManager Instance => _instance ??= new();
    #endregion

    private Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new();
    private Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    public void Register()
    {
      _onRecv.Add((ushort)PacketId.C_PlayerInfoReq, MakePacket<C_PlayerInfoReq>);
      _handler.Add((ushort)PacketId.C_PlayerInfoReq, PacketHandler.C_PlayerInfoReqHandler);

    }
    
    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;
        
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        if (_onRecv.TryGetValue(id, out var action))
            action.Invoke(session, buffer);
    }

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T packet = new T();
        packet.Read(buffer);

        if (_handler.TryGetValue(packet.Protocol, out var action))
            action.Invoke(session, packet);
    }
}