using System.Net;

namespace OpeNetLib.Packet
{
    public readonly struct OriginPacket
    {
        public readonly byte[] Data;
        public readonly IPEndPoint Origin;
        public OriginPacket(IPEndPoint Origin, byte[] Data)
        {
            this.Origin = Origin;
            this.Data = Data;
        }
    }
}
