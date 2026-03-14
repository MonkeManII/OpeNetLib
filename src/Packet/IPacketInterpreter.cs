namespace OpeNetLib.Packet
{
    internal interface IPacketInterpreter<TConvert>
        where TConvert : IByteConvertable<TConvert>
    {
    }
}
