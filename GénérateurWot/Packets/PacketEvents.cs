using System;

namespace GénérateurWot.Packets
{
    public class PersonalPacket
    {
        public string GuidId { get; set; }
        public object Package { get; set; }
    }

    public class PingPacket
    {
        public string GuidId { get; set; }
    }
    
    public class PacketEvents : EventArgs
    {
        public SimpleClient Sender;
        public SimpleClient Receiver;
        public object Packet;
    }

    public class PersonalPacketEvents : EventArgs
    {
        public SimpleClient Sender;
        public SimpleClient Receiver;
        public PersonalPacket Packet;
    }
    
    public class UserConnectionPacket
    {
        public string UserGuid { get; set; }
        public string[] Users { get; set; }
        public bool IsJoining { get; set; }
    }
}