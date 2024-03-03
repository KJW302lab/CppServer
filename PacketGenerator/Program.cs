using System.Xml;

namespace PacketGenerator;

class Program
{
    private static string _genPackets;
    private static ushort _packetId;
    private static string _packetEnums;
    
    static void Main(string[] args)
    {
        string pdlPath = "PDL.xml";
        
        XmlReaderSettings settings = new()
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        if (args.Length >= 1)
            pdlPath = args[0];

        using (XmlReader r = XmlReader.Create(pdlPath, settings))
        {
            r.MoveToContent();

            while (r.Read())
            {
                if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                    ParsePacket(r);
                // Console.WriteLine(r.Name + " " + r["name"]);
            }

            string fileText = string.Format(PacketFormat.fileFormat, _packetEnums, _genPackets);
            
            File.WriteAllText("GenPackets.cs", fileText);
        }
    }

    public static void ParsePacket(XmlReader r)
    {
        if (r.NodeType == XmlNodeType.EndElement)
            return;

        if (r.Name.ToLower() != "packet")
        {
            Console.WriteLine("Invalid packet node");
            return;
        }

        string packetName = r["name"];
        if (string.IsNullOrEmpty(packetName))
        {
            Console.WriteLine("Packet without name");
            return;
        }

        var t = ParseMembers(r);

        _genPackets += string.Format(PacketFormat.packetFormat,
            packetName, t.Item1, t.Item2, t.Item3);

        _packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++_packetId) + Environment.NewLine + "\t";
    }

    public static Tuple<string, string, string> ParseMembers(XmlReader r)
    {
        string packetName = r["name"];

        string memberCode = "";
        string memberReadCode = "";
        string memberWriteCode = "";

        int depth = r.Depth + 1;
        
        while (r.Read())
        {
            if (r.Depth != depth)
                break;

            string memberName = r["name"];
            if (string.IsNullOrEmpty(memberName))
            {
                Console.WriteLine("Member without name");
                return null;
            }

            if (string.IsNullOrEmpty(memberCode) == false)
                memberCode += Environment.NewLine;
            if (string.IsNullOrEmpty(memberReadCode) == false)
                memberReadCode += Environment.NewLine;
            if (string.IsNullOrEmpty(memberWriteCode) == false)
                memberWriteCode += Environment.NewLine;

            string memberType = r.Name.ToLower();
            switch (memberType)
            {
                case "byte":
                case "sbyte":
                    memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                    memberReadCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                    memberWriteCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                    break;
                case "bool":
                case "short":
                case "ushort":
                case "int":
                case "long":
                case "float":
                case "double":
                    memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                    memberReadCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                    memberWriteCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                    break;
                case "string":
                    memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                    memberReadCode += string.Format(PacketFormat.readStringFormat, memberName);
                    memberWriteCode += string.Format(PacketFormat.writeStringFormat, memberName);
                    break;
                case "list":
                    var t = ParseList(r);
                    memberCode += t.Item1;
                    memberReadCode += t.Item2;
                    memberWriteCode += t.Item3;
                    break;
                default:
                    break;
            }
        }

        memberCode = memberCode.Replace("\n", "\n\t");
        memberReadCode = memberReadCode.Replace("\n", "\n\t\t");
        memberWriteCode = memberWriteCode.Replace("\n", "\n\t\t");
        return new(memberCode, memberReadCode, memberWriteCode);
    }
    
    public static Tuple<string, string, string> ParseList(XmlReader r)
    {
        string listName = r["name"];
        if (string.IsNullOrEmpty(listName))
        {
            Console.WriteLine("list without name");
            return null;
        }

        var t = ParseMembers(r);

        string memberCode = string.Format(PacketFormat.memberListFormat,
            FirstCharToUpper(listName),
            FirstCharToLower(listName),
            t.Item1,
            t.Item2,
            t.Item3
        );

        string readCode = string.Format(PacketFormat.readListFormat,
            FirstCharToUpper(listName),
            FirstCharToLower(listName));
        
        string writeCode = string.Format(PacketFormat.writeListFormat,
            FirstCharToLower(listName));

        return new(memberCode, readCode, writeCode);
    }

    public static string ToMemberType(string memberType)
    {
        switch (memberType)
        {
            case "bool":
                return "ToBoolean";
            case "short":
                return "ToInt16";
            case "ushort":
                return "ToUInt16";
            case "int":
                return "ToInt32";
            case "long":
                return "ToInt64";
            case "float":
                return "ToSingle";
            case "double":
                return "ToDouble";
            default:
                return "";
        }
    }

    public static string FirstCharToUpper(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        return input[0].ToString().ToUpper() + input.Substring(1);
    }
    
    public static string FirstCharToLower(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        return input[0].ToString().ToLower() + input.Substring(1);
    }
}