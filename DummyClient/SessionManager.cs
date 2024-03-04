namespace DummyClient;

public class SessionManager
{
    private static SessionManager _session = new();
    public static SessionManager Instance => _session;

    private List<ServerSession> _sessions = new();
    private object _lock = new();

    public void SendForEach()
    {
        lock (_lock)
        {
            foreach (var serverSession in _sessions)
            {
                C_Chat chatPacket = new();
                chatPacket.chat = $"Hello Server!";
                ArraySegment<byte> segment = chatPacket.Write();
                serverSession.Send(segment);
            }
        }
    }

    public ServerSession Generate()
    {
        lock (_lock)
        {
            ServerSession session = new();
            _sessions.Add(session);
            return session;
        }
    }
}