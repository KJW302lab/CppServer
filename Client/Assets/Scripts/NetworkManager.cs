using System;
using System.Net;
using ServerCore;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private ServerSession _session = new();

    public void Send(ArraySegment<byte> sendBuffer)
    {
        _session.Send(sendBuffer);
    }
    
    void Start()
    {
        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new();
        connector.Connect(endPoint, () => _session, 1);
    }

    private void Update()
    {
        var packets = PacketQueue.Instance.PopAll();
        
        foreach (var packet in packets)
            PacketManager.Instance.HandlePacket(_session, packet);
    }
}
