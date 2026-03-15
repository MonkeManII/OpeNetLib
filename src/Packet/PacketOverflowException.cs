namespace OpeNetLib.Packet
{
    internal class PacketOverflowException(int overflowAmount)
        : Exception($"Assigned {overflowAmount} more bytes than available.") { }
}
