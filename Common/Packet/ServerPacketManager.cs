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
      _makeFunc.Add((ushort)PacketId.C_LeaveGame, MakePacket<C_LeaveGame>);
      _handler.Add((ushort)PacketId.C_LeaveGame, PacketHandler.C_LeaveGameHandler);

      _makeFunc.Add((ushort)PacketId.C_Move, MakePacket<C_Move>);
      _handler.Add((ushort)PacketId.C_Move, PacketHandler.C_MoveHandler);

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