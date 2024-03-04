namespace Server;

public class GameRoom
{
    private List<ClientSession> _sessions = new();
    private object _lock = new();

    public void Broadcast(ClientSession session, string chat)
    {
        S_Chat packet = new S_Chat();
        packet.playerId = session.SessionId;
        packet.chat = $"{chat} I am {packet.playerId}";
        ArraySegment<byte> segment = packet.Write();

        lock (_lock)
        {
            foreach (var clientSession in _sessions)
                clientSession.Send(segment);
        }
    }
    
    public void Enter(ClientSession session)
    {
        lock (_lock)
        {
            _sessions.Add(session);
            session.Room = this;   
        }
    }

    public void Leave(ClientSession session)
    {
        lock (_lock)
        {
            _sessions.Remove(session);   
        }
    }
}