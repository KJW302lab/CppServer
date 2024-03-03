using ServerCore;

public class PacketHandler
{
    public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet)
    {
        C_PlayerInfoReq p = packet as C_PlayerInfoReq;
        
        Console.WriteLine($"PlayerInfoReq : {p.playerId} {p.name}");

        foreach (var skill in p.skills)
            Console.WriteLine($"Skill({skill.id}) ({skill.level}) ({skill.duration})");
    }
}