using System;
using System.Collections.Generic;
using ServerCore;

public class PacketManager
{
    #region Instance
    private static PacketManager _instance;
    public static PacketManager Instance => _instance ??= new();
    #endregion

    private Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new();
    private Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    PacketManager()
    {
        Register();
    }

    public void Register()
    {
      _makeFunc.Add((ushort)PacketId.S_BroadcastEnterGame, MakePacket<S_BroadcastEnterGame>);
      _handler.Add((ushort)PacketId.S_BroadcastEnterGame, PacketHandler.S_BroadcastEnterGameHandler);

      _makeFunc.Add((ushort)PacketId.S_BroadcastLeaveGame, MakePacket<S_BroadcastLeaveGame>);
      _handler.Add((ushort)PacketId.S_BroadcastLeaveGame, PacketHandler.S_BroadcastLeaveGameHandler);

      _makeFunc.Add((ushort)PacketId.S_PlayerList, MakePacket<S_PlayerList>);
      _handler.Add((ushort)PacketId.S_PlayerList, PacketHandler.S_PlayerListHandler);

      _makeFunc.Add((ushort)PacketId.S_BroadcastMove, MakePacket<S_BroadcastMove>);
      _handler.Add((ushort)PacketId.S_BroadcastMove, PacketHandler.S_BroadcastMoveHandler);

    }
    
    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null)
    {
        ushort count = 0;
        
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        if (_makeFunc.TryGetValue(id, out var func))
        {
            IPacket packet = func.Invoke(session, buffer);
            if (onRecvCallback != null)
                onRecvCallback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }
    }

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T packet = new T();
        packet.Read(buffer);
        return packet;
    }

    public void HandlePacket(PacketSession session, IPacket packet)
    {
        if (_handler.TryGetValue(packet.Protocol, out var action))
            action.Invoke(session, packet);
    }
}