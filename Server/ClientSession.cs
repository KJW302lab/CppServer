using System.Net;
using System.Text;
using ServerCore;

namespace Server;

public abstract class Packet
{
    public ushort size;
    public ushort packetId;

    public abstract ArraySegment<byte> Write();
    public abstract void Read(ArraySegment<byte> segment);
}

class PlayerInfoReq : Packet
{
    public long playerId;
    public string name;
    
    public struct SkillInfo
    {
        public int id;
        public short skillLevel;
        public float duration;

        public bool Write(Span<byte> span, ref ushort count)
        {
            bool success = true;
            
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), id);
            count += sizeof(int);
            
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), skillLevel);
            count += sizeof(short);
            
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), duration);
            count += sizeof(float);

            return success;
        }

        public void Read(ReadOnlySpan<byte> span, ref ushort count)
        {
            id = BitConverter.ToInt32(span.Slice(count, span.Length - count));
            count += sizeof(int);
            
            skillLevel = BitConverter.ToInt16(span.Slice(count, span.Length - count));
            count += sizeof(short);
            
            duration = BitConverter.ToSingle(span.Slice(count, span.Length - count));
            count += sizeof(float);
        }
    }

    public List<SkillInfo> skills = new();

    public PlayerInfoReq()
    {
        packetId = (ushort)PacketId.PLAYER_INFO_REQ;
    }
    
    public override void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> span = new(segment.Array, segment.Offset, segment.Count);

        // packetSize
        count += sizeof(ushort);
        // packetId
        count += sizeof(ushort);
        
        playerId = BitConverter.ToInt64(span.Slice(count, span.Length - count));
        count += sizeof(long);
        
        // string
        ushort nameLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
        count += sizeof(ushort);
        name = Encoding.Unicode.GetString(span.Slice(count, nameLen));
        count += nameLen;
        
        // skill list
        skills.Clear();
        ushort skillLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
        count += sizeof(ushort);
        for (int i = 0; i < skillLen; i++)
        {
            SkillInfo skill = new SkillInfo();
            skill.Read(span, ref count);
            skills.Add(skill);
        }
    }
    
    public override ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> span = new(segment.Array, segment.Offset, segment.Count);
        
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketId.PLAYER_INFO_REQ);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), playerId);
        count += sizeof(long);

        ushort nameLen = (ushort)Encoding.Unicode.GetBytes(name, 0, name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
        count += sizeof(ushort);
        count += nameLen;
        
        // skill list
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)skills.Count);
        count += sizeof(ushort);
        foreach (var skill in skills)
            success &= skill.Write(span, ref count);
        
        success &= BitConverter.TryWriteBytes(span, count);

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }
}

public enum PacketId
{
    PLAYER_INFO_REQ = 1,
    PLAYER_INFO_OK = 2,
}

class ClientSession : PacketSession
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");
        
        Thread.Sleep(5000);
        Disconnect();
    }
    
    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        ushort count = 0;
        
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        switch ((PacketId)id)
        {
            case PacketId.PLAYER_INFO_REQ:
            {
                PlayerInfoReq p = new();
                p.Read(buffer);
                Console.WriteLine($"PlayerInfoReq : {p.playerId} {p.name}");
                
                foreach (var skill in p.skills)
                    Console.WriteLine($"Skill({skill.id}) ({skill.skillLevel}) ({skill.duration})");
            }
                break;
        }

        Console.WriteLine($"RecvPacketId : {id}, Size : {size}");
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"Send bytes : {numOfBytes}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisconnected : {endPoint}");
    }
}