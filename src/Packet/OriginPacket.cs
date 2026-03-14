using System.Net;

namespace OpeNetLib.Packet
{
    public record struct OriginPacket(IPEndPoint Origin, byte[] Data);
}
