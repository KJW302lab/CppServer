namespace DummyClient;

public class SessionManager
{
    private static SessionManager _session = new();
    public static SessionManager Instance => _session;

    private List<ServerSession> _sessions = new();
    private object _lock = new();

    private Random _rand = new();
    
    public void SendForEach()
    {
        lock (_lock)
        {
            foreach (var serverSession in _sessions)
            {
                C_Move movePacket = new();
                movePacket.posX = _rand.Next(-50, 50);
                movePacket.posY = 0;
                movePacket.posZ = _rand.Next(-50, 50);
                serverSession.Send(movePacket.Write());
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