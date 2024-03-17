namespace PacketGenerator;

public class PacketFormat
{
    // 0 packet Regist
    public static string managerFormat = 
@"using System;
using System.Collections.Generic;
using ServerCore;

public class PacketManager
{{
    #region Instance
    private static PacketManager _instance;
    public static PacketManager Instance => _instance ??= new();
    #endregion

    private Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new();
    private Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    PacketManager()
    {{
        Register();
    }}

    public void Register()
    {{
{0}    }}
    
    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null)
    {{
        ushort count = 0;
        
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        if (_makeFunc.TryGetValue(id, out var func))
        {{
            IPacket packet = func.Invoke(session, buffer);
            if (onRecvCallback != null)
                onRecvCallback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }}
    }}

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {{
        T packet = new T();
        packet.Read(buffer);
        return packet;
    }}

    public void HandlePacket(PacketSession session, IPacket packet)
    {{
        if (_handler.TryGetValue(packet.Protocol, out var action))
            action.Invoke(session, packet);
    }}
}}";

    // 0 packetName
    public static string mangerRegisterFormat = 
@"      _makeFunc.Add((ushort)PacketId.{0}, MakePacket<{0}>);
      _handler.Add((ushort)PacketId.{0}, PacketHandler.{0}Handler);

";
    
    
    // 0 packet name/number
    // 1 packet list
    public static string fileFormat = 
@"using System;
using System.Net;
using System.Text;
using ServerCore;
using System.Collections.Generic;

public enum PacketId
{{
	{0}
}}

public interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}}

{1}";


    // 0 packet name
    // 1 packet number
    public static string packetEnumFormat = 
@"{0} = {1},";
    
    
    
    // 0 packet Name
    // 1 member
    // 2 member Read
    // 3 member Write
    public static string packetFormat = 
@"
public class {0} : IPacket
{{
    {1}

    public ushort Protocol => (ushort)PacketId.{0};

    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> s = new(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        {2}
    }}
    
    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;
        Span<byte> s = new(segment.Array, segment.Offset, segment.Count);
        
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketId.{0});
        count += sizeof(ushort);
        {3}
        success &= BitConverter.TryWriteBytes(s, count);
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }}
}}
";

    // 0 type
    // 1 name
    public static string memberFormat = 
@"public {0} {1};";

    // 0 listName (upper)
    // 1 listName (lower)
    // 2 list member
    // 3 read
    // 4 write
    public static string memberListFormat = 
@"public List<{0}> {1}s = new();

public class {0}
{{
    {2}
    public void Read(ReadOnlySpan<byte> s, ref ushort count)
    {{
        {3}
    }}

    public bool Write(Span<byte> s, ref ushort count)
    {{
        bool success = true;
        {4}
        return success;
    }}
}}";


    // 0 listName upper
    // 1 listName lower
    public static string readListFormat = 
@"this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(s, ref count);
    {1}s.Add({1});
}}";

    
    // 0 listName lower
    public static string writeListFormat = 
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.{0}s.Count);
count += sizeof(ushort);
foreach (var {0} in {0}s)
    success &= {0}.Write(s, ref count);";

    // 0 name
    // 1 To~type
    // 2 type
    public static string readFormat = 
@"this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});";

    // 0 name
    // 1 type
    public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});";

    // 0 name
    public static string readStringFormat = 
@"ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;";

    // 0 name
    // 1 type
    public static string writeFormat = 
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});";
    
    // 0 name
    // 1 type
    public static string writeByteFormat = 
@"segment.Array[segment.Offset + count] = (byte)this.{0};
count += sizeof({1});";

    // 0 name
    public static string writeStringFormat = 
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, {0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";
}