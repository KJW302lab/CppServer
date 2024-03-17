using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketQueue
{
    private static PacketQueue _instance;
    public static PacketQueue Instance => _instance ??= new();

    private Queue<IPacket> _packetQueue = new();
    private object _lock = new();

    public void Push(IPacket packet)
    {
        lock (_lock)
        {
            _packetQueue.Enqueue(packet);
        }
    }

    public IPacket Pop()
    {
        lock (_lock)
        {
            if (_packetQueue.Count <= 0)
                return null;

            return _packetQueue.Dequeue();
        }
    }
}
