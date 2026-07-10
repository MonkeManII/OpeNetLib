namespace OpeNetLib.Packet
{
    [Obsolete("OpeNetLib's binary serializer has been deprecated. Use an external library instead.")]
    internal class PacketOverflowException(int overflowAmount)
        : Exception($"Assigned {overflowAmount} more bytes than available.") { }
}
