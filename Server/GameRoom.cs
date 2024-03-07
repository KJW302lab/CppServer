using ServerCore;

namespace Server;

public class GameRoom : IJobQueue
{
    private List<ClientSession> _sessions = new();
    private JobQueue _jobQueue = new();
    
    public void Push(Action job)
    {
        _jobQueue.Push(job);
    }

    public void Broadcast(ClientSession session, string chat)
    {
        S_Chat packet = new S_Chat();
        packet.playerId = session.SessionId;
        packet.chat = $"{chat} I am {packet.playerId}";
        ArraySegment<byte> segment = packet.Write();

        foreach (var clientSession in _sessions)
            clientSession.Send(segment);
    }
    
    public void Enter(ClientSession session)
    {
        _sessions.Add(session);
        session.Room = this;   
    }

    public void Leave(ClientSession session)
    {
        _sessions.Remove(session);
    }
}